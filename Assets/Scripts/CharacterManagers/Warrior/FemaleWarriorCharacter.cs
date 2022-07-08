using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female warrior character.
/// </summary>
public class FemaleWarriorCharacter : WarriorCharacter
{
    #region Properties and Fields

    private FemaleWarriorAnimationManager femaleWarriorAnimationManager;
    public override CharacterFightingStyle FightingStyle => CharacterFightingStyle.Light;

    [Header("Battle Axe")]
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

    protected override float BasicAttackForceDelay => 0.3f;

    #region Combo Attack

    private int currentComboCount = 0;
    private int requestedComboCount = 0;
    private const int maximumComboCount = 2;
    private const float comboDelaySeconds = .1f;
    private bool comboDelayElapsed;

    private bool CanRequestAnotherComboAttack => !animationManager.IsInterrupted && !animationManager.IsInteracting && !animationManager.IsGuarding && !animationManager.IsUsingSkill
        && animationManager.IsAttacking && !femaleWarriorAnimationManager.IsContinueAttackRequested && currentComboCount < 2 && stamina > attackStaminaCost && comboDelayElapsed;

    #endregion

    #region Skills

    #region Leap Attack

    protected override float JumpingTime => .65f;

    protected override float LeapAttackForceDelay => 0.5f;


    #endregion

    #region Whirlwind

    protected override float WhirlwindStartAnimationDelay => .5f;
    protected override float WhirlwindEndAnimationDelay => .3f;
    protected override float WhirlwindAttackTriggerPeriod => .5f;

    #endregion

    #region Ground Slam

    protected override float GroundSlamStartDelay => .7f;

    #endregion

    #endregion

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
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
        femaleWarriorAnimationManager = warriorAnimationManager as FemaleWarriorAnimationManager;
    }

    #region Attack

    public override bool TryAttack(Vector3 attackTarget)
    {
        if (CanAttack)
        {
            StartComboAttack(attackTarget);
            return true;
        }
        else if (animationManager.IsAttacking && comboDelayElapsed && requestedComboCount < maximumComboCount)
        {
            StartCoroutine(WaitForComboDelay());
            requestedComboCount += 1;
            return true;
        }
        return false;
    }

    [PunRPC]
    public void StartComboAttack(Vector3 attackTarget)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(StartComboAttack), RpcTarget.Others, attackTarget);
        }
        OnAttack(attackTarget);
        currentComboCount = 0;
        requestedComboCount = 0;
        StartCoroutine(ManageComboRequests());
        StartCoroutine(WaitForComboDelay());
    }

    private IEnumerator WaitForComboDelay()
    {
        comboDelayElapsed = false;
        yield return new WaitForSeconds(comboDelaySeconds);
        comboDelayElapsed = true;
    }

    private IEnumerator ManageComboRequests()
    {
        //TODO
        femaleWarriorAnimationManager.SetContinueComboAttack(true);
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        stamina -= attackStaminaCost;
        yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsAttacking);
        femaleWarriorAnimationManager.SetContinueComboAttack(false);
    }


    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAttackTrigger());
    }

    private IEnumerator ManageAttackTrigger()
    {
        while (animationManager.IsAttacking)
        {
            yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsAttacking);
            if (animationManager.CanDealDamage)
            {
                leftBattleAxeTrigger.IsActive = true;
                rightBattleAxeTrigger.IsActive = true;
            }
            yield return new WaitWhile(() => animationManager.CanDealDamage);
            leftBattleAxeTrigger.IsActive = false;
            rightBattleAxeTrigger.IsActive = false;
        }
    }


    protected override void OnAttackChaseTarget()
    {
        base.OnAttackChaseTarget();
        StartCoroutine(ManageAttackTrigger());
        StartCoroutine(ManageForceAttack());
    }

    private IEnumerator ManageForceAttack()
    {
        var target = chaseTarget == null ? null : chaseTarget.GetComponent<Character>();
        while (chaseTarget != null && animationManager.IsAttacking)
        {
            yield return new WaitUntil(() => (animationManager.CanDealDamage && leftBattleAxeTrigger.IsActive && rightBattleAxeTrigger.IsActive) || !animationManager.IsAttacking);
            if (animationManager.CanDealDamage && leftBattleAxeTrigger.IsActive && rightBattleAxeTrigger.IsActive)
            {
                leftBattleAxeTrigger.ForceAttackAfterDelay(target, BasicAttackForceDelay); // TODO...
            }
            yield return new WaitWhile(() => animationManager.CanDealDamage);
        }
    }

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
        if(animationManager.CanDealDamage)
        {
            leftBattleAxeTrigger.IsActive = true;
            rightBattleAxeTrigger.IsActive = true;
            if (leapAttackTarget != null)
            {
                leftBattleAxeTrigger.ForceAttackAfterDelay(leapAttackTarget, LeapAttackForceDelay);
                rightBattleAxeTrigger.ForceAttackAfterDelay(leapAttackTarget, LeapAttackForceDelay);
            }
            AudioManager.Instance.PlayOneShotSFX(rightBattleAxeAudioSource, SFX.Slash);
        }
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        leftBattleAxeTrigger.IsActive = false;
        rightBattleAxeTrigger.IsActive = false;
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
            rightBattleAxeTrigger.IsActive = true;
            leftBattleAxeTrigger.IsActive = true;
            float elapsedTime = 0;
            while (elapsedTime < WhirlwindAttackTriggerPeriod && warriorAnimationManager.IsWhirlwindOnGoing && warriorAnimationManager.CanDealDamage)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            rightBattleAxeTrigger.IsActive = false;
            leftBattleAxeTrigger.IsActive = false;
        }
    }

    #endregion

    #endregion

    #endregion
}
