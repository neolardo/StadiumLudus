using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PhotonView))]
/// <summary>
/// Represents a playable character of the game.
/// </summary>
public abstract class Character : MonoBehaviour, IHighlightable
{
    #region Properties and fields

    #region Playstyle

    /// <summary>
    /// The <see cref="CharacterFightingStyle"/> of this <see cref="Character"/>.
    /// </summary>
    public abstract CharacterFightingStyle FightingStyle { get; }

    /// <summary>
    /// The <see cref="CharacterClass"/> of this <see cref="Character"/>.
    /// </summary>
    public abstract CharacterClass Class { get; }

    #endregion

    #region UI and Managers

    protected CharacterUI characterUI;
    public PhotonView PhotonView { get; private set; }

    [Header("UI")]
    [Tooltip("Represents the character trigger outline.")]
    [SerializeField]
    private Outline outline;
    private float lastOutlineTriggerElapsedSeconds = Globals.OutlineDelay * 2;

    #endregion

    #region Health

    /// <summary>
    /// Represents the current health of the character.
    /// </summary>
    protected float health;

    [Header("Health")]
    [Tooltip("Represents the maximum health of the character.")]
    [SerializeField]
    private float healthMaximum = 100f;

    [Tooltip("Represents the health recovery delay in seconds.")]
    [SerializeField]
    private float healthRecoveryDelay= 1.6f;

    [Tooltip("Represents the initial health recovery amount per second.")]
    [SerializeField]
    private float healthRecoveryInitialAmount = 0;

    /// <summary>
    /// Represents the current health recovery amount per second.
    /// </summary>
    private float healthRecoveryAmount;

    /// <summary>
    /// Indicates whether this character is alive or not.
    /// </summary>
    public bool IsAlive => health > 0;

    /// <summary>
    /// The relative normalized health this character currently has.
    /// </summary>
    public float HealthRatio => health / healthMaximum;

    #endregion

    #region Stamina

    /// <summary>
    /// Represents the current stamina of the character.
    /// </summary>
    protected float stamina;

    [Header("Stamina")]
    [Tooltip("Represents the maximum stamina of the charater which allows it to interract, attack and block.")]
    [SerializeField]
    private float staminaMaximum = 100f;

    [Tooltip("Represents the stamina recovery delay in seconds.")]
    [SerializeField]
    private float staminaRecoveryDelay = 1.6f;

    [Tooltip("Represents the initial stamina recovery amount per second.")]
    [SerializeField]
    private float staminaRecoveryInitialAmount = 10f;

    /// <summary>
    /// Represents the current stamina recovery amount per second.
    /// </summary>
    private float staminaRecoveryAmount;

    /// <summary>
    /// The relative normalized stamina this character currently has.
    /// </summary>
    public float StaminaRatio => stamina / staminaMaximum;

    #endregion

    #region Movement

    /// <summary>
    /// Inidicates whether this character can move or not.
    /// </summary>
    protected virtual bool CanMove => !animationManager.IsMovementLocked;

    [Header("Movement")]
    [Tooltip("Represents the maximum movement speed of the character.")]
    [SerializeField]
    private float movementSpeedInitialMaximum = 3.5f;

    [Tooltip("Represents the maximum sprinting speed of the character.")]
    [SerializeField]
    private float sprintingSpeedInitialMaximum = 5f;

    private float movementSpeedMaximum;

    private float sprintingSpeedMaximum;

    [Tooltip("Represents the acceleration of the character.")]
    [SerializeField]
    private float acceleration = 20f;

    [Tooltip("Represents the deceleration of the character.")]
    [SerializeField]
    private float deceleration = 20f;

    [Tooltip("Represents the radial attack range of this character.")]
    [SerializeField]
    protected float attackRange = 1f;

    [Tooltip("Represents the audio source of this character.")]
    [SerializeField]
    protected AudioSource characterAudioSource;

    private Vector2 characterPositionRatioOnScreen;

