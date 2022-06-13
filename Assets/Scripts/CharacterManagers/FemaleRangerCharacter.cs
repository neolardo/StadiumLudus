using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female ranger character.
/// </summary>
public class FemaleRangerCharacter : Character
{
    #region Properties and Fields

    private FemaleRangerAnimationManager femaleRangerAnimationManager;

    [Header("Arrow")]
    [Tooltip("The arrow pool manager.")]
    [SerializeField]
    private ProjectilePoolManager arrowPool;

    [Tooltip("The arrow game object which is animated.")]
    [SerializeField]
    private GameObject animatedArrow;

    [Tooltip("Represents the minimum damage of a fired arrow.")]
    [SerializeField]
    private float arrowMinimumDamage;

    [Tooltip("Represents the maximum damage of a fired arrow.")]
    [SerializeField]
    private float arrowMaximumDamage;

    [Tooltip("Represents the force of a fired arrow.")]
    [SerializeField]
    private float arrowForce = 3;

    private bool hasInitialized;

    #region Skills

    #region Dash

    [Header("Dash")]
    [Tooltip("Represents jump force of the dash.")]
    [SerializeField]
    private float dashJumpForce = 250;

    [Tooltip("Represents maximum distance of the dash.")]
    [SerializeField]
    private float dashMaximumDistance = 3.5f;

    [Tooltip("Represents cooldown of the dash skill in seconds.")]
    [SerializeField]
    private float dashCooldown = 5f;

    private const int DashSkillNumber = 1;

    private const float dashJumpingTime = 0.29f;

    private bool IsDashAvailable { get; set; } = true;

    private bool IsDashFirstFrame { get; set; }

    private bool CanDash => IsAlive && IsDashAvailable && !femaleRangerAnimationManager.IsInterrupted && !femaleRangerAnimationManager.IsAttacking && !femaleRangerAnimationManager.IsGuarding && !femaleRangerAnimationManager.IsUsingSkill;

    private Vector3 dashTarget;

    private Vector3 dashJumpDelta;

    #endregion

    #region Smoke

    private const int SmokeSkillNumber = 2;

    [Header("Smoke")]
    [Tooltip("Represents cooldown of the smoke skill in seconds.")]
    [SerializeField]
    private float smokeCooldown = 5f;

    private bool IsSmokeAvailable { get; set; } = true;

    private bool CanSmoke => IsAlive && IsSmokeAvailable && !femaleRangerAnimationManager.IsInterrupted && !femaleRangerAnimationManager.IsAttacking && !femaleRangerAnimationManager.IsGuarding && !femaleRangerAnimationManager.IsUsingSkill;

    #endregion

    #region Trap

    private const int TrapSkillNumber = 3;

    [Header("Trap")]
    [Tooltip("Represents cooldown of the trap skill in seconds.")]
    [SerializeField]
    private float trapCooldown = 5f;

    private bool IsTrapAvailable => trapChargeCount > 0;

    [Tooltip("The initial number of charges for the trap skill.")]
    [SerializeField]
    private int trapInitialChargeCount = 1;

    [Tooltip("The maximum number of charges for the trap skill.")]
    [SerializeField]
    private int trapMaximumChargeCount = 3;

    private int trapChargeCount;

    private bool CanPlaceTrap => IsAlive && IsTrapAvailable && !femaleRangerAnimationManager.IsInterrupted && !femaleRangerAnimationManager.IsAttacking && !femaleRangerAnimationManager.IsGuarding && !femaleRangerAnimationManager.IsUsingSkill;

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Initialize

    protected void OnEnable()
    {
        // order is important
        if (!hasInitialized)
        {
            Initialize();
            hasInitialized = true;
        }
    }

