using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// An abstract class for every ranger character.
/// </summary>
public abstract class RangerCharacter : Character
{
    #region Properties and Fields

    protected RangerAnimationManager rangerAnimationManager;

    public override CharacterClass Class => CharacterClass.Ranger;

    #region Skills

    #region Dash

    private const int DashSkillNumber = 1;

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

    [Tooltip("Represents the stamina cost of the dash skill.")]
    [SerializeField]
    private float dashStaminaCost = 10f;
    protected abstract float DashJumpingTime { get; }

    private bool IsDashAvailable { get; set; } = true;

    private bool IsDashFirstFrame { get; set; }

    private bool CanDash => IsAlive && IsDashAvailable && !rangerAnimationManager.IsInterrupted && !rangerAnimationManager.IsAttacking && !rangerAnimationManager.IsGuarding && !rangerAnimationManager.IsUsingSkill && stamina > dashStaminaCost;

    private Vector3 dashTarget;

    private Vector3 dashJumpDelta;

    #endregion

    #region Smoke

    private const int SmokeSkillNumber = 2;

    [Header("Smoke")]
    [Tooltip("The smoke particle system.")]
    [SerializeField]
    private ParticleSystem smokeParticleSystem;

    [Tooltip("The transform of the smoke.")]
    [SerializeField]
    private Transform smokeTransform;

    [Tooltip("Represents the duration of the smoke skill in seconds.")]
    [SerializeField]
    private float smokeDuration = 30f;

    [Tooltip("Represents the cooldown of the smoke skill in seconds.")]
    [SerializeField]
    private float smokeCooldown = 5f;

    [Tooltip("Represents the stamina cost of the smoke skill.")]
    [SerializeField]
    private float smokeStaminaCost = 10f;

    private Vector3 smokePositionDelta = Vector3.up * 1.5f;
    private bool IsSmokeAvailable { get; set; } = true;

    private bool CanSmoke => IsAlive && IsSmokeAvailable && !rangerAnimationManager.IsInterrupted && !rangerAnimationManager.IsAttacking && !rangerAnimationManager.IsGuarding && !rangerAnimationManager.IsUsingSkill && stamina > smokeStaminaCost;

    #endregion

    #region Trap

    private const int TrapSkillNumber = 3;

    [Header("Trap")]
    [Tooltip("The trap pool manager.")]
    [SerializeField]
    private TrapPoolManager trapPool;

    [Tooltip("The minimum damage dealt by a trap.")]
    [SerializeField]
    private float trapMinimumDamage;

    [Tooltip("The maximum damage dealt by a trap.")]
    [SerializeField]
    private float trapMaximumDamage;

    [Tooltip("The amount of seconds the trap remains active after placed on the ground.")]
    [SerializeField]
    private float trapDuration = 30f;

    [Tooltip("Represents cooldown of the trap skill in seconds.")]
    [SerializeField]
    private float trapCooldown = 5f;

    [Tooltip("The initial number of charges for the trap skill.")]
    [SerializeField]
    private int trapInitialChargeCount = 1;

    [Tooltip("The maximum number of charges for the trap skill.")]
    [SerializeField]
    private int trapMaximumChargeCount = 3;

    protected int trapChargeCount;
    protected abstract float TrapPlacementDelay { get; }
    private bool CanPlaceTrap => IsAlive && trapChargeCount > 0 && !rangerAnimationManager.IsInterrupted && !rangerAnimationManager.IsAttacking && !rangerAnimationManager.IsGuarding && !rangerAnimationManager.IsUsingSkill;

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Initialize

    protected override void Awake()
    {
        base.Awake();
        if (smokeDuration < Globals.CompareDelta)
        {
            Debug.LogWarning("Cmoke duration for a female ranger character is set to a non-positive value.");
        }
        if (smokeDuration > smokeCooldown)
        {
            Debug.LogWarning("Smoke cooldown for a female ranger character is set to a lesser value than the duration.");
        }
        if (trapMinimumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Trap maximum damage for a female ranger character is set to a non-positive value.");
        }
        if (trapMaximumDamage < trapMinimumDamage)
        {
            Debug.LogWarning("Trap maximum damage for a female ranger character is set to a lesser value than the minimum.");
        }
        if (trapDuration < Globals.CompareDelta)
        {
            Debug.LogWarning("Trap duration for a female ranger character is set to a non-positive value.");
        }
        var mainPS = smokeParticleSystem.main;
        mainPS.duration = smokeDuration;
        mainPS.startLifetime = smokeDuration;
        smokeTransform.SetParent(null);
        trapChargeCount = trapInitialChargeCount;
        trapPool.MinimumDamage = trapMinimumDamage;
        trapPool.MaximumDamage = trapMaximumDamage;
        trapPool.Duration = trapDuration;
        rangerAnimationManager = animationManager as RangerAnimationManager;
        StartCoroutine(ManageTrapCooldownAndRecharge());
    }

