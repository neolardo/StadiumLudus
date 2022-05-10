using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male warrior character.
/// </summary>
public class MaleWarriorCharacter : Character
{
    #region Properties and Fields

    public AttackTrigger battleAxeTrigger;
    [Tooltip("Represents the minimum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMinimumDamage;
    [Tooltip("Represents the maximum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMaximumDamage;

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        battleAxeTrigger.MinimumDamage = battleAxeMinimumDamage;
        battleAxeTrigger.MaximumDamage = battleAxeMaximumDamage;
        if (battleAxeMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Battle axe maximum damage for a male warrior character is set to a non-positive value.");
        }
        if (battleAxeMaximumDamage < battleAxeMinimumDamage)
        {
            Debug.LogWarning("Battle axe maximum damage for a male warrior character is set to a lesser value than the minimum.");
        }
    }

    #region Attack

    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAttackTrigger());
        StartCoroutine(RotateToAttackDirection(attackTarget));
    }

    private IEnumerator ManageAttackTrigger()
    {
        yield return new WaitUntil(() => animationManager.CanDealDamage);
        battleAxeTrigger.IsActive = true;
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        battleAxeTrigger.IsActive = false;
    }

    #endregion

    #endregion
}
