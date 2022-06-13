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

    #endregion

    #region Ground Slam

    private const int GroundSlamSkillNumber = 2;

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

    public override void FireSkill(int skillNumber, Vector3 clickPosition)
    {
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                LeapAttack(clickPosition);
                break;
            case GroundSlamSkillNumber:
                GroundSlam();
                break;
            default:
                Debug.LogWarning("Invalid skill number for a male warrior character.");
                break;
        }
    }

    public override int InitialChargeCountOfSkill(int skillNumber)
    {
        return 0;
    }

    public override bool IsSkillChargeable(int skillNumber)
    {
        return false;
    }

    #region Leap Attack

    private void LeapAttack(Vector3 attackTarget)
    {
        if (IsAlive && IsLeapAttackAvailable && !maleWarriorAnimationManager.IsInterrupted && !maleWarriorAnimationManager.IsAttacking && !maleWarriorAnimationManager.IsGuarding)
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
