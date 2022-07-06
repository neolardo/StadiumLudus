using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// An abstract class for every warrior character.
/// </summary>
public abstract class WarriorCharacter : Character
{
    #region Properties and Fields

    protected WarriorAnimationManager warriorAnimationManager;
    public override CharacterClass Class => CharacterClass.Barbarian;

    #region Skills

    #region Leap Attack

    protected const int LeapAttackSkillNumber = 1;

    [Header("Leap Attack")]
    [Tooltip("Represents jump force of the leap attack.")]
    [SerializeField]
    private float leapAttackJumpForce = 250f;

    [Tooltip("Represents maximum distance of the leap attack.")]
    [SerializeField]
    private float leapAttackMaximumDistance = 3.5f;

    [Tooltip("Represents cooldown of the leap attack skill in seconds.")]
    [SerializeField]
    private float leapAttackCooldown = 5f;

    [Tooltip("Represents the stamina cost of the leap attack skill.")]
    [SerializeField]
    private float leapAttackStaminaCost = 15f;

    private bool IsLeapAttackAvailable { get; set; } = true;

    private float jumpingTimeDelta;
    private Vector3 jumpOrigin;
    private Vector3 jumpTarget;

    private Transform leapAttackTarget;
    private bool CanLeapAttack => IsAlive && IsLeapAttackAvailable && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill && stamina > leapAttackStaminaCost;

    protected abstract float JumpingTime { get; }

    #endregion

    #region Whirlwind

    private const int WhirlwindSkillNumber = 2;

    [Header("Whirlwind")]
    [Tooltip("Represents the rotation speed of the whirlwind in degrees/second.")]
    [SerializeField]
    private float whirlwindRotationSpeed = 920f;

    [Tooltip("Represents the amount of stamina is drained while using the whirlwind per seconds.")]
    [SerializeField]
    private float whirlwindStaminaCost = 35f;

    private bool CanWhirlwind => IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill;

    protected abstract float WhirlwindStartAnimationDelay { get; }
    protected abstract float WhirlwindEndAnimationDelay { get; }
    protected abstract float WhirlwindAttackTriggerPeriod { get; }

    #endregion

    #region Ground Slam

    private const int GroundSlamSkillNumber = 3;

    [Header("Ground Slam")]
    [SerializeField]
    private GroundSlamManager groundSlamManager;

    [SerializeField]
    protected AttackTrigger groundSlamAttackTrigger;

    [Tooltip("Represents the stamina cost of the ground slam skill.")]
    [SerializeField]
    private float groundSlamStaminaCost = 20f;

    [Tooltip("Represents maximum distance of the ground slam.")]
    [SerializeField]
    private float groundSlamMaximumDistance = 3.5f;

    [Tooltip("Represents minimum damage of the ground slam attack.")]
    [SerializeField]
    private float groundSlamMinimumDamage;

    [Tooltip("Represents maximum damage of the ground slam attack.")]
    [SerializeField]
    private float groundSlamMaximumDamage;

    [Tooltip("Represents cooldown of the ground slam skill in seconds.")]
    [SerializeField]
    private float groundSlamCooldown = 5f;
    private bool IsGroundSlamAvailable { get; set; } = true;
    private bool CanGroundSlam => IsAlive && IsGroundSlamAvailable && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill && stamina > groundSlamStaminaCost;
    protected abstract float GroundSlamStartDelay { get; }

    private const float groundSlamRockAttackTriggerDuration = .5f;

    #endregion  

