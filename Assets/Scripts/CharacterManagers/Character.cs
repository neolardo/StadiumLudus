using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
/// <summary>
/// Represents a playable character of the game.
/// </summary>
public abstract class Character : MonoBehaviour
{
    #region Properties and fields

    #region Stats

    [Tooltip("Represents the maximum health of the charater.")]
    [SerializeField]
    private float healthMaximum = 100f;

    private float health;

    [Tooltip("Represents the maximum stamina of the charater which allows it to interract, attack and block.")]
    [SerializeField]
    private float staminaMaximum = 100f;

    private float stamina;

    /// <summary>
    /// Indicates whether this character is alive or not.
    /// </summary>
    public bool IsAlive => health > 0;

    /// <summary>
    /// The relative normalized health this character currently has.
    /// </summary>
    public float HealthRatio => health / healthMaximum;

    /// <summary>
    /// The relative normalized stamina this character currently has.
    /// </summary>
    public float StaminaRatio => stamina / staminaMaximum;

    #endregion

    #region Movement

    /// <summary>
    /// Inidicates whether this character can move or not.
    /// </summary>
    protected virtual bool CanMove => animationManager.CanMove;

    [Tooltip("Represents the maximum movement speed of the character.")]
    [SerializeField]
    private float movementSpeedMaximum = 3.5f;

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
    protected const float destinationThreshold = 0.1f;
    protected const float destinationMinimum = 0.7f;
    protected const float rotationThreshold = 2f;

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

    [HideInInspector]
    public new Transform transform;
    public CharacterAnimationManager animationManager;
    protected Rigidbody rb;
    private NavMeshAgent agent;
    private Transform chaseTarget;
    private Vector3 rotationTarget;
    private NavMeshPath helperPath;


    #endregion

    #region Interactions

    private Interactable interactionTarget;
    private Vector3 interactionPoint;
    protected const float interactionRange = 0.1f;


    #endregion

    #endregion

    #region Methods

    protected virtual void Start()
    {
        health = healthMaximum;
        stamina = staminaMaximum;
        transform = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        helperPath = new NavMeshPath();
        var characterScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        characterPositionRatioOnScreen = new Vector2(0.5f, characterScreenPosition.y / Screen.height);
        ClearDestination();
    }
    protected virtual void FixedUpdate()
    {
        UpdateInteractionCheck();
        UpdateChase();
        UpdateMove();
    }

    #region Skills

    public abstract void FireSkill(int skillNumber, Vector3 clickPosition);

    #endregion

    #region Attack

    public virtual bool TryAttack(Vector3 attackTarget)
    {
        if (IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding)
        {
            OnAttack(attackTarget);
            return true;
        }
        return false;
    }

    protected virtual void OnAttack(Vector3 attackTarget)
    {
        animationManager.Attack();
        rotationTarget = attackTarget;
    }

    #endregion

    #region Guarding

    public void StartGuarding()
    {
        if (IsAlive && !animationManager.IsInterrupted && !animationManager.IsGuarding && !animationManager.IsAttacking)
        {
            animationManager.StartGuarding();
        }
    }

    public void SetRotationTarget(Vector3 guardTarget) 
    {
        rotationTarget = guardTarget;
    }

    public void EndGuarding()
    {
        animationManager.EndGuarding();
    }

    #endregion

    #region Take Damage

    public bool TryTakeDamage(float amount, HitDirection direction)
    {
        if (IsAlive) // && CanBeInterrupted
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
                    EndGuarding();
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
                OnDie(direction);
            }
            return true;
        }
        return false;
    }

    protected virtual void OnDie(HitDirection direction)
    {
        animationManager.Die(direction);
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    #endregion

    #region Interactions

    public void SetInteractionTarget(Interactable interactable)
    {
        MoveTo(interactable.GetClosestInteractionPoint(rb.position));
        interactionTarget = interactable;
        interactionPoint = Destination;
        chaseTarget = null;
    }

    private void UpdateInteractionCheck()
    {
        if (interactionTarget != null)
        {
            if ((interactionPoint - rb.position).magnitude < interactionRange)
            {
                interactionTarget.TryInteract(this);
                interactionTarget = null;
                ClearDestination();
            }
        }
    }

    #region Drink

    public void DrinkFromFountain(Vector3 fountainPosition)
    {
        rotationTarget = fountainPosition;
        animationManager.Drink();
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
                Destination = chaseTarget.position;
            }
        }
    }


    #endregion

    #region Movement

    public void MoveTo(Vector3 position)
    {
        agent.CalculatePath(position, helperPath);
        if (helperPath.status == NavMeshPathStatus.PathComplete)
        {
            var lastCorner = helperPath.corners[helperPath.corners.Length - 1];
            if ((lastCorner - rb.position).magnitude < destinationMinimum)
            {
                var dir = new Vector3(characterPositionRatioOnScreen.x - Input.mousePosition.x / Screen.width, 0, characterPositionRatioOnScreen.y - Input.mousePosition.y / Screen.height).normalized;
                Destination = rb.position + dir * destinationMinimum;
            }
            else
            {
                Destination = lastCorner;
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
            movementSpeed = Mathf.Min(movementSpeed + Time.fixedDeltaTime * acceleration, movementSpeedMaximum);
            var moveDirection = (nextDestination - rb.position).normalized;
            var rotationSpeed = Mathf.Lerp(rotationSpeedMinimum, rotationSpeedMaximum, Quaternion.Angle(targetRotation, rb.rotation) / 180f);
            rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.transform.position + movementSpeed * Time.fixedDeltaTime * moveDirection);
        }
        else
        {
            if(animationManager.IsGuarding || animationManager.IsAttacking || animationManager.IsInteracting)
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
        animationManager.SetMovementSpeed(movementSpeed / movementSpeedMaximum);
    }

    protected void ClearDestination()
    {
        Destination = rb.position;
    }

    #endregion

    #endregion
}
