# Stadium Ludus
Stadium Ludus is a small, point and click style multiplayer game where you can compete with others in an arena.
<br> 
This game was made in Unity.

## Gameplay
Opening the game, a player has to create a room where the others can join.
<br>
Only players in the same room are able to play with each other.
<br>
After joining, they have to select a character.
<br>
The players can choose from two *classes* and from two *fighting styles* for their character.
<br>
* **Barbarian** class characters are focused on close ranged combat while **Rangers** fight from afar.
* Characters with **heavy fighting style** strike strong but slow. **Light fighting styles** are quick but less powerful.

After everybody chose their character, a game round starts.
<br>
Each character can attack, guard, sprint and use their skills to achieve victory.
<br>
They can also interact with certain objects of the environment to gain temporary boosts.
<br>
The round ends when a character wins by defeating everybody else in the arena.
<br>
When the round has finished, players can ask for a rematch.

## Input

The characters are controlled via mouse and keyboard.
<br>
Since it's a point and click game, the mouse controls where the character goes. Attacking and rotating towards a direction while using skills or guarding are also determined by the position of the mouse. Using certain keyboard buttons the player can use the character's skills, guard and sprint.

Characters can attack by clicking on an enemy. If the enemy is too far away to fire an attack or skill, the character will try to chase it and fire when it's close enough.

Guarding only blocks attacks coming from the frontal direction of the character and there are even some skills which cannot be guarded at all.

# How it's made
In this section I will briefly describe the decisions I made throughout making the game.

## Movement
Whenever the player clicks on the ground, a new destination is set for their character. The path to this destination is calculated using Unity's built-in **NavMeshAgent** component, however the character is not directly controlled by it. For the sake of having more control over the character's movement, I implemented a physics-based character controller which uses the character's **RigidBody** component.
The characters can only be navigated to relatively flat parts of the environment, marked as walkable **NavMesh** areas.
The characters can also jump by using either the **Dash** or the **Leap Attack** skill. The target destination of the jump is truncated so that the character always lands on a walkable area of the environment.

## Combat System
Characters can attack, guard and use skills to fight. 

Each character has a **HitBox Collider** component which determines the volume where the character can take damage.
Every weapon has a **Trigger Collider** component which is responsible for dealing damage to enemies upon colliding with their **HitBox Colliders**.

Whenever the player hovers over an enemy, it gets highlighted so the player can be sure that it's set as the target of their attack.

When an enemy is targeted and it is close enough to be attacked, it's always hit regardless of whether their **HitBox Collider** has collided with a **Trigger Collider** or not. This ensures that the player doesn't have to position its character precisely towards the enemy once they were selected as a target.
When there is no target selected however, the trigger collisions do matter and determine whether the attack would potentially deal damage or not.

**Ranger** characters use projectiles to attack their enemies. If there is a target set to the projectile it will always fly towards their direction so it cannot be avoided by moving away, however environmental objects and other characters can stop it if they collide.

## Networking
I used **Photon Pun** to implement the multiplayer part of the game. 
Photon uses a **Room - Player** structure where **Players** in the same **Room** can play together.
Due to lack of server infrastructure, I implemented the multiplayer based on a player-hosted client/server architecture instead of using a dedicated server. I attempted to decrease the effects of latency using validation and certain rules.

To ensure that the local player always feels in control of their character, I have made players see their own character's actions instantly without latency, while other characters are synced through the network with potential latency.

The positions of the characters are synced via the **PhotonViewTransform** component, all the other actions are synced via **RPC methods**.

For projectiles - being fast moving objects -, their **Transforms** are not constantly synced, only their start and end positions are, so they look smooth for all players.

Dealing damage is handled the following way:
* The local character initiates an attack.
* If that attack hits a valid target, then an **RPC** is sent to the enemy character's owner with the info of the hit along with a timestamp.
* The hit is then validated by checking whether the **HitBox Collider** of the character collided with the weapon's **Trigger Collider** at the time or not.
* Finally, the result of the validation is sent back to every player.

Whevener the latency differs too much between two players, they are not able to damage each other so that they remain safe and can continue playing when their connection has been restored.