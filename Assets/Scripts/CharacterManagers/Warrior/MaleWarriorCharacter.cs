using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male warrior character.
/// </summary>
public class MaleWarriorCharacter : WarriorCharacter
{
    #region Properties and Fields
    public override CharacterFightingStyle FightingStyle => CharacterFightingStyle.Heavy;

    #region Attack

    [Header("BattleAxe")]
    [Tooltip("Represents the attack trigger of the battle axe weapon.")]
    public AttackTrigger battleAxeTrigger;

    [Tooltip("Represents the minimum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMinimumDamage;

    [Tooltip("Represents the maximum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMaximumDamage;

    [Tooltip("Represents audio source of the battle axe.")]
    [SerializeField]
    private AudioSource battleAxeAudioSource;

    protected override float BasicAttackForceDelay => 0f;

    #endregion

    #region Skills

    #region Leap Attack

    protected override float JumpingTime => .65f;

    protected override float LeapAttackForceDelay => 0.5f;


    #endregion

    #region Whirlwind

    protected override float WhirlwindStartAnimationDelay => .3f;
    protected override float WhirlwindEndAnimationDelay => .3f;
    protected override float WhirlwindAttackTriggerPeriod => .5f;

    #endregion

    #region Ground Slam

    protected override float GroundSlamStartDelay => 0.7f;


    #endregion

    #endregion

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
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

    #region Without Target

    protected override void OnAttackWithoutTarget(Vector3 attackTarget)
    {
        base.OnAttackWithoutTarget(attackTarget);
        StartCoroutine(ManageAttackTrigger());
    }

    private IEnumerator ManageAttackTrigger()
    {
        yield return new WaitUntil(() => warriorAnimationManager.CanDealDamage || !warriorAnimationManager.IsAttacking);
        if (warriorAnimationManager.CanDealDamage)
        {
            battleAxeTrigger.IsActive = true;
            AudioManager.Instance.PlayOneShotSFX(battleAxeAudioSource, SFX.Slash);
        }
        yield return new WaitWhile(() => warriorAnimationManager.CanDealDamage);
        battleAxeTrigger.IsActive = false;
    }

    #endregion

    #region With Target

    protected override void OnAttackChaseTarget()
    {
        base.OnAttackChaseTarget();
        StartCoroutine(ManageAttackTrigger());
        StartCoroutine(ManageForceAttack());
    }

    private IEnumerator ManageForceAttack()
    {
        yield return new WaitUntil(() => (battleAxeTrigger.IsActive && warriorAnimationManager.CanDealDamage) || !warriorAnimationManager.IsAttacking );
        var target = chaseTarget == null ? null : chaseTarget.GetComponent<Character>();
        if (target != null && battleAxeTrigger.IsActive && warriorAnimationManager.CanDealDamage)
        {
            battleAxeTrigger.ForceAttackAfterDelay(target, BasicAttackForceDelay);
        }
    }

    #endregion

    #endregion

    #region Skills

    #region Leap Attack

    protected override void OnLeapAttack()
    {
        StartCoroutine(ManageLeapAttackAttackTrigger());
    }

    private IEnumerator ManageLeapAttackAttackTrigger()
    {
        yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsUsingSkill);
        if (animationManager.CanDealDamage)
        {
            battleAxeTrigger.IsActive = true;
            if (leapAttackTarget != null)
            {
                battleAxeTrigger.ForceAttackAfterDelay(leapAttackTarget, LeapAttackForceDelay);
            }
            AudioManager.Instance.PlayOneShotSFX(battleAxeAudioSource, SFX.Slash);
        }
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        battleAxeTrigger.IsActive = false;
    }

    #endregion

    #region Whirlwind

    protected override void OnWhirlwind()
    {
        StartCoroutine(ManageWhirlwindAttackTrigger());
    }

    private IEnumerator ManageWhirlwindAttackTrigger()
    {
        yield return new WaitUntil(() => warriorAnimationManager.CanDealDamage || !warriorAnimationManager.IsWhirlwindOnGoing);
        while (warriorAnimationManager.IsWhirlwindOnGoing && warriorAnimationManager.CanDealDamage)
        {
            battleAxeTrigger.IsActive = true;
            float elapsedTime = 0;
            while (elapsedTime < WhirlwindAttackTriggerPeriod && warriorAnimationManager.IsWhirlwindOnGoing && warriorAnimationManager.CanDealDamage)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            battleAxeTrigger.IsActive = false;
        }
    }

    #endregion

    #endregion

    #endregion
}