    private void Initialize()
    {
        if (arrowMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a non-positive value.");
        }
        if (arrowMaximumDamage < arrowMinimumDamage)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a lesser value than the minimum.");
        }
        if (arrowForce <= 0)
        {
            Debug.LogWarning("Arrow force for a female ranger character is set to non-positive value.");
        }
        arrowPool.MinimumDamage = arrowMinimumDamage;
        arrowPool.MaximumDamage = arrowMaximumDamage;
        arrowPool.Force = arrowForce;
        trapChargeCount = trapInitialChargeCount;
    }

    protected override void Start()
    {
        base.Start();
        femaleRangerAnimationManager = animationManager as FemaleRangerAnimationManager;
        StartCoroutine(ManageTrapCooldownAndRecharge());
    }

    #endregion

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateDash();
    }

    #region Attack

    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAnimations());
    }

    private IEnumerator ManageAnimations()
    {
        yield return new WaitUntil(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(false);
        arrowPool.Fire();
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(true);
    }

    #endregion

    #region Skills

    public override void FireSkill(int skillNumber, Vector3 clickPosition)
    {
        switch (skillNumber)
        {
            case DashSkillNumber:
                Dash(clickPosition);
                break;
            case SmokeSkillNumber:
                Smoke();
                break;
            case TrapSkillNumber:
                PlaceTrap();
                break;
            default:
                Debug.LogWarning("Invalid skill number for a female ranger character.");
                break;
        }
    }

    public override int InitialChargeCountOfSkill(int skillNumber)
    {
        return skillNumber == TrapSkillNumber ? trapInitialChargeCount : 0;
    }

    public override bool IsSkillChargeable(int skillNumber)
    {
        return skillNumber == TrapSkillNumber;
    }

    #region Dash

    private void Dash(Vector3 attackTarget)
    {
        if (CanDash)
        {
            var edgePoint = rb.position - (attackTarget - rb.position).normalized * dashMaximumDistance;
            var raycastPoint = edgePoint + Vector3.up * 5;
            Ray ray = new Ray(raycastPoint, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                attackTarget = hit.point;
            }
            IsDashFirstFrame = true;
            IsDashAvailable = false;
            dashTarget = attackTarget;
            SetRotationTarget(attackTarget);
            MoveTo(dashTarget);
            femaleRangerAnimationManager.Dash();
            //StartCoroutine(ManageAttackTrigger());
            StartCoroutine(ManageDashCooldown());
            StartCoroutine(ResetDestinationAfterDash());
        }
    }

    private void UpdateDash()
    {
        if (femaleRangerAnimationManager.IsJumping)
        {
            if (IsDashFirstFrame)
            {
                rb.AddForce(Vector3.up * dashJumpForce, ForceMode.Impulse);
                dashJumpDelta = (new Vector3(dashTarget.x, rb.position.y, dashTarget.z) - rb.position) / (dashJumpingTime * 50);
                IsDashFirstFrame = false;
            }
            else
            {
                rb.MovePosition(rb.position + dashJumpDelta);
            }
        }
    }

    private IEnumerator ManageDashCooldown()
    {
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(DashSkillNumber, dashCooldown);
        }
        yield return new WaitForSeconds(dashCooldown);
        IsDashAvailable = true;
    }

    private IEnumerator ResetDestinationAfterDash() // might refactor later
    {
        yield return new WaitUntil(() => femaleRangerAnimationManager.IsJumping);
        yield return new WaitWhile(() => femaleRangerAnimationManager.IsJumping);
        ClearDestination();
    }

    #endregion

    #region Smoke

    private void Smoke() 
    {
        if (CanSmoke)
        {
            IsSmokeAvailable = false;
            femaleRangerAnimationManager.Smoke();
            StartCoroutine(ManageSmokeCooldown());
        }
    }

    private IEnumerator ManageSmokeCooldown()
    {
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(SmokeSkillNumber, smokeCooldown);
        }
        yield return new WaitForSeconds(smokeCooldown);
        IsSmokeAvailable = true;
    }


    #endregion

    #region Trap
    private void PlaceTrap() 
    {
        if (CanPlaceTrap)
        {
            trapChargeCount -= 1;
            femaleRangerAnimationManager.PlaceTrap();
            if (characterUI != null)
            {
                characterUI.RemoveSkillCharge(TrapSkillNumber);
            }
        }
    }


    private IEnumerator ManageTrapCooldownAndRecharge()
    {
        while (IsAlive)
        {
            yield return new WaitUntil(() => trapChargeCount < trapMaximumChargeCount || !IsAlive);
            if (trapChargeCount < trapMaximumChargeCount)
            {
                if (characterUI != null)
                {
                    characterUI.StartSkillCooldown(TrapSkillNumber, trapCooldown);
                }
                yield return new WaitForSeconds(trapCooldown);
                trapChargeCount += 1;
                characterUI.AddSkillCharge(TrapSkillNumber);
            }
        }
    }

    #endregion

    #endregion

    #endregion
}
