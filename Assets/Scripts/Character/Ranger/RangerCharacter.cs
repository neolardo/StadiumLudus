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

    #region Attack

    protected Character attackTarget;

    protected override bool CanSetAttackRotationTarget => rangerAnimationManager.IsDrawing;

    #endregion

    #region Skills

    #region Dash

    private const int DashSkillNumber = 1;

    [Header("Dash")]
    [Tooltip("Represents distance of the dash.")]
    [SerializeField]
    private float dashDistance = 3.5f;

    [Tooltip("Represents cooldown of the dash skill in seconds.")]
    [SerializeField]
    private float dashCooldown = 5f;

    [Tooltip("Represents the stamina cost of the dash skill.")]
    [SerializeField]
    private float dashStaminaCost = 10f;
    private bool IsDashAvailable { get; set; } = true;
    private bool CanDash => IsAlive && IsDashAvailable && !IsInAction && stamina > dashStaminaCost;

    private float elapsedDashingTime;
    private Vector3 dashPoint;
    private Vector3 dashOrigin;
    private const float dashStartVelocity = 40;
    private const float dashEndVelocity = 5;
    private const float maximumDashingTime = .4f;

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

    private bool CanSmoke => IsAlive && IsSmokeAvailable && !IsInAction && stamina > smokeStaminaCost;

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
    private bool CanPlaceTrap => IsAlive && trapChargeCount > 0 && !IsInAction;

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
        if (PhotonView.IsMine)
        {
            StartCoroutine(ManageTrapCooldownAndRecharge());
        }
    }

    #endregion

    #region Updates

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (PhotonView.IsMine)
        {
            UpdateDash();
        }
    }

    #endregion

    #region Attack

    #region End

    public override void EndAttack(Vector3 attackPoint, Character target = null)
    {
        base.EndAttack(attackPoint, target);
        attackTarget = target;
        OnEndAttack();
    }

    [PunRPC]
    public virtual void OnEndAttack()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(OnEndAttack), RpcTarget.Others);
        }
        rangerAnimationManager.SetIsDrawing(false);
    }

    #endregion

    #endregion

    #region Take Damage

    protected override void OnTakeDamage()
    {
        base.OnTakeDamage();
        rangerAnimationManager.SetIsDashing(false);
        rangerAnimationManager.SetIsDrawing(false);
    }

    #endregion

    #region Skills

    public override void StartSkill(int skillNumber, Vector3 clickPosition, Character target)
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
        if (PhotonView.IsMine)
        {
            characterHUD.StartSkillCooldown(skillNumber, cooldown);
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
    public void Dash(Vector3 targetPoint)
    {
        if (!PhotonView.IsMine || CanDash)
        { 
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(Dash), RpcTarget.Others, targetPoint);
                IsDashAvailable = false;
                elapsedDashingTime = 0;
                stamina -= dashStaminaCost;
                forceRotation = true;
                dashOrigin = rb.position;
                dashPoint = Globals.GetPointAtRange(rb.position, targetPoint, dashDistance, agent);
                rotationTarget = dashPoint + (dashPoint - rb.position).normalized * destinationDistanceMinimum;
                MoveTo(dashPoint);
                StartCoroutine(ManageCooldown(DashSkillNumber));
                StartCoroutine(ResetDestinationAfterDash());
            }
            AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.Dash, doNotRepeat: true);
            rangerAnimationManager.Dash();
            OnDash();
        }
        else if (PhotonView.IsMine)
        {
            characterHUD.OnCannotPerformSkillOrAttack(stamina < dashStaminaCost, !IsDashAvailable, DashSkillNumber);
        }
    }

    protected virtual void OnDash() { }

    private void UpdateDash()
    {
        if (rangerAnimationManager.IsJumping && elapsedDashingTime < maximumDashingTime && (rb.position - dashOrigin).magnitude < (dashPoint - dashOrigin).magnitude)
        {
            float x = (rb.position - dashOrigin).magnitude / (dashPoint - dashOrigin).magnitude;
            float dashVelocity = Mathf.Lerp(dashStartVelocity, dashEndVelocity, x);
            Vector3 dashDirection = (dashPoint - dashOrigin).normalized;
            rb.velocity = new Vector3(dashVelocity * dashDirection.x, rb.velocity.y, dashVelocity * dashDirection.z);
            elapsedDashingTime += Time.fixedDeltaTime;
        }
        else if (rangerAnimationManager.IsJumping)
        {
            OnDashFinished();
        }
    }

    [PunRPC]
    public void OnDashFinished()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(OnDashFinished), RpcTarget.Others);
        }
        rangerAnimationManager.SetIsDashing(false);
    }

    private IEnumerator ResetDestinationAfterDash()
    {
        yield return new WaitUntil(() => animationManager.IsJumping);
        yield return new WaitWhile(() => animationManager.IsJumping);
        forceRotation = false;
        ClearDestination();
    }

    #endregion

    #region Smoke

    [PunRPC]
    public void Smoke()
    {
        if (!PhotonView.IsMine || CanSmoke)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(Smoke), RpcTarget.Others);
                StartCoroutine(ManageCooldown(SmokeSkillNumber));
                stamina -= smokeStaminaCost;
                IsSmokeAvailable = false;
            }
            rangerAnimationManager.Smoke();
            AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.Smoke);
            smokeTransform.position = transform.position + smokePositionDelta;
            smokeParticleSystem.Play();
        }
        else if (PhotonView.IsMine)
        {
            characterHUD.OnCannotPerformSkillOrAttack(stamina < smokeStaminaCost, !IsSmokeAvailable, SmokeSkillNumber);
        }
    }

    #endregion

    #region Trap

    [PunRPC]
    public void PlaceTrap()
    {
        if (!PhotonView.IsMine || CanPlaceTrap)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(PlaceTrap), RpcTarget.Others);
                trapChargeCount -= 1;
                trapPool.PlaceTrap(TrapPlacementDelay);
                characterHUD.RemoveSkillCharge(TrapSkillNumber);
            }
            rangerAnimationManager.PlaceTrap();
        }
        else if (PhotonView.IsMine)
        {
            characterHUD.OnCannotPerformSkillOrAttack(false, trapChargeCount == 0, TrapSkillNumber);
        }
    }

    private IEnumerator ManageTrapCooldownAndRecharge()
    {
        while (IsAlive)
        {
            yield return new WaitUntil(() => trapChargeCount < trapMaximumChargeCount || !IsAlive);
            if (trapChargeCount < trapMaximumChargeCount)
            {
                characterHUD.StartSkillCooldown(TrapSkillNumber, trapCooldown);
                yield return new WaitForSeconds(trapCooldown);
                trapChargeCount += 1;
                characterHUD.AddSkillCharge(TrapSkillNumber);
            }
        }
    }

    #endregion

    #endregion

    #endregion
}