    #endregion

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateDash();
    }

    #region Skills

    public override void StartSkill(int skillNumber, Vector3 clickPosition)
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
                Debug.LogWarning("Invalid skill number for a ranger character.");
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

    private IEnumerator ManageCooldown(int skillNumber)
    {
        float cooldown = 0;
        switch (skillNumber)
        {
            case DashSkillNumber:
                cooldown = dashCooldown;
                break;
            case SmokeSkillNumber:
                cooldown = smokeCooldown;
                break;
            default:
                Debug.Log("Invalid skill number for a ranger character.");
                break;
        }
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(skillNumber, cooldown);
        }
        yield return new WaitForSeconds(cooldown);
        switch (skillNumber)
        {
            case DashSkillNumber:
                IsDashAvailable = true;
                break;
            case SmokeSkillNumber:
                IsSmokeAvailable = true;
                break;
            default:
                Debug.Log("Invalid skill number for a ranger character.");
                break;
        }
    }

    #region Dash

    [PunRPC]
    public void Dash(Vector3 target)
    {
        if (CanDash || !PhotonView.IsMine)
        { 
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(Dash), RpcTarget.Others, target);
            }
            var edgePoint = rb.position - (target - rb.position).normalized * dashMaximumDistance;
            var raycastPoint = edgePoint + Vector3.up * 5;
            Ray ray = new Ray(raycastPoint, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                target = hit.point;
            }
            IsDashFirstFrame = true;
            IsDashAvailable = false;
            dashTarget = target;
            SetRotationTarget(target);
            MoveTo(dashTarget);
            rangerAnimationManager.Dash();
            // sound?
            StartCoroutine(ManageCooldown(DashSkillNumber));
            StartCoroutine(ResetDestinationAfterDash());
            stamina -= dashStaminaCost;
        }
        else if (PhotonView.IsMine && characterUI != null)
        {
            characterUI.OnCannotPerformSkillOrAttack(stamina < dashStaminaCost, !IsDashAvailable, DashSkillNumber);
        }
    }

    private void UpdateDash()
    {
        if (rangerAnimationManager.IsJumping)
        {
            if (IsDashFirstFrame)
            {
                rb.AddForce(Vector3.up * dashJumpForce, ForceMode.Impulse);
                dashJumpDelta = (new Vector3(dashTarget.x, rb.position.y, dashTarget.z) - rb.position) / (DashJumpingTime * 50);
                IsDashFirstFrame = false;
            }
            else
            {
                rb.MovePosition(rb.position + dashJumpDelta);
            }
        }
    }


    private IEnumerator ResetDestinationAfterDash()
    {
        yield return new WaitUntil(() => rangerAnimationManager.IsJumping);
        yield return new WaitWhile(() => rangerAnimationManager.IsJumping);
        ClearDestination();
    }

    #endregion

    #region Smoke

    [PunRPC]
    public void Smoke()
    {
        if (CanSmoke || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(Smoke), RpcTarget.Others);
            }
            IsSmokeAvailable = false;
            rangerAnimationManager.Smoke();
            AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.Smoke);
            smokeTransform.position = rb.position + smokePositionDelta;
            smokeParticleSystem.Play();
            StartCoroutine(ManageCooldown(SmokeSkillNumber));
            stamina -= smokeStaminaCost;
        }
        else if (PhotonView.IsMine && characterUI != null)
        {
            characterUI.OnCannotPerformSkillOrAttack(stamina < smokeStaminaCost, !IsSmokeAvailable, SmokeSkillNumber);
        }
    }

    #endregion

    #region Trap

    [PunRPC]
    public void PlaceTrap()
    {
        if (CanPlaceTrap || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(PlaceTrap), RpcTarget.Others);
            }
            trapChargeCount -= 1;
            rangerAnimationManager.PlaceTrap();
            trapPool.PlaceTrap(TrapPlacementDelay);
            if (characterUI != null)
            {
                characterUI.RemoveSkillCharge(TrapSkillNumber);
            }
        }
        else if (PhotonView.IsMine && characterUI != null)
        {
            characterUI.OnCannotPerformSkillOrAttack(false, trapChargeCount == 0, TrapSkillNumber);
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
                if (characterUI != null)
                {
                    characterUI.AddSkillCharge(TrapSkillNumber);
                }
            }
        }
    }

    #endregion

    #endregion

    #endregion
}
