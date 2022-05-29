using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a female warrior character.
/// </summary>
public class FemaleWarriorCharacter : Character
{
    #region Properties and Fields

    public AttackTrigger leftBattleAxeTrigger;
    public AttackTrigger rightBattleAxeTrigger;

    [Tooltip("Represents the minimum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMinimumDamage;

    [Tooltip("Represents the maximum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMaximumDamage;

    [Tooltip("Represents audio source of the left battle axe.")]
    [SerializeField]
    private AudioSource leftBattleAxeAudioSource;

    [Tooltip("Represents audio source of the right battle axe.")]
    [SerializeField]
    private AudioSource rightBattleAxeAudioSource;

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        leftBattleAxeTrigger.MinimumDamage = battleAxeMinimumDamage;
        leftBattleAxeTrigger.MaximumDamage = battleAxeMaximumDamage;
        rightBattleAxeTrigger.MinimumDamage = battleAxeMinimumDamage;
        rightBattleAxeTrigger.MaximumDamage = battleAxeMaximumDamage;
        if (battleAxeMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Battle axe maximum damage for a female warrior character is set to a non-positive value.");
        }
        if (battleAxeMaximumDamage < battleAxeMinimumDamage)
        {
            Debug.LogWarning("Battle axe maximum damage for a female warrior character is set to a lesser value than the minimum.");
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
        //TODO
        yield return new WaitUntil(() => animationManager.CanDealDamage);
        leftBattleAxeTrigger.IsActive = true;
        rightBattleAxeTrigger.IsActive = true;
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        leftBattleAxeTrigger.IsActive = false;
        rightBattleAxeTrigger.IsActive = false;
    }

    #endregion

    #endregion
}
