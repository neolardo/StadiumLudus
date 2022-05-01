using UnityEngine;

/// <summary>
/// Represents a playable character of this game.
/// </summary>
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

    public bool IsAlive => health > 0;

    #endregion

    #region Movement

    public Vector2 NextPosition { get; set; }
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
        var currentPos = new Vector2(rb.position.x, rb.position.z);
        if ((currentPos - NextPosition).magnitude > positionThreshold)
        {
            var targetRotation = Quaternion.LookRotation(new Vector3(NextPosition.x, 0, NextPosition.y) - rb.position);
            transform.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            movementSpeed = Mathf.Min(movementSpeed + Time.fixedDeltaTime * acceleration, movementSpeedMaximum);
            transform.position += rb.transform.forward * movementSpeed * Time.fixedDeltaTime;
            animationManager.Move(true);
        }
        else
        {
            movementSpeed = Mathf.Max(movementSpeed - Time.fixedDeltaTime * deceleration, 0);
            animationManager.Move(false);
        }
        animationManager.SetMovementSpeed(movementSpeed / movementSpeedMaximum);
    }

    private void ClearNextPosition()
    {
        NextPosition = new Vector2(rb.position.x, rb.position.z);
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
