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
    private bool previousAttackEnded;

    private bool CanRequestAnotherComboAttack => !animationManager.IsInterrupted && !animationManager.IsInteracting && !animationManager.IsGuarding && !animationManager.IsUsingSkill
        && animationManager.IsAttacking && requestedComboCount < maximumComboCount && stamina > attackStaminaCost * (requestedComboCount - currentComboCount + 1) && previousAttackEnded;

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

    #region Start

    public override void StartAttack(Vector3 attackPoint, Character target = null)
    {
        if (!animationManager.IsAttacking)
        {
            base.StartAttack(attackPoint, target);
        }
        else
        {
            RequestAnotherComboAttack();
        }
    }

    [PunRPC]
    public void RequestAnotherComboAttack()
    {
        if (CanRequestAnotherComboAttack || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(RequestAnotherComboAttack), RpcTarget.Others);
            }
            requestedComboCount += 1;
            previousAttackEnded = false;
        }
    }

    #endregion

    #region End

    public override void EndAttack(Vector3 attackPoint, Character target = null)
    {
        EndPreviousAttack();
    }

    [PunRPC]
    public void EndPreviousAttack()
    {
        if (animationManager.IsAttacking || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(EndPreviousAttack), RpcTarget.Others);
            }
            previousAttackEnded = true;
        }
    }

    #endregion

    #region Without Target

    protected override void OnAttackWithoutTarget(Vector3 attackTarget)
    {
        base.OnAttackWithoutTarget(attackTarget);
        currentComboCount = 0;
        requestedComboCount = 0;
        previousAttackEnded = false;
        StartCoroutine(ManageComboRequests());
        StartCoroutine(ManageAttackTrigger());
    }

    private IEnumerator ManageComboRequests()
    {
        femaleWarriorAnimationManager.SetContinueComboAttack(false);
        while (animationManager.IsAttacking)
        {
            yield return new WaitUntil(() => requestedComboCount > currentComboCount || !animationManager.IsAttacking);
            if (animationManager.IsAttacking)
            {
                femaleWarriorAnimationManager.SetContinueComboAttack(true);
            }
            yield return new WaitWhile(() => animationManager.CanDealDamage); //wait for the end of the current attack animation

            yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsAttacking);
            if (animationManager.IsAttacking)
            {
                stamina -= attackStaminaCost;
                currentComboCount += 1;
            }
            femaleWarriorAnimationManager.SetContinueComboAttack(false);
        }
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

    #endregion

    #region With Target

    protected override void OnAttackChaseTarget()
    {
        base.OnAttackChaseTarget();
        currentComboCount = 0;
        requestedComboCount = 0;
        previousAttackEnded = false;
        StartCoroutine(ManageComboRequests());
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
                leftBattleAxeTrigger.ForceAttackAfterDelay(target, BasicAttackForceDelay); // TODO... which axe?
            }
            yield return new WaitWhile(() => animationManager.CanDealDamage);
        }
    }

    #endregion

    #endregion

    #region Take Damage

    protected override void OnTakeDamage()
    {
        base.OnTakeDamage();
        femaleWarriorAnimationManager.SetContinueComboAttack(false);
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
