using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male warrior character.
/// </summary>
public class MaleWarriorCharacter : WarriorCharacter
{
    #region Properties and Fields
    public override CharacterFightingStyle FightingStyle => CharacterFightingStyle.Heavy;

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

    #region Skills

    #region Leap Attack

    protected override float JumpingTime => .5f;

    #endregion

    #region Whirlwind

    protected override float WhirlwindStartAnimationDelay => .5f;
    protected override float WhirlwindEndAnimationDelay => .3f;
    protected override float WhirlwindAttackTriggerPeriod => .5f;

    #endregion

    #region Ground Slam

    protected override float GroundSlamStartDelay => 0.9f;

    #endregion

    #endregion

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

    #region Skills

    #region Leap Attack

    protected override void OnLeapAttack(Vector3 attackTarget)
    {
        StartCoroutine(ManageLeapAttackAttackTrigger());
    }


    private IEnumerator ManageLeapAttackAttackTrigger()
    {
        yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsUsingSkill);
        if (animationManager.CanDealDamage)
        {
            battleAxeTrigger.IsActive = true;
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