    #endregion

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
        groundSlamAttackTrigger.MinimumDamage = groundSlamMinimumDamage;
        groundSlamAttackTrigger.MaximumDamage = groundSlamMaximumDamage;
        if (groundSlamMinimumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Ground slam maximum damage for a warrior character is set to a non-positive value.");
        }
        if (groundSlamMaximumDamage < groundSlamMinimumDamage)
        {
            Debug.LogWarning("Ground slam maximum damage for a warrior character is set to a lesser value than the minimum.");
        }
        warriorAnimationManager = animationManager as WarriorAnimationManager;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateLeapAttackJumping();
    }

    #region Skills

    public override void StartSkill(int skillNumber, Vector3 clickPosition, Character target)
    {
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                LeapAttack(clickPosition, target);
                break;
            case WhirlwindSkillNumber:
                StartWhirlwind();
                break;
            case GroundSlamSkillNumber:
                GroundSlam(clickPosition);
                break;
            default:
                Debug.LogWarning("Invalid skill number for a warrior character.");
                break;
        }
    }

    public override void EndSkill(int skillNumber)
    {
        if (skillNumber == WhirlwindSkillNumber)
        {
            EndWhirlwind();
        }
    }

    private IEnumerator ManageCooldown(int skillNumber)
    {
        float cooldown = 0;
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                cooldown = leapAttackCooldown;
                break;
            case GroundSlamSkillNumber:
                cooldown = groundSlamCooldown;
                break;
            default:
                Debug.Log("Invalid skill number for a warrior character.");
                break;
        }
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(skillNumber, cooldown);
        }
        yield return new WaitForSeconds(cooldown);
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                IsLeapAttackAvailable = true;
                break;
            case GroundSlamSkillNumber:
                IsGroundSlamAvailable = true;
                break;
            default:
                Debug.Log("Invalid skill number for a warrior character.");
                break;
        }
    }

    #region Leap Attack

    [PunRPC]
    public void LeapAttack(Vector3 attackTarget, Character target)
    {
        if (CanLeapAttack || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(LeapAttack), RpcTarget.Others, attackTarget, null);
            }
            leapAttackTarget = target == null ?  null : target.transform;
            jumpingTimeDelta = 0;
            IsLeapAttackAvailable = false;
            jumpOrigin = rb.position;
            stamina -= leapAttackStaminaCost;
            forceRotation = true;
            jumpTarget = ClampPointInsideRange(attackTarget, leapAttackMaximumDistance);
            SetRotationTarget(jumpTarget);
            MoveTo(jumpTarget);
            warriorAnimationManager.LeapAttack();
            StartCoroutine(ManageCooldown(LeapAttackSkillNumber));
            StartCoroutine(AddJumpForce());
            StartCoroutine(ResetDestinationAfterLeap());
            if (PhotonView.IsMine && leapAttackTarget != null)
            {
                StartCoroutine(ManageJumpAndRotationTarget());
            }
            OnLeapAttack();
        }
        else if (PhotonView.IsMine && characterUI != null)
        {
            characterUI.OnCannotPerformSkill(stamina < leapAttackStaminaCost, !IsLeapAttackAvailable, LeapAttackSkillNumber);
        }
    }

    protected abstract void OnLeapAttack();

    private void UpdateLeapAttackJumping()
    {
        if (warriorAnimationManager.IsJumping && jumpingTimeDelta < JumpingTime)
        {
            var tempPosition = Vector3.Lerp(jumpOrigin, jumpTarget, jumpingTimeDelta / JumpingTime);
            rb.MovePosition(new Vector3(tempPosition.x, rb.position.y, tempPosition.z));
            jumpingTimeDelta += Time.fixedDeltaTime;
        }
    }

    private IEnumerator ManageJumpAndRotationTarget()
    {
        yield return new WaitUntil(() => warriorAnimationManager.IsJumping);
        while (warriorAnimationManager.IsJumping)
        {
            var tempTarget = leapAttackTarget.transform.position;
            if ((tempTarget - jumpOrigin).magnitude > leapAttackMaximumDistance)
            {
                tempTarget = jumpOrigin + (tempTarget - jumpOrigin).normalized * leapAttackMaximumDistance;
            }
            SetJumpTarget(tempTarget);
            SetRotationTarget(leapAttackTarget.transform.position);
            yield return null;
        }
    }

    [PunRPC]
    public void SetJumpTarget(Vector3 jumpTarget)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(SetJumpTarget), RpcTarget.Others, jumpTarget);
        }
        this.jumpTarget = jumpTarget;
    }

    private IEnumerator AddJumpForce()
    {
        yield return new WaitUntil(() => warriorAnimationManager.IsJumping);
        rb.AddForce(leapAttackJumpForce * Vector3.up, ForceMode.Impulse);
    }

    private IEnumerator ResetDestinationAfterLeap()
    {
        yield return new WaitUntil(() => warriorAnimationManager.IsJumping);
        yield return new WaitWhile(() => warriorAnimationManager.IsJumping);
        forceRotation = false;
        leapAttackTarget = null;
        ClearDestination();
    }

    #endregion

    #region Whirlwind

    [PunRPC]
    public void StartWhirlwind()
    {
        if (CanWhirlwind || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(StartWhirlwind), RpcTarget.Others);
            }
            warriorAnimationManager.StartWhirlwind();
            AudioManager.Instance.PlaySFX(characterAudioSource, SFX.Whirlwind);
            characterAudioSource.loop = true;
            StartCoroutine(ManageWhirlwindStaminaDrain());
            StartCoroutine(ManageWhirlwindRotation());
            OnWhirlwind();
        }
    }

    [PunRPC]
    public void EndWhirlwind()
    {
        if ((IsAlive && warriorAnimationManager.IsWhirlwindOnGoing) || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(EndWhirlwind), RpcTarget.Others);
            }
            AudioManager.Instance.Stop(characterAudioSource);
            characterAudioSource.loop = false;
            warriorAnimationManager.EndWhirlwind();
        }
    }

    protected abstract void OnWhirlwind();

    private IEnumerator ManageWhirlwindRotation()
    {
        yield return new WaitForSeconds(WhirlwindStartAnimationDelay);
        while (warriorAnimationManager.IsWhirlwindOnGoing)
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(whirlwindRotationSpeed * Time.deltaTime, Vector3.up));
            yield return null;
        }
        float elapsedTime = 0;
        while (elapsedTime < WhirlwindEndAnimationDelay)
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(Mathf.Lerp(whirlwindRotationSpeed, 0, elapsedTime / WhirlwindEndAnimationDelay) * Time.deltaTime, Vector3.up));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ManageWhirlwindStaminaDrain()
    {
        var staminaDelta = Time.deltaTime * whirlwindStaminaCost;
        while (stamina > staminaDelta && warriorAnimationManager.IsWhirlwindOnGoing)
        {
            stamina -= staminaDelta;
            yield return null;
            staminaDelta = Time.deltaTime * whirlwindStaminaCost;
        }
        if (warriorAnimationManager.IsWhirlwindOnGoing)
        {
            EndWhirlwind();
        }
    }

    #endregion

    #region Ground Slam

    [PunRPC]
    public void GroundSlam(Vector3 attackTarget)
    {
        if (CanGroundSlam || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(GroundSlam), RpcTarget.Others, attackTarget);
            }
            attackTarget = ClampPointInsideRange(attackTarget, groundSlamMaximumDistance);
            IsGroundSlamAvailable = false;
            SetRotationTarget(attackTarget);
            forceRotation = true;
            warriorAnimationManager.GroundSlam();
            StartCoroutine(ManageCooldown(GroundSlamSkillNumber));
            groundSlamManager.Fire(attackTarget, GroundSlamStartDelay);
            stamina -= groundSlamStaminaCost;
            OnGroundSlam(attackTarget);
            StartCoroutine(EndForceRotateAfterUsingSkill());
        }
        else if (PhotonView.IsMine && characterUI!=null)
        {
            characterUI.OnCannotPerformSkill(stamina < groundSlamStaminaCost, !IsGroundSlamAvailable, GroundSlamSkillNumber);
        }
    }

    protected virtual void OnGroundSlam(Vector3 attackTarget)
    {
        StartCoroutine(ManageGroundSlamAttackTrigger());
    }
    private IEnumerator ManageGroundSlamAttackTrigger()
    {
        yield return new WaitUntil(() => groundSlamManager.IsRockVisible);
        groundSlamAttackTrigger.IsActive = true;
        //audio?
        yield return new WaitForSeconds(groundSlamRockAttackTriggerDuration);
        groundSlamAttackTrigger.IsActive = false;
    }

    private IEnumerator EndForceRotateAfterUsingSkill()
    {
        yield return new WaitUntil(() => !animationManager.IsUsingSkill);
        forceRotation = false;
    }

    #endregion

    #endregion

    #endregion
}
