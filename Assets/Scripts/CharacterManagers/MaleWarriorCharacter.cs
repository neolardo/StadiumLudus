using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male warrior character.
/// </summary>
public class MaleWarriorCharacter : Character
{
    #region Properties and Fields

    private MaleWarriorAnimationManager maleWarriorAnimationManager;

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

    private const int LeapAttackSkillNumber = 1;

    private const float jumpingTime = 0.5f;

    private bool IsLeapAttackAvailable { get; set; } = true;

    private bool IsLeapAttackFirstFrame { get; set; }

    private Vector3 jumpTarget;

    private Vector3 currentJumpDelta;
    private bool CanLeapAttack => IsAlive && IsLeapAttackAvailable && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill;

    #endregion

    #region Whirlwind


    [Header("Whirlwind")]
    [Tooltip("Represents the rotation speed of the whirlwind in degrees/second.")]
    [SerializeField]
    private float whirlwindRotationSpeed = 920f;

    [Tooltip("Represents the amount of stamina is drained while using the whirlwind per seconds.")]
    [SerializeField]
    private float whirlwindStaminaCost = 50f;

    private const float whirlwindStartAnimationDelay = 0.5f;
    private const float whirlwindEndAnimationDelay = 0.3f;

    private const int WhirlwindSkillNumber = 2;

    private bool CanWhirlwind => IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill;

    #endregion

    #region Ground Slam

    private const int GroundSlamSkillNumber = 3;

    #endregion  

    #endregion

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        maleWarriorAnimationManager = animationManager as MaleWarriorAnimationManager;
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateLeapAttackJumping();
    }

    #region Attack

    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAttackTrigger());
    }

    private IEnumerator ManageAttackTrigger()
    {
        yield return new WaitUntil(() => maleWarriorAnimationManager.CanDealDamage);
        battleAxeTrigger.IsActive = true;
        AudioManager.Instance.PlayOneShotSFX(battleAxeAudioSource, SFX.Slash, 0);
        yield return new WaitWhile(() => maleWarriorAnimationManager.CanDealDamage);
        battleAxeTrigger.IsActive = false;
    }

    #endregion

    #region Skills

    public override void StartSkill(int skillNumber, Vector3 clickPosition)
    {
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                LeapAttack(clickPosition);
                break;
            case WhirlwindSkillNumber:
                StartWhirlwind();
                break;
            case GroundSlamSkillNumber:
                GroundSlam();
                break;
            default:
                Debug.LogWarning("Invalid skill number for a male warrior character.");
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

    #region Leap Attack

    private void LeapAttack(Vector3 attackTarget)
    {
        if (CanLeapAttack)
        {
            if ((attackTarget - rb.position).magnitude > leapAttackMaximumDistance)
            {
                var edgePoint = rb.position + (attackTarget - rb.position).normalized * leapAttackMaximumDistance;
                var raycastPoint = edgePoint + Vector3.up * 5;
                Ray ray = new Ray(raycastPoint, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
                {
                    edgePoint = hit.point;
                }
                attackTarget = edgePoint;
            }
            IsLeapAttackFirstFrame = true;
            IsLeapAttackAvailable = false;
            jumpTarget = attackTarget;
            SetRotationTarget(attackTarget);
            MoveTo(jumpTarget);
            maleWarriorAnimationManager.LeapAttack();
            StartCoroutine(ManageAttackTrigger());
            StartCoroutine(ManageLeapAttackCooldown());
            StartCoroutine(ResetDestinationAfterLeap());
        }
    }

    private void UpdateLeapAttackJumping()
    {
        if (maleWarriorAnimationManager.IsJumping)
        {
            if (IsLeapAttackFirstFrame)
            {
                rb.AddForce(Vector3.up * leapAttackJumpForce, ForceMode.Impulse);
                currentJumpDelta = (new Vector3(jumpTarget.x, rb.position.y, jumpTarget.z) - rb.position) / (jumpingTime * 50);
                IsLeapAttackFirstFrame = false;
            }
            else
            {
                rb.MovePosition(rb.position + currentJumpDelta);
            }
        }
    }

    private IEnumerator ManageLeapAttackCooldown()
    {
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(LeapAttackSkillNumber, leapAttackCooldown);
        }
        yield return new WaitForSeconds(leapAttackCooldown);
        IsLeapAttackAvailable = true;
    }

    private IEnumerator ResetDestinationAfterLeap()
    {
        yield return new WaitUntil(() => maleWarriorAnimationManager.IsJumping);
        yield return new WaitWhile(() => maleWarriorAnimationManager.IsJumping);
        ClearDestination();
    }

    #endregion

    #region Whirlwind

    private void StartWhirlwind()
    {
        if (CanWhirlwind)
        {
            maleWarriorAnimationManager.StartWhirlwind();
            StartCoroutine(ManageWhirlwindStaminaDrain());
            StartCoroutine(ManageWhirlwindRotation());
        }
    }

    private void EndWhirlwind()
    {
        if (IsAlive && maleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            maleWarriorAnimationManager.EndWhirlwind();
        }
    }

    private IEnumerator ManageWhirlwindRotation()
    {
        yield return new WaitForSeconds(whirlwindStartAnimationDelay);
        while (maleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(whirlwindRotationSpeed * Time.deltaTime, Vector3.up));
            yield return null;
        }
        float elapsedTime = 0;
        while (elapsedTime < whirlwindEndAnimationDelay)
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(Mathf.Lerp(whirlwindRotationSpeed, 0, elapsedTime / whirlwindEndAnimationDelay) * Time.deltaTime, Vector3.up));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ManageWhirlwindStaminaDrain()
    {
        var staminaDelta = Time.deltaTime * whirlwindStaminaCost;
        while (stamina > staminaDelta && maleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            stamina -= staminaDelta;
            yield return null;
            staminaDelta = Time.deltaTime * whirlwindStaminaCost;
        }
        if (maleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            EndWhirlwind();
        }
    }
    #endregion

    #region Ground Slam

    private void GroundSlam()
    {
        if (IsAlive && !maleWarriorAnimationManager.IsInterrupted && !maleWarriorAnimationManager.IsAttacking && !maleWarriorAnimationManager.IsGuarding)
        {

        }
    }

    #endregion

    #endregion

    #endregion
}
