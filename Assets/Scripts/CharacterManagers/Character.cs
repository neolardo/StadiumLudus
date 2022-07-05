using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PhotonView))]
/// <summary>
/// Represents a playable character of the game.
/// </summary>
public abstract class Character : MonoBehaviour
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

    [Tooltip("Represents the maximum rotation speed for the character's movement in deg/sec.")]
    [SerializeField]
    private float rotationSpeedMaximum = 1500f;

    [Tooltip("Represents the minimum rotation speed for the character's movement in deg/sec.")]
    [SerializeField]
    private float rotationSpeedMinimum = 400f;

    [Tooltip("Represents the radial attack range of this character.")]
    [SerializeField]
    protected float attackRange = 1f;

    [Tooltip("Represents the audio source of this character.")]
    [SerializeField]
    protected AudioSource characterAudioSource;

    private Vector2 characterPositionRatioOnScreen;

    protected float movementSpeed;
    protected const float destinationThreshold = Globals.PositionThreshold;
    protected const float destinationMinimum = 0.7f;
    protected const float rotationThreshold = 2f;
    protected const float agentSpeedMultiplier = 4f/3.5f;

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
    public CharacterAnimationManager animationManager;
    protected Rigidbody rb;
    private NavMeshAgent agent;
    private Transform chaseTarget;
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

    [SerializeField]
    protected Transform hitValidationColliderContainer;

    protected virtual bool CanAttack => IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill && !animationManager.IsInteracting && stamina > attackStaminaCost;

    /// <summary>
    /// A list of <see cref="Transform"/>s of the character's hitbox <see cref="Collider"/>s.
    /// </summary>
    private List<Transform> hitBoxTransforms;

    /// <summary>
    /// A list of <see cref="Collider"/> <see cref="Transform"/>s used for validating a hit.
    /// </summary>
    private List<Transform> validationColliderTransforms;

    /// <summary>
    /// A  <see cref="CachedCircularBuffer"/> storing the most recent info of the <see cref="hitBoxTransforms"/>.
    /// </summary>
    private CachedCircularBuffer<HitBoxInfoStorage> hitBoxInfoCircularBuffer;

    /// <summary>
    /// The number of fixed update frames recorded for validating a hit info.
    /// </summary>
    private const int NumberOfRecordedHitBoxFrames = 50*3;

    #endregion

    #region Guard

    protected virtual bool CanGuard => IsAlive && !animationManager.IsInterrupted && !animationManager.IsGuarding && !animationManager.IsAttacking && !animationManager.IsUsingSkill;

    #endregion

    #region Interactions

    private Interactable interactionTarget;
    private Vector3 interactionPoint;
    protected const float interactionRange = 0.1f;
    protected const float fountainHealDelay = 0.5f;
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
        agent = GetComponent<NavMeshAgent>();
        PhotonView = GetComponent<PhotonView>();
        outline = GetComponent<Outline>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = sprintingSpeedMaximum * agentSpeedMultiplier;
        helperPath = new NavMeshPath();
        if (attackStaminaCost < Globals.CompareDelta)
        {
            Debug.LogWarning($"Basic attack stamina cost for a {gameObject.name} is set to a non-positive value.");
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

    public void SetCharacterPositionOnScreen(Vector2 value)
    {

        var characterScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        characterPositionRatioOnScreen = new Vector2(0.5f, characterScreenPosition.y / Screen.height);
    }

    #endregion

    #region Updates

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
    public abstract void StartSkill(int skillNumber, Vector3 clickPosition);
    public virtual void EndSkill(int skillNumber) { }
    public virtual bool IsSkillChargeable(int skillNumber) => false;
    public virtual int InitialChargeCountOfSkill(int skillNumber) => 0;

    /// <summary>
    /// Clamps a point inside a given distance from the <see cref="Character"/>.
    /// </summary>
    /// <param name="target">The target point.</param>
    /// <param name="range">The maximum range in the target point's direction.</param>
    /// <returns>The target point closer than the given range.</returns>
    protected Vector3 ClampPointInsideRange(Vector3 target, float range)
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
        else if (PhotonView.IsMine && characterUI != null && stamina < attackStaminaCost)
        {
            characterUI.OnCannotPerformSkillOrAttack(stamina < attackStaminaCost, false);
        }
        return false;
    }

    protected virtual void OnAttack(Vector3 attackTarget)
    {
        animationManager.Attack();
        rotationTarget = attackTarget;
        stamina -= attackStaminaCost;
    }

    #endregion

    #region HitBox Validation

    private void InitializeHitBoxRecording()
    {
        if (PhotonView.IsMine)
        {
            hitBoxTransforms = new List<Transform>();
            validationColliderTransforms = new List<Transform>();
            hitBoxInfoCircularBuffer = new CachedCircularBuffer<HitBoxInfoStorage>(NumberOfRecordedHitBoxFrames);
            FindAndAddHitBoxTransformsToList(transform);
            foreach (var t in hitBoxTransforms)
            {
                var copy = Instantiate(t.GetComponent<Collider>(), hitValidationColliderContainer);
                copy.gameObject.SetActive(true);
                Destroy(copy.GetComponent<HitBox>());
                copy.gameObject.layer = Globals.ValidationLayer;
                validationColliderTransforms.Add(copy.transform);
            }
        }
    }

    private void FindAndAddHitBoxTransformsToList(Transform t)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            FindAndAddHitBoxTransformsToList(t.GetChild(i));
        }
        if (t.CompareTag(Globals.HitBoxTag))
        {
            hitBoxTransforms.Add(t);
        }
    }

    private void UpdateHitBoxInfo()
    {
        var info = hitBoxInfoCircularBuffer.GetNext();
        info.CanBeInterrupted = animationManager.CanBeInterrupted;
        if (info.CanBeInterrupted)
        {
            for (int i = 0; i < hitBoxTransforms.Count; i++)
            {
                info.ColliderTransformArray[i] = (hitBoxTransforms[i].position, hitBoxTransforms[i].rotation);
            }
        }
    }

    public bool IsHitValid(Vector3 colliderPoint1, Vector3 colliderPoint2, float colliderRadius, int oldTimeStamp)
    {
        int deltaFixedUpdateFrames = Mathf.RoundToInt((PhotonNetwork.ServerTimestamp - oldTimeStamp) / 20f); // 1000/50 ms is one fixed update frame
        if(deltaFixedUpdateFrames < NumberOfRecordedHitBoxFrames)
        {
            var info = hitBoxInfoCircularBuffer[deltaFixedUpdateFrames];
            if (info.CanBeInterrupted)
            {
                for (int i = 0; i < validationColliderTransforms.Count; i++)
                {
                    validationColliderTransforms[i].position = info.ColliderTransformArray[i].position;
                    validationColliderTransforms[i].rotation = info.ColliderTransformArray[i].rotation;
                }
                var result = Physics.CheckCapsule(colliderPoint2, colliderPoint1, colliderRadius, 1 << Globals.ValidationLayer);
                return result;
            }
        }
        return false;
    }

    #endregion

    #region Take Damage

    [PunRPC]
    public void TryTakeDamage(float amount, HitDirection direction, Vector3 attackColliderPoint0, Vector3 attackColliderPoint1, float attackColliderRadius, int senderAttackTriggerPhotonViewID, PhotonMessageInfo info)
    {
        if (IsAlive && animationManager.CanBeInterrupted && PhotonView.IsMine && IsHitValid(attackColliderPoint0,  attackColliderPoint1,  attackColliderRadius, info.SentServerTimestamp))
        { 
            PhotonView.RPC(nameof(TakeDamage), RpcTarget.All, amount, direction);
            PhotonView.Find(senderAttackTriggerPhotonViewID).RPC(nameof(AttackTrigger.OnCharacterDamaged), RpcTarget.All, PhotonView.ViewID);
        }
    }

    [PunRPC]
    public void TakeDamage(float amount, HitDirection direction)
    {
        if (animationManager.IsGuarding)
        {
            float guardedAmount = stamina - Mathf.Max(0, stamina - amount);
            stamina -= guardedAmount;
            amount -= guardedAmount;
        }
        health -= amount;
        if (IsAlive)
        {
            animationManager.Impact(stamina > 0 && animationManager.IsGuarding, direction);
            if (stamina <= Globals.CompareDelta && animationManager.IsGuarding)
            {
                if (PhotonView.IsMine)
                {
                    EndGuarding();
                }
                AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.HitOnFlesh);
            }
            else if (stamina > Globals.CompareDelta && animationManager.IsGuarding)
            {
                AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.GuardHit);
            }
            else
            {
                AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.HitOnFlesh);
            }
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

    #endregion

    #region Die and Win

    [PunRPC]
    protected virtual void OnDie(HitDirection direction)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(OnDie), RpcTarget.Others, direction);
        }
        animationManager.Die(direction);
        ClearDestination();
        animationManager.Move(0);
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
        rb.constraints = RigidbodyConstraints.FreezeAll;
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
        var point = interactable.GetClosestInteractionPoint(rb.position);
        if ((point - rb.position).magnitude < interactionRange)
        {
            interactionTarget = interactable;
            Interract();
        }
        else 
        {
            MoveTo(point); 
            interactionTarget = interactable;
            interactionPoint = Destination;
        }
        chaseTarget = null;
    }

    private void UpdateInteractionCheck()
    {
        if (interactionTarget != null)
        {
            if ((interactionPoint - rb.position).magnitude < interactionRange)
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

    public void DrinkFromFountain(Vector3 fountainPosition, float healAmount)
    {
        SetRotationTarget(fountainPosition);
        animationManager.Drink();
        StartCoroutine(TryHealAfterDelay(healAmount));
    }

    private IEnumerator TryHealAfterDelay(float healAmount)
    {
        yield return new WaitForSeconds(fountainHealDelay);
        if (!animationManager.IsInterrupted)
        {
            health = Mathf.Min(health + healAmount, healthMaximum);
        }
    }

    #endregion

    #region Kneel

    public void KneelBeforeStatue(Vector3 statuePosition)
    {
        SetRotationTarget(statuePosition); 
        animationManager.Kneel();
    }

    #endregion

    #endregion

    #region Chase

    public void SetChaseTarget(Transform target)
    {
        chaseTarget = target;
        interactionTarget = null;
    }

    private void UpdateChase()
    {
        if (chaseTarget != null)
        {
            if ((chaseTarget.position - rb.position).magnitude < attackRange)
            {
                TryAttack(chaseTarget.position);
                chaseTarget = null;
                ClearDestination();
            }
            else
            {
                SetDestination(chaseTarget.position);
            }
        }
    }


    #endregion

    #region Movement

    [PunRPC]
    public void SetDestination(Vector3 destination)
    {
        if(PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(SetDestination), RpcTarget.Others, destination);
        }
        Destination = destination;
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
                SetDestination( rb.position + dir * destinationMinimum);
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
            var targetRotation = Quaternion.LookRotation(new Vector3(nextDestination.x, rb.position.y, nextDestination.z) - rb.position);
            movementSpeed = Mathf.Min(movementSpeed + Time.fixedDeltaTime * acceleration, TrySprint() ? sprintingSpeedMaximum : movementSpeedMaximum);
            var moveDirection = (nextDestination - rb.position).normalized;
            var rotationSpeed = Mathf.Lerp(rotationSpeedMinimum, rotationSpeedMaximum, Quaternion.Angle(targetRotation, rb.rotation) / 180f);
            rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.transform.position + movementSpeed * Time.fixedDeltaTime * moveDirection);
        }
        else
        {
            if(animationManager.IsGuarding || animationManager.IsAttacking || animationManager.IsInteracting || forceRotation)
            {
                var targetRotation = Quaternion.LookRotation(new Vector3(rotationTarget.x, rb.position.y, rotationTarget.z) - rb.position);
                if (Quaternion.Angle(targetRotation, rb.rotation) > rotationThreshold && (rotationTarget - rb.position).magnitude > destinationThreshold)
                {
                    var rotationSpeed = Mathf.Lerp(rotationSpeedMinimum, rotationSpeedMaximum, Quaternion.Angle(targetRotation, rb.rotation) / 180f);
                    rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                }
            }
            movementSpeed = Mathf.Max(movementSpeed - Time.fixedDeltaTime * deceleration, 0);
        }
        animationManager.Move(movementSpeed / movementSpeedMaximum);
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

    protected void ClearDestination()
    {
        SetDestination(rb.position);
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

    #region Buffs

    public void AddBuff(Buff buff)
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

    public void RemoveBuffs()
    {
        if(currentBuff != null)
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

    #region Outline

    public void ShowOutLine()
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