    protected float movementSpeed;
    private float rotationVelocity;
    protected const float destinationThreshold = Globals.PositionThreshold;
    protected const float destinationMinimum = 0.7f;
    protected const float rotationThreshold = 2f;
    protected const float agentSpeedMultiplier = 4f/3.5f;
    private const float rotationSmoothDelta = 0.1f;
    private const float maximumRotationDelta = 120f;
    private const float maximumAgentDestinationDelta = 2f;
    private const float refreshDestinationDelta = 2f;
    protected const float agentAvoidanceRadius = .4f;
    private List<Transform> otherCharacterTransforms;
    private Vector3 _destination;

    /// <summary>
    /// The destination where the character is headed.
    /// </summary>
    protected Vector3 Destination
    {
        get
        {
            return _destination;
        }
        set
        {
            _destination = value;
            agent.destination = value;
        }
    }

    /// <summary>
    /// The cached transform of this <see cref="Character"/>.
    /// </summary>
    [HideInInspector]
    public new Transform transform;
    [Tooltip("Represents the animation manager of this character.")]
    public CharacterAnimationManager animationManager;
    [Tooltip("Represents the character trigger of this character.")]
    [SerializeField]
    protected GameObject characterTrigger;
    protected Rigidbody rb;
    private NavMeshAgent agent;
    protected Transform chaseTarget;
    private Vector3 rotationTarget;
    private NavMeshPath helperPath;
    protected bool forceRotation = false;
    protected bool isSprintingRequested = false;
    private const float sprintStaminaCostPerFixedUpdateFrame = 20f/50f;
    #endregion

    #region Attack

    [Header("Basic Attack")]
    [Tooltip("Represents the stamina cost of a basic attack.")]
    [SerializeField]
    protected float attackStaminaCost;
    protected virtual bool IsInAction => animationManager.IsInterrupted || animationManager.IsAttacking || animationManager.IsGuarding || animationManager.IsUsingSkill || animationManager.IsInteracting;
    protected virtual bool CanAttack => IsAlive && !IsInAction && stamina > attackStaminaCost;
    protected virtual bool CanSetChaseTarget => IsAlive && !IsInAction;

    #region Validation

    [Header("Validation")]
    [Tooltip("The hitbox collider's transform.")]
    [SerializeField]
    private Transform hitBoxTransform;

    /// <summary>
    /// The validation <see cref="Collider"/>'s <see cref="Transform"/>.
    /// </summary>
    private Transform validationBoxTransform;

    /// <summary>
    /// A <see cref="CircularBuffer"/> storing the recent <see cref="HitBoxInfo"/>s.
    /// </summary>
    private CircularBuffer<HitBoxInfo> hitBoxInfoCircularBuffer;

    /// <summary>
    /// The number of fixed update frames recorded for validating a hit info.
    /// </summary>
    private const int NumberOfRecordedValidationFrames = 50*3;

    #endregion

    #endregion

    #region Guard

    protected virtual bool CanGuard => IsAlive && !IsInAction;

    #endregion

    #region Interactions
    protected virtual bool CanSetInteractionTarget => IsAlive && !IsInAction;
    protected virtual bool CanInteract => IsAlive &&!IsInAction && (interactionPoint - rb.position).magnitude < interactionRange;

    private Interactable interactionTarget;
    private Vector3 interactionPoint;
    protected const float interactionRange = 0.1f;
    protected Buff currentBuff;

    #endregion

    private bool AllowUpdate { get; set; } = true;

    #endregion

    #region Methods

    #region Initialize

    protected virtual void Awake()
    {
        health = healthMaximum;
        healthRecoveryAmount = healthRecoveryInitialAmount;
        stamina = staminaMaximum;
        staminaRecoveryAmount = staminaRecoveryInitialAmount;
        movementSpeedMaximum = movementSpeedInitialMaximum;
        sprintingSpeedMaximum = sprintingSpeedInitialMaximum;
        transform = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        PhotonView = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = sprintingSpeedMaximum * agentSpeedMultiplier;
        otherCharacterTransforms = new List<Transform>();
        helperPath = new NavMeshPath();
        if (attackStaminaCost < Globals.CompareDelta)
        {
            Debug.LogWarning($"Basic attack stamina cost for a {gameObject.name} is set to a non-positive value.");
        }
        if (PhotonView.IsMine)
        {
            characterTrigger.layer = Globals.IgnoreRaycastLayer;
            characterTrigger.tag = Globals.IgnoreBoxTag;
        }
        InitializeHitBoxRecording();
        ClearDestination();
        StartCoroutine(RecoverHealth()); 
        StartCoroutine(RecoverStamina());
        StartCoroutine(HighlightOnTriggered());
    }

