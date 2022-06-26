using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCharacterController : MonoBehaviour
{
    private Character character;
    public GameObject waypoint1;
    public GameObject waypoint2;
    public bool walkBetweenWaypoints;
    public bool attack;
    public bool guard;
    public bool die;
    private int nextWaypoint = 0;
    private const float positionThreshold = 0.5f;


    void Start()
    {
        character = GetComponent<Character>();
        if (waypoint1 == null)
        { 
            Debug.LogWarning("Waypoint1 for a test character controller is null.");
        }
        if (waypoint2 == null)
        {
            Debug.LogWarning("Waypoint2 for a test character controller is null.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (character.IsAlive)
        {
            CheckInputParameters();
        }
    }

    private void CheckInputParameters()
    {
        if (die)
        {
            //character.TryTakeDamage(200, HitDirection.Back);
            die = false;
        }
        if (attack)
        {
            character.TryAttack(character.transform.position + Random.insideUnitSphere);
            attack = false;
        }
        if (guard)
        {
            character.StartGuarding();
        }
        else
        {
            character.EndGuarding();
        }
        if (walkBetweenWaypoints)
        {
            var nextWaypointPosition = waypoint1.transform.position;
            var otherWaypointPosition = waypoint2.transform.position;
            if (nextWaypoint == 1)
            {
                nextWaypointPosition = waypoint2.transform.position;
                otherWaypointPosition = waypoint1.transform.position;
            }
            if ((character.transform.position - nextWaypointPosition).magnitude < positionThreshold)
            {
                nextWaypoint = nextWaypoint == 1 ? 0 : 1;
                var temp = nextWaypointPosition;
                nextWaypointPosition = otherWaypointPosition;
                otherWaypointPosition = temp;
            }
            character.MoveTo(nextWaypointPosition);
        }
    }
}
