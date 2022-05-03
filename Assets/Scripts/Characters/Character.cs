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

    [Tooltip("Represents the health of the charater.")]
    [SerializeField]
    private float health = 100f;

    [Tooltip("Represents the energy of the charater which alllows it to interract, attack and block.")]
    [SerializeField]
    private float stamina = 100f;

    /// <summary>
    /// Indicates whether this character is alive or not.
    /// </summary>
    public bool IsAlive => health > 0;

    #endregion

    #region Movement

    [Tooltip("Represents the maximum movement speed of the character.")]
    [SerializeField]
    private float movementSpeedMaximum = 3.5f;

    [Tooltip("Represents the acceleration of the character.")]
    [SerializeField]
    private float acceleration = 4f;

    [Tooltip("Represents the deceleration of the character.")]
    [SerializeField]
    private float deceleration = 4f;

    [Tooltip("Represents the rotation speed for the character's movement in deg/sec.")]
    [SerializeField]
    private float rotationSpeed = 540f;

    [Tooltip("Represents the rotation speed of a character during attacking in deg/sec.")]
    [SerializeField]
    protected float attackRotationSpeed = 700f;

    [Tooltip("Represents the rotation speed of a character during guarding in deg/sec.")]
    [SerializeField]
    protected float guardRotationSpeed = 700f;

    [Tooltip("Represents the radial attack range of this character.")]
    [SerializeField]
    protected float attackRange = 1f;

    protected float movementSpeed;
    protected const float positionThreshold = 0.6f;
    protected const float rotationThreshold = 2f;

    private Vector3 _nextPosition;

    /// <summary>
    /// The next position where the character is headed.
    /// </summary>
    public Vector3 NextPosition
    {
        get
        {
            return _nextPosition;
        }
        set
        {
            _nextPosition = value;
            agent.destination = value;
        }
    }

    public CharacterAnimationManager animationManager;
    protected Rigidbody rb;
    private NavMeshAgent agent;

    #endregion

    #endregion

    #region Methods

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        ClearNextPosition();
    }
    private void FixedUpdate()
    {
        if (animationManager.CanMove)
        {
            HandleMove();
        }
    }

    #region Attack

    public bool IsTargetInAttackRange(Vector3 attackTarget)
    {
        return (attackTarget - rb.position).magnitude < attackRange;
    }

    public void Attack(Vector3 attackTarget)
    {
        if (!animationManager.IsInterrupted && !animationManager.IsAttacking)
        {
            ClearNextPosition();
            animationManager.Attack();
            movementSpeed = 0;
            OnAttack(attackTarget);
        }
    }

    protected abstract void OnAttack(Vector3 attackTarget);

    protected IEnumerator RotateToAttackDirection(Vector3 attackTarget)
    {
        attackTarget = new Vector3(attackTarget.x, rb.position.y, attackTarget.z);
        var targetRotation = Quaternion.LookRotation(attackTarget - rb.position);
        while (Quaternion.Angle(targetRotation, rb.rotation) > rotationThreshold)
        {
            rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, attackRotationSpeed * Time.fixedDeltaTime);
            yield return null;
        }
    }

    #endregion

    #region Guarding

    public void StartGuarding()
    {
        if (!animationManager.IsInterrupted && !animationManager.IsGuarding)
        {
            ClearNextPosition();
            animationManager.StartGuarding();
            movementSpeed = 0;
        }
    }

    public void RotateToGuardDirection(Vector3 guardTarget) // TODO: do not rotate after release
    {
        if (animationManager.IsGuarding)
        {
            guardTarget = new Vector3(guardTarget.x, rb.position.y, guardTarget.z);
            var targetRotation = Quaternion.LookRotation(guardTarget - rb.position);
            if (Quaternion.Angle(targetRotation, rb.rotation) > rotationThreshold)
            {
                rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, guardRotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    public void EndGuarding()
    {
        animationManager.EndGuarding();
    }

    #endregion

    #region Take Damage

    public bool TryTakeDamage(float amount, HitDirection direction)
    {
        if (IsAlive && animationManager.CanBeInterrupted)
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
                if (stamina < Globals.CompareDelta && animationManager.IsGuarding)
                {
                    EndGuarding();
                }
            }
            else
            {
                animationManager.Die(direction);
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            return true;
        }
        return false;
    }

    #endregion

    #region Movement

    private void HandleMove()
    {
        var currentNextPosition = agent.path.corners.Length > 0 ? agent.path.corners[0] : NextPosition;
        if ((rb.position - NextPosition).magnitude > positionThreshold)
        {
            var targetRotation = Quaternion.LookRotation( currentNextPosition - rb.position);
            rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            movementSpeed = Mathf.Min(movementSpeed + Time.fixedDeltaTime * acceleration, movementSpeedMaximum);
            var dir = rb.transform.forward;
            rb.MovePosition(rb.transform.position + movementSpeed * Time.fixedDeltaTime * dir);
            animationManager.Move(true);
        }
        else
        {
            ClearNextPosition();
            movementSpeed = Mathf.Max(movementSpeed - Time.fixedDeltaTime * deceleration, 0);
            animationManager.Move(false);
        }
        animationManager.SetMovementSpeed(movementSpeed / movementSpeedMaximum);
    }

    protected void ClearNextPosition()
    {
        NextPosition = rb.position;
    }

    #endregion

    #endregion
}
