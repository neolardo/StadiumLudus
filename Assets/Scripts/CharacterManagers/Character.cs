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
    [Tooltip("Represents the character trigger highlight.")]
    [SerializeField]
    private Highlight highlight;
    private float lastHighlightTriggerElapsedSeconds = Globals.HighlightDelay * 2;

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
    private float healthRecoveryDelay = 1.6f;

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
    /// Inidicates whether this <see cref="Character"/> can move or not.
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
    protected const float agentSpeedMultiplier = 4f / 3.5f;
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
    protected bool isChasing = false;
    protected bool forceRotation = false;
    protected bool isSprintingRequested = false;
    private const float sprintStaminaCostPerFixedUpdateFrame = 20f / 50f;
    #endregion

    #region Attack

    [Header("Basic Attack")]
    [Tooltip("Represents the stamina cost of a basic attack.")]
    [SerializeField]
    protected float attackStaminaCost;
    protected virtual bool IsInAction => animationManager.IsInterrupted || animationManager.IsAttacking || animationManager.IsGuarding || animationManager.IsUsingSkill || animationManager.IsInteracting;
    protected virtual bool CanAttack => IsAlive && !IsInAction && stamina > attackStaminaCost;
    protected virtual bool CanSetChaseTarget => IsAlive && !IsInAction;
    protected virtual bool CanSetAttackRotationTarget { get; set; } = false;

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
    private const int NumberOfRecordedValidationFrames = 50 * 3;

    #endregion

    #region Rig Colliders

    [Header("Rig colliders")]
    [Tooltip("The rig collider's root transform.")]
    [SerializeField]
    private Transform rigColliderRoot;

    private List<Transform> rigColliderTransforms;

    #endregion

    #endregion

    #region Guard

    protected virtual bool CanGuard => IsAlive && !IsInAction;

    #endregion

    #region Interactions
    protected virtual bool CanSetInteractionTarget => IsAlive && !IsInAction;
    protected virtual bool CanInteract => IsAlive && !IsInAction && (interactionPoint - rb.position).magnitude < interactionRange;

    protected Interactable interactionTarget;
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
            hitBoxTransform.gameObject.layer = Globals.IgnoreRaycastLayer; // prevent self hit
            hitBoxTransform.tag = Globals.IgnoreBoxTag;
        }
        else
        {
            Destroy(rb);
        }
        InitializeHitBoxRecording();
        InitializeRigColliderTransforms();
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
        if (IsAlive && AllowUpdate && PhotonView.IsMine)
        {
            UpdateInteractionCheck();
            UpdateChase();
            UpdateHitBoxInfo();
            UpdateMove();
        }
    }

    #endregion

    #region Skills

    /// <summary>
    /// Starts using a skill if possible. This should only be called from the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="skillNumber">The number of the skill.</param>
    /// <param name="clickPosition">The click position.</param>
    /// <param name="target">The optional target <see cref="Character"/>.</param>
    public abstract void StartSkill(int skillNumber, Vector3 clickPosition, Character target = null);

    /// <summary>
    /// Ends a skill. This should only be called from the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="skillNumber">The number of the skill.</param>
    public virtual void EndSkill(int skillNumber) { }

    /// <summary>
    /// Indicates whether a skill with the given number is a rechargeable or not.
    /// </summary>
    /// <param name="skillNumber">The number of the skill.</param>
    /// <returns>True if the skill is a rechargeable, otherwise false.</returns>
    public virtual bool IsSkillChargeable(int skillNumber) => false;

    /// <summary>
    /// Determines the initial charge number of the given skill.
    /// </summary>
    /// <param name="skillNumber">The number of the skill.</param>
    /// <returns>The initial charge number of the given skill.</returns>
    public virtual int InitialChargeCountOfSkill(int skillNumber) => 0;

    #endregion

    #region Attack

    /// <summary>
    /// Starts an attack. This should only be called from the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="attackPoint">The attack point.</param>
    /// <param name="target">The optional attack target <see cref="Character"/>.</param>
    public virtual void StartAttack(Vector3 attackPoint, Character target = null)
    {
        if (target != null)
        {
            SetChaseTarget(target.transform);
        }
        else
        {
            AttackWithoutTarget(attackPoint);
        }
    }

    /// <summary>
    /// Ends an attack. This should only be called from the <see cref="CharacterController"/>. 
    /// </summary>
    /// <param name="attackPoint">The attack point.</param>
    /// <param name="target">The optional attack target <see cref="Character"/>.</param>
    public virtual void EndAttack(Vector3 attackPoint, Character target = null) { }

    /// <summary>
    /// Sets the attack rotation target between the start and the end of an attack. This should only be called from the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="rotationTarget">The rotation target.</param>
    public void SetAttackRotationTarget(Vector3 rotationTarget)
    {
        if (CanSetAttackRotationTarget)
        {
            SetRotationTarget(rotationTarget);
        }
    }

    #region Without Target

    [PunRPC]
    public void AttackWithoutTarget(Vector3 attackPoint)
    {
        if (!PhotonView.IsMine || CanAttack )
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(AttackWithoutTarget), RpcTarget.Others, attackPoint);
            }
            OnAttackWithoutTarget(attackPoint);
        }
    }

    protected virtual void OnAttackWithoutTarget(Vector3 attackPoint)
    {
        isChasing = false;
        chaseTarget = null;
        interactionTarget = null;
        ClearDestination();
        animationManager.Attack();
        rotationTarget = attackPoint;
        stamina -= attackStaminaCost;
    }

    #endregion

    #region With Target

    protected void SetChaseTarget(Transform target)
    {
        if (CanSetChaseTarget)
        {
            chaseTarget = target;
            isChasing = target != null;
            interactionTarget = null;
        }
    }

    private void UpdateChase()
    {
        if (isChasing && chaseTarget !=null)
        {
            SetDestination(chaseTarget.position);
            AttackChaseTarget();
        }
    }

    [PunRPC]
    public void AttackChaseTarget()
    {
        if (!PhotonView.IsMine || (CanAttack && ((chaseTarget.position - rb.position).magnitude < attackRange)))
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(AttackChaseTarget), RpcTarget.Others);
            }
            isChasing = false;
            OnAttackChaseTarget();
        }
        else if (PhotonView.IsMine && ((chaseTarget.position - rb.position).magnitude < attackRange) && stamina < attackStaminaCost)
        {
            isChasing = false;
            ClearDestination();
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
        while (animationManager.IsAttacking && chaseTarget != null)
        {
            SetRotationTarget(chaseTarget.position);
            yield return null;
        }
        chaseTarget = null;
        isChasing = false;
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

    /// <summary>
    /// Determines whether a hit is valid by checking the hitbox <see cref="Collider"/>'s and the <see cref="AttackTrigger"/>'s position at the time when the hit occured.
    /// </summary>
    /// <param name="colliderPoint1">The first colliderpoint of the <see cref="AttackTrigger"/>'s <see cref="Collider"/>.</param>
    /// <param name="colliderPoint2">The second colliderpoint of the <see cref="AttackTrigger"/>'s <see cref="Collider"/>.</param>
    /// <param name="colliderRadius">The radius of the <see cref="AttackTrigger"/>'s <see cref="Collider"/>.</param>
    /// <param name="oldTimeStamp">The server timestamp when the collision has occured.</param>
    /// <param name="isForced">True if the attack was forced, so it is unavoidable, unless the <see cref="Character"/> cannot be interrupted.</param>
    /// <returns>True if the hit was valid, otherwise false.</returns>
    public bool IsHitValid(Vector3 colliderPoint1, Vector3 colliderPoint2, float colliderRadius, int oldTimeStamp, bool isForced)
    {
        int deltaFixedUpdateFrames = Mathf.RoundToInt((PhotonNetwork.ServerTimestamp - oldTimeStamp) / 20f); // 1000/50 ms is one fixed update frame delta
        if (deltaFixedUpdateFrames < NumberOfRecordedValidationFrames)
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

    #region Rig Colliders

    private void InitializeRigColliderTransforms()
    {
        rigColliderTransforms = new List<Transform>();
        AddRigTransformsToListRecursively(rigColliderRoot);
    }

    private void AddRigTransformsToListRecursively(Transform t)
    {
        if (t.CompareTag(Globals.IgnoreBoxTag) && t.GetComponent<Collider>() != null)
        {
            rigColliderTransforms.Add(t);
            for (int i = 0; i < t.childCount; i++)
            {
                AddRigTransformsToListRecursively(t.GetChild(i));
            }
        }
    }

    public int GetClosestRigColliderTransformIndex(Vector3 hitPoint)
    {
        float minDist = (rigColliderTransforms[0].position - hitPoint).magnitude;
        int minIndex = 0;
        for(int i=1; i<rigColliderTransforms.Count; i++)
        {
            float tempDist = (rigColliderTransforms[i].position - hitPoint).magnitude;
            if (tempDist < minDist)
            {
                minDist = tempDist;
                minIndex = i;
            }
        }
        return minIndex;
    }

    public Transform GetRigColliderTransform(int rigColliderIndex)
    {
        return rigColliderTransforms[rigColliderIndex];
    }

    #endregion

    #region Take Damage

    [PunRPC]
    public void TryTakeDamage(float amount, HitDirection direction, Vector3 attackColliderPoint0, Vector3 attackColliderPoint1, float attackColliderRadius, int senderAttackTriggerPhotonViewID, bool isForced, bool canBeGuarded, PhotonMessageInfo info)
    {
        if (IsAlive && PhotonView.IsMine && IsHitValid(attackColliderPoint0,  attackColliderPoint1,  attackColliderRadius, info.SentServerTimestamp, isForced))
        {
            TakeDamage(amount, direction, canBeGuarded);
            PhotonView.Find(senderAttackTriggerPhotonViewID).RPC(nameof(AttackTrigger.OnDamagingSucceeded), RpcTarget.All, PhotonView.ViewID);
        }
        else if(PhotonView.IsMine)
        {
            PhotonView.Find(senderAttackTriggerPhotonViewID).RPC(nameof(AttackTrigger.OnDamagingFailed), RpcTarget.All, PhotonView.ViewID);
        }
    }

    [PunRPC]
    public void TakeDamage(float amount, HitDirection direction, bool canBeGuarded)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(TakeDamage), RpcTarget.Others, amount, direction, canBeGuarded);
        }
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
            ClearDestination();
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
        if (!PhotonView.IsMine || CanGuard)
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
        if (!PhotonView.IsMine || animationManager.IsGuarding)
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(EndGuarding), RpcTarget.Others);
            }
            animationManager.EndGuarding();
        }
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
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        health = -1; // making sure the character is no longer alive
        animationManager.Move(0);
        animationManager.Die(direction);
        ClearDestination();
        chaseTarget = null;
        interactionTarget = null;
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
        AllowUpdate = true;
    }

    #endregion

    #region Interactions

    /// <summary>
    /// Sets the interaction target of this <see cref="Character"/>.  This should only be called from the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="interactable">The interaction target.</param>
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

    public void TryHeal(float healthHealAmount, float staminaHealAmount)
    {
        if (IsAlive && animationManager.IsInteracting)
        {
            health = Mathf.Min(health + healthHealAmount, healthMaximum);
            stamina = Mathf.Min(stamina + staminaHealAmount, staminaMaximum);
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

    protected void SetDestination(Vector3 destination)
    {
        if(PhotonView.IsMine)
        {
            if ((Destination - destination).magnitude > refreshDestinationDelta)
            {
                agent.nextPosition = rb.position + transform.forward * destinationMinimum * 1.5f;
            }
            Destination = destination;
        }
    }

    protected void ClearDestination()
    {
        if (PhotonView.IsMine)
        {
            Destination = rb.position;
            agent.nextPosition = rb.position;
        }
    }

    public void SetRotationTarget(Vector3 rotationTarget)
    {
        if (PhotonView.IsMine)
        {
            this.rotationTarget = rotationTarget;
        }
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

    /// <summary>
    /// Set the destination of the <see cref="Character"/>.  This should only be called from the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="position">The next destionation of the <see cref="Character"/>.</param>
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
        SetMovementSpeed(movementSpeed);//animationManager.Move(movementSpeed / movementSpeedMaximum);
    }

    [PunRPC]
    public void SetMovementSpeed(float movementSpeed)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(SetMovementSpeed), RpcTarget.Others, movementSpeed);
        }
        this.movementSpeed = movementSpeed;
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
        lastHighlightTriggerElapsedSeconds = 0;
    }

    private IEnumerator HighlightOnTriggered()
    {
        highlight.enabled = false;
        while (true)
        {
            yield return new WaitUntil(() => lastHighlightTriggerElapsedSeconds < Globals.HighlightDelay);
            while (lastHighlightTriggerElapsedSeconds < Globals.HighlightDelay)
            {
                highlight.enabled = true;
                lastHighlightTriggerElapsedSeconds += Time.deltaTime;
                yield return null;
            }
            highlight.enabled = false;
        }
    }

    #endregion

    #endregion
}
