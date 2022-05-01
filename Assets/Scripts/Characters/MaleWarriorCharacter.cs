using System.Collections;
using UnityEngine;

public class MaleWarriorCharacter : Character
{
    public AttackTrigger battleAxeTrigger;
    [Tooltip("Represents the minimum damage of the battle axe weapon.")]
    public float battleAxeMinimumDamage;
    [Tooltip("Represents the maximum damage of the battle axe weapon.")]
    public float battleAxeMaximumDamage;

    protected override void OnStart()
    {
        base.OnStart();
        battleAxeTrigger.SetDamage(battleAxeMinimumDamage, battleAxeMaximumDamage);
        if (battleAxeMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Battle axe maximum damage for a male warrior character is set to a non-positive value.");
        }
        if (battleAxeMaximumDamage < battleAxeMinimumDamage)
        {
            Debug.LogWarning("Battle axe maximum damage for a male warrior character is set to a lesser value than the minimum.");
        }
    }

    public override bool TryAttack()
    {
        if (!animationManager.IsInterrupted && !animationManager.IsAttacking)
        {
            animationManager.Attack();
            movementSpeed = 0;
            StartCoroutine(ManageAttackTrigger());
            return true;
        }
        return false;
    }

    private IEnumerator ManageAttackTrigger()
    {
        battleAxeTrigger.IsActive = true;
        yield return new WaitWhile(() => animationManager.IsAttacking);
        battleAxeTrigger.IsActive = false;
    }
}
