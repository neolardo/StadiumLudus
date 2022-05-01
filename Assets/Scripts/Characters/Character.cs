using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a playable character of this game.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class Character : MonoBehaviour
{
    #region Properties and fields

    #region Stats

    [Tooltip("Represents the health of the charater.")]
    public float health = 100;
    [Tooltip("Represents the energy of the charater which alllows it to interract, attack and block.")]
    public float stamina = 100;
    [Tooltip("Represents the rotation speed of the character in deg/sec.")]
    public float rotationSpeed = 360;
    [Tooltip("Represents the maximum movement speed of the character.")]
    public float movementSpeedMaximum = 3.5f;
    [Tooltip("Represents the acceleration of the character.")]
    public float acceleration = 4f;
    [Tooltip("Represents the deceleration of the character.")]
    public float deceleration = 4f;

    protected Rigidbody rb;

    public CharacterAnimationManager animationManager;
    public NavMeshAgent agent;
    public GameObject debugNextPoint;

    public bool IsAlive => health > 0;

    #endregion

    #region Movement

    private Vector3 _nextPosition;
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
    protected float movementSpeed;
    private const float positionThreshold = 0.6f;

    #endregion

    #endregion

    #region Methods

    void Start()
    {
        OnStart();
    }

    protected virtual void OnStart()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        ClearNextPosition();
    }

    #region Attack
    public abstract bool TryAttack();

    #endregion

    #region Take Damage

    public void TakeDamage(float amount, HitDirection direction)
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
                    animationManager.Guard(false);
                }
            }
            else
            {
                animationManager.Die(direction);
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }

    #endregion

    #region Guarding

    public bool TryStartGuarding()
    {
        if (!animationManager.IsInterrupted)
        {
            animationManager.Guard(true);
            movementSpeed = 0;
            return true;
        }
        return false;
    }

    public void EndGuarding()
    {
        animationManager.Guard(false);
    }

    #endregion

    #region Movement

    private void HandleMove()
    {
        var currentNextPosition = agent.path.corners.Length > 0 ? agent.path.corners[0] : NextPosition;
        Debug.Log($"current next pos: { currentNextPosition}");
        debugNextPoint.transform.position = currentNextPosition;
        if ((rb.position - NextPosition).magnitude > positionThreshold)
        {
            var targetRotation = Quaternion.LookRotation( currentNextPosition - rb.position);
            rb.transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            movementSpeed = Mathf.Min(movementSpeed + Time.fixedDeltaTime * acceleration, movementSpeedMaximum);
            rb.MovePosition(rb.transform.position + movementSpeed * Time.fixedDeltaTime * (currentNextPosition - rb.transform.position).normalized);//(rb.transform.position + movementSpeed * Time.fixedDeltaTime * rb.transform.forward);//(rb.transform.position +  ( currentNextPosition - rb.transform.position ) * Time.fixedDeltaTime * 10);
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

    private void FixedUpdate()
    {
        if (animationManager.CanMove)
        {
            HandleMove();
        }
    }

    #endregion

    #endregion
}
