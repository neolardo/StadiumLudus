using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female ranger character.
/// </summary>
public class FemaleRangerCharacter : Character
{
    #region Properties and Fields

    private FemaleRangerAnimationManager femaleRangerAnimationManager;

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

    private const float dashJumpingTime = 0.36f;

    // charges maybe?

    private bool IsDashAvailable { get; set; } = true;

    private bool IsDashFirstFrame { get; set; }

    private Vector3 dashTarget;

    private Vector3 dashJumpDelta;

    #endregion

    #region Smoke

    private const int SmokeSkillNumber = 2;

    #endregion

    #region Trap

    private const int TrapSkillNumber = 3;

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
    }

    protected override void Start()
    {
        base.Start();
        femaleRangerAnimationManager = animationManager as FemaleRangerAnimationManager;
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

    #region Dash

    private void Dash(Vector3 attackTarget)
    {
        if (IsAlive && IsDashAvailable && !femaleRangerAnimationManager.IsInterrupted && !femaleRangerAnimationManager.IsAttacking && !femaleRangerAnimationManager.IsGuarding)
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
        float remainingTime = dashCooldown;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }
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

    private void Smoke() { }

    #endregion

    #region Trap
    private void PlaceTrap() { }

    #endregion

    #endregion

    #endregion
}