    public void InitializeAsLocalCharacter(CharacterUI characterUI)
    {
        this.characterUI = characterUI;
    }

    public void InitializeCharacterList()
    {
        otherCharacterTransforms.Clear();
        foreach (var c in GameRoundManager.Instance.LocalCharacterReferenceDictionary.Values)
        {
            if (c != this)
            {
                otherCharacterTransforms.Add(c.transform);
            }
        }
    }

    public void SetCharacterPositionOnScreen(Vector2 value)
    {
        characterPositionRatioOnScreen = value;
    }

    #endregion

    #region Update

    protected virtual void FixedUpdate()
    {
        if (IsAlive)
        {
            if (AllowUpdate && PhotonView.IsMine)
            {
                UpdateInteractionCheck();
                UpdateChase();
                UpdateHitBoxInfo();
            }
            UpdateMove();
        }
    }

    #endregion

    #region Skills
    public abstract void StartSkill(int skillNumber, Vector3 clickPosition, Character target);
    public virtual void EndSkill(int skillNumber) { }
    public virtual bool IsSkillChargeable(int skillNumber) => false;
    public virtual int InitialChargeCountOfSkill(int skillNumber) => 0;

    /// <summary>
    /// Clamps a point inside a given distance from the <see cref="Character"/>.
    /// </summary>
    /// <param name="target">The target point.</param>
    /// <param name="range">The maximum range in the target point's direction.</param>
    /// <param name="forceOverwrite">True if the given target should be recalculated by raycasting.</param>
    /// <returns>The target point closer than the given range.</returns>
    protected Vector3 ClampPointInsideRange(Vector3 target, float range, bool forceOverwrite = false)
    {
        if ((target - rb.position).magnitude > range)
        {
            var edgePoint = rb.position + (target - rb.position).normalized * range;
            var raycastPoint = edgePoint + Vector3.up * 5;
            Ray ray = new Ray(raycastPoint, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10, 1 << Globals.GroundLayer))
            {
                edgePoint = hit.point;
            }
            target = edgePoint;
        }
        else if (forceOverwrite)
        {
            Ray ray = new Ray(target + Vector3.up, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10, 1 << Globals.GroundLayer))
            {
                target = hit.point;
            }
        }
        return target;
    }

    #endregion

    #region Attack

    [PunRPC]
    public virtual bool TryAttack(Vector3 attackTarget)
    {
        if (CanAttack || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(TryAttack), RpcTarget.Others, attackTarget);
            }
            OnAttack(attackTarget);
            return true;
        }
        return false;
    }

    protected virtual void OnAttack(Vector3 attackTarget)
    {
        chaseTarget = null;
        interactionTarget = null;
        ClearDestination();
        animationManager.Attack();
        rotationTarget = attackTarget;
        stamina -= attackStaminaCost;
    }

    #region Chase

    public void SetChaseTarget(Transform target)
    {
        if (CanSetChaseTarget)
        {
            chaseTarget = target;
            interactionTarget = null;
        }
    }

    private void UpdateChase()
    {
        if (chaseTarget != null)
        {
            SetDestination(chaseTarget.position);
            TryAttackChaseTarget();
        }
    }

    [PunRPC]
    public void TryAttackChaseTarget()
    {
        if (!PhotonView.IsMine || (CanAttack && ((chaseTarget.position - rb.position).magnitude < attackRange)))
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(TryAttackChaseTarget), RpcTarget.Others);
            }
            OnAttackChaseTarget();
        }
    }

    protected virtual void OnAttackChaseTarget()
    {
        ClearDestination();
        animationManager.Attack();
        stamina -= attackStaminaCost;
        if (PhotonView.IsMine)
        {
            StartCoroutine(RotateToChaseTargetWhileAttacking());
        }
    }

    protected IEnumerator RotateToChaseTargetWhileAttacking()
    {
        yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsAttacking);
        while (animationManager.IsAttacking && chaseTarget != null)
        {
            SetRotationTarget(chaseTarget.position);
            yield return null;
        }
        chaseTarget = null;
        ClearDestination();
    }

    #endregion

    #endregion

    #region HitBox Validation

    private void InitializeHitBoxRecording()
    {
        if (PhotonView.IsMine)
        {
            hitBoxInfoCircularBuffer = new CircularBuffer<HitBoxInfo>(NumberOfRecordedValidationFrames);
            var copy = Instantiate(hitBoxTransform.GetComponent<Collider>(), null);
            Destroy(copy.GetComponent<HitBox>());
            copy.gameObject.layer = Globals.ValidationLayer;
            copy.gameObject.tag = Globals.IgnoreBoxTag;
            copy.gameObject.name = "CharacterValidationBox";
            validationBoxTransform = copy.transform;
            copy.gameObject.SetActive(true);
        }
    }

    private void UpdateHitBoxInfo()
    {
        var info = new HitBoxInfo(hitBoxTransform.position, hitBoxTransform.rotation, animationManager.CanBeInterrupted);
        hitBoxInfoCircularBuffer.Push(info);
    }

    public bool IsHitValid(Vector3 colliderPoint1, Vector3 colliderPoint2, float colliderRadius, int oldTimeStamp, bool isForced)
    {
        int deltaFixedUpdateFrames = Mathf.RoundToInt((PhotonNetwork.ServerTimestamp - oldTimeStamp) / 20f); // 1000/50 ms is one fixed update frame delta
        if(deltaFixedUpdateFrames < NumberOfRecordedValidationFrames)
        {
            var info = hitBoxInfoCircularBuffer[deltaFixedUpdateFrames];
            if (info.canBeInterrupted)
            {
                if (isForced)
                {
                    return true;
                }
                else
                {
                    validationBoxTransform.position = info.colliderPosition;
                    validationBoxTransform.rotation = info.colliderRotation;
                    var result = Physics.CheckCapsule(colliderPoint1, colliderPoint2, colliderRadius, 1 << Globals.ValidationLayer);
                    return result;
                }
            }
        }
        return false;
    }

    #endregion

    #region Take Damage

    [PunRPC]
    public void TryTakeDamage(float amount, HitDirection direction, Vector3 attackColliderPoint0, Vector3 attackColliderPoint1, float attackColliderRadius, int senderAttackTriggerPhotonViewID, bool isForced, bool canBeGuarded, PhotonMessageInfo info)
    {
        if (IsAlive && PhotonView.IsMine && IsHitValid(attackColliderPoint0,  attackColliderPoint1,  attackColliderRadius, info.SentServerTimestamp, isForced))
        { 
            PhotonView.RPC(nameof(TakeDamage), RpcTarget.All, amount, direction, canBeGuarded);
            PhotonView.Find(senderAttackTriggerPhotonViewID).RPC(nameof(AttackTrigger.OnDamagingSucceeded), RpcTarget.All, PhotonView.ViewID);
        }
        else 
        {
            PhotonView.Find(senderAttackTriggerPhotonViewID).RPC(nameof(AttackTrigger.OnDamagingFailed), RpcTarget.All, PhotonView.ViewID);
        }
    }

    [PunRPC]
    public void TakeDamage(float amount, HitDirection direction, bool canBeGuarded)
    {
        if (animationManager.IsGuarding && direction != HitDirection.Back && canBeGuarded)
        {
            float guardedAmount = stamina - Mathf.Max(0, stamina - amount);
            stamina -= guardedAmount;
            amount -= guardedAmount;
        }
        health -= amount;
        if (IsAlive)
        {
            bool successfullyGuarded = stamina > 0 && animationManager.IsGuarding && canBeGuarded;
            animationManager.Impact(successfullyGuarded, direction);
            if (successfullyGuarded)
            {
                if (direction == HitDirection.Back)
                {
                    AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.HitOnFlesh);
                }
                else
                {
                    AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.GuardHit);
                }
            }
            else
            {
                if (PhotonView.IsMine)
                {
                    EndGuarding();
                }
                AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.HitOnFlesh);
            }
            if (PhotonView.IsMine)
            {
                ClearDestination();
            }
            OnTakeDamage();
        }
        else
        {
            AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.HitOnFlesh);
            AudioManager.Instance.PlayOneShotSFX(characterAudioSource, FightingStyle == CharacterFightingStyle.Heavy ? SFX.MaleDeath : SFX.FemaleDeath);
            if (PhotonView.IsMine)
            {
                OnDie(direction);
            }
        }
    }

    protected virtual void OnTakeDamage() { }

    #endregion

    #region Guarding

    [PunRPC]
    public void StartGuarding()
    {
        if (CanGuard || !PhotonView.IsMine)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(StartGuarding), RpcTarget.Others);
            }
            animationManager.StartGuarding();
        }
    }

    [PunRPC]
    public void EndGuarding()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(EndGuarding), RpcTarget.Others);
        }
        animationManager.EndGuarding();
    }

    public void SetGuardTarget(Vector3 guardTarget)
    {
        if (animationManager.IsGuarding)
        {
            SetRotationTarget(guardTarget);
        }
    }

    #endregion

    #region Die and Win

    [PunRPC]
    protected virtual void OnDie(HitDirection direction)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(OnDie), RpcTarget.Others, direction);
        }
        health = -1; // making sure the character is no longer alive
        animationManager.Die(direction);
        ClearDestination();
        animationManager.Move(0);
        chaseTarget = null;
        interactionTarget = null;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        if (characterUI != null)
        {
            characterUI.ShowEndScreen(false);
        }
        GameRoundManager.Instance.OnCharacterDied();
        AllowUpdate = false;
    }

    [PunRPC]
    public void OnWin()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(OnWin), RpcTarget.Others);
        }
        ClearDestination();
        animationManager.Move(0);
        chaseTarget = null;
        interactionTarget = null;
        if (characterUI != null)
        {
            characterUI.ShowEndScreen(true);
        }
        AllowUpdate = false;
    }

    #endregion

    #region Interactions

    public void SetInteractionTarget(Interactable interactable)
    {
        if (CanSetInteractionTarget)
        {
            interactionPoint = interactable.GetClosestInteractionPoint(rb.position);
            if (CanInteract)
            {
                interactionTarget = interactable;
                Interract();
            }
            else 
            {
                MoveTo(interactionPoint);
                interactionTarget = interactable;
                interactionPoint = Destination;
            }
            chaseTarget = null;
        }
    }

    private void UpdateInteractionCheck()
    {
        if (interactionTarget != null)
        {
            if (CanInteract)
            {
                Interract();
            }
        }
    }

    private void Interract()
    {
        interactionTarget.PhotonView.RPC(nameof(Interactable.TryInteract), RpcTarget.All, PhotonView.ViewID);
        interactionTarget = null;
        ClearDestination();
    }

    #region Drink

    public void DrinkFromFountain(Vector3 fountainPosition)
    {
        if (IsAlive && !IsInAction)
        {
            SetRotationTarget(fountainPosition);
            animationManager.Drink();
        }
    }

    public void TryHeal(float healAmount)
    {
        if (IsAlive && animationManager.IsInteracting)
        {
            health = Mathf.Min(health + healAmount, healthMaximum);
        }
    }

    #endregion

    #region Buffs

    public void KneelBeforeStatue(Vector3 statuePosition)
    {
        if (IsAlive && !IsInAction)
        {
            SetRotationTarget(statuePosition);
            animationManager.Kneel();
        }
    }

    public void AddBuff(Buff buff)
    {
        if (IsAlive && animationManager.IsInteracting)
        {
            RemoveBuffs();
            currentBuff = buff;
            switch (buff.type)
            {
                case BuffType.HealthRecovery:
                    healthRecoveryAmount = buff.applimentMode == BuffApplimentMode.Additive ? healthRecoveryInitialAmount + buff.effectValue : healthRecoveryInitialAmount * buff.effectValue;
                    break;
                case BuffType.StaminaRecovery:
                    staminaRecoveryAmount = buff.applimentMode == BuffApplimentMode.Additive ? staminaRecoveryInitialAmount + buff.effectValue : staminaRecoveryInitialAmount * buff.effectValue;
                    break;
                case BuffType.MovementSpeed:
                    movementSpeedMaximum = buff.applimentMode == BuffApplimentMode.Additive ? movementSpeedInitialMaximum + buff.effectValue : movementSpeedInitialMaximum * buff.effectValue;
                    sprintingSpeedMaximum = buff.applimentMode == BuffApplimentMode.Additive ? sprintingSpeedInitialMaximum + buff.effectValue : sprintingSpeedInitialMaximum * buff.effectValue;
                    agent.speed = sprintingSpeedMaximum * agentSpeedMultiplier;
                    break;
                default:
                    Debug.LogWarning($"Invalid buff type: { buff.type}");
                    break;
            }
        }
        else
        {
            buff.ForceDeactivate();
        }
    }

    public void RemoveBuffs()
    {
        if (currentBuff != null)
        {
            currentBuff.ForceDeactivate();
            currentBuff = null;
        }
        healthRecoveryAmount = healthRecoveryInitialAmount;
        staminaRecoveryAmount = staminaRecoveryInitialAmount;
        movementSpeedMaximum = movementSpeedInitialMaximum;
        sprintingSpeedMaximum = sprintingSpeedInitialMaximum;
        agent.speed = sprintingSpeedMaximum * agentSpeedMultiplier;

    }

    #endregion

    #endregion

    #region Movement

    [PunRPC]
    public void SetDestination(Vector3 destination)
    {
        if(PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(SetDestination), RpcTarget.Others, destination);
        }
        if ((Destination - destination).magnitude > refreshDestinationDelta)
        {
            agent.nextPosition = rb.position + transform.forward * destinationMinimum * 1.5f;
        }
        Destination = destination;
    }

    [PunRPC]
    protected void ClearDestination()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(ClearDestination), RpcTarget.Others);
        }
        Destination = rb.position;
        agent.nextPosition = rb.position;
    }


    [PunRPC]
    public void SetRotationTarget(Vector3 rotationTarget)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(SetRotationTarget), RpcTarget.Others, rotationTarget);
        }
        this.rotationTarget = rotationTarget;
    }

    [PunRPC]
    public void SetIsSprintingRequested(bool isSprintingRequested)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(SetIsSprintingRequested), RpcTarget.Others, isSprintingRequested);
        }
        this.isSprintingRequested = isSprintingRequested;
    }

    public void MoveTo(Vector3 position)
    {
        agent.CalculatePath(position, helperPath);
        if (helperPath.status == NavMeshPathStatus.PathComplete)
        {
            var lastCorner = helperPath.corners[helperPath.corners.Length - 1];
            if ((lastCorner - rb.position).magnitude < destinationMinimum)
            {
                var dir = new Vector3(characterPositionRatioOnScreen.x - Input.mousePosition.x / Screen.width, 0, characterPositionRatioOnScreen.y - Input.mousePosition.y / Screen.height).normalized;
                SetDestination(rb.position + dir * destinationMinimum);
            }
            else
            {
                SetDestination(lastCorner);
            }
            chaseTarget = null;
            interactionTarget = null;
        }
    }

    private void UpdateMove()
    {
        var nextDestination = agent.path.corners[0];
        float distanceToDestination = (rb.position - nextDestination).magnitude;
        if (distanceToDestination > destinationThreshold && CanMove)
        {
            rb.freezeRotation = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            // prevent slow rotation on big directional change
            if ((nextDestination - Destination).magnitude > maximumAgentDestinationDelta && Vector3.Angle(transform.forward, (Destination - nextDestination).normalized) > maximumRotationDelta)
            {
                agent.nextPosition = rb.position + (Destination - nextDestination).normalized * destinationMinimum * 1.5f;
            } 
            movementSpeed = Mathf.Min(movementSpeed + Time.fixedDeltaTime * acceleration, TrySprint() ? sprintingSpeedMaximum : movementSpeedMaximum);
            var moveDirection = (nextDestination - rb.position).normalized;
            rb.MovePosition(RecalculatePositionBeforeColidingWithPlayers(moveDirection, movementSpeed));
            var targetRotation = Quaternion.LookRotation(new Vector3(nextDestination.x, rb.position.y, nextDestination.z) - rb.position);
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, ref rotationVelocity, rotationSmoothDelta);
            rb.rotation = Quaternion.Euler(0, angle, 0);
        }
        else
        {
            if (animationManager.IsGuarding || animationManager.IsAttacking || animationManager.IsInteracting || forceRotation)
            {
                var targetRotation = Quaternion.LookRotation(new Vector3(rotationTarget.x, rb.position.y, rotationTarget.z) - rb.position);
                if (Quaternion.Angle(targetRotation, rb.rotation) > rotationThreshold && (rotationTarget - rb.position).magnitude > destinationThreshold)
                {
                    var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, ref rotationVelocity, rotationSmoothDelta);
                    rb.rotation = Quaternion.Euler(0, angle, 0);
                }
                rb.freezeRotation = false;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                rb.freezeRotation = true;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.angularVelocity = Vector3.zero;
            }
            movementSpeed = Mathf.Max(movementSpeed - Time.fixedDeltaTime * deceleration, 0);
        }
        animationManager.Move(movementSpeed / movementSpeedMaximum);
    }

    private Vector3 RecalculatePositionBeforeColidingWithPlayers(Vector3 moveDirection, float movementSpeed)
    {
        bool playersCollide = true;
        for (int i = 0; i < 2 && playersCollide; i++)
        {
            playersCollide = false;
            var nextPosition = rb.position + moveDirection * movementSpeed * Time.fixedDeltaTime;
            foreach (var c in otherCharacterTransforms)
            {
                if ((nextPosition - c.transform.position).magnitude < agentAvoidanceRadius * 2)
                {
                    if ((Destination - c.transform.position).magnitude < agentAvoidanceRadius * 2)
                    {
                        ClearDestination();
                        return rb.position;
                    }
                    else
                    {
                        playersCollide = true;
                        moveDirection += (nextPosition - c.transform.position).normalized;
                    }
                }
            }
            moveDirection = moveDirection.normalized;
        }
        return rb.position + movementSpeed * Time.fixedDeltaTime * moveDirection;
    }

    private bool TrySprint()
    {
        if (isSprintingRequested && stamina > sprintStaminaCostPerFixedUpdateFrame)
        {
            stamina -= sprintStaminaCostPerFixedUpdateFrame;
            return true;
        }
        return false;
    }
    #endregion

    #region Recovery

    private IEnumerator RecoverHealth()
    {
        float lastHealth = health;
        while (IsAlive)
        {
            if (lastHealth > health + Globals.CompareDelta)
            {
                lastHealth = health;
                yield return new WaitForSeconds(healthRecoveryDelay);
            }
            else if (health < healthMaximum)
            {
                health = Mathf.Min(healthMaximum, health + healthRecoveryAmount * Time.deltaTime);
                lastHealth = health;
            }
            yield return null;
        }
    }

    private IEnumerator RecoverStamina()
    {
        float lastStamina = stamina;
        while (IsAlive)
        {
            if (lastStamina > stamina + Globals.CompareDelta)
            {
                lastStamina = stamina;
                yield return new WaitForSeconds(staminaRecoveryDelay);
            }
            else if (stamina < staminaMaximum)
            {
                stamina = Mathf.Min(staminaMaximum, stamina + staminaRecoveryAmount * Time.deltaTime);
                lastStamina = stamina;
            }
            yield return null;
        }
    }

    #endregion

    #region Highlight

    public void Highlight()
    {
        lastOutlineTriggerElapsedSeconds = 0;
    }

    private IEnumerator HighlightOnTriggered()
    {
        outline.enabled = false;
        while (true)
        {
            yield return new WaitUntil(() => lastOutlineTriggerElapsedSeconds < Globals.OutlineDelay);
            while (lastOutlineTriggerElapsedSeconds < Globals.OutlineDelay)
            {
                outline.enabled = true;
                lastOutlineTriggerElapsedSeconds += Time.deltaTime;
                yield return null;
            }
            outline.enabled = false;
        }
    }

    #endregion

    #endregion
}
