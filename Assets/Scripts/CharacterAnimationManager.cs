using UnityEngine;

[RequireComponent(typeof(Animator))]
/// <summary>
/// Manages the animations of a <see cref="Character"/>.
/// </summary>
public class CharacterAnimationManager : MonoBehaviour
{
    #region Properties and Fields

    private Animator animator;
    public bool CanBeInterrupted => !IsAttacking && !IsInterrupted;
    public bool CanMove => !IsInterrupted && !IsAttacking && !IsGuarding;
    public bool IsInterrupted { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsGuarding { get; private set; }

    #region Animator Constants

    private const string AnimatorMovementSpeed = "MovementSpeed";
    private const string AnimatorIsMoving = "IsMoving";
    private const string AnimatorIsGuarding = "IsGuarding";
    private const string AnimatorAttack1 = "Attack1";
    private const string AnimatorAttack2 = "Attack2";
    private const string AnimatorImpactFrontLeft = "ImpactFrontLeft";
    private const string AnimatorImpactFrontRight = "ImpactFrontRight";
    private const string AnimatorImpactBack = "ImpactBack";
    private const string AnimatorGuardImpactFront = "GuardImpactFront";
    private const string AnimatorGuardImpactBack = "GuardImpactBack";
    private const string AnimatorDieFront = "DieFront";
    private const string AnimatorDieBack = "DieBack";

    #endregion

    #endregion

    #region Methods

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    #region Moving

    public void Move(bool isMoving)
    {
        animator.SetBool(AnimatorIsMoving, isMoving);
    }

    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat(AnimatorMovementSpeed, speed);
    }

    #endregion

    #region Attack

    public void Attack()
    {
        IsAttacking = true;
        animator.SetTrigger(Random.Range(0, 2) == 0 ? AnimatorAttack1 : AnimatorAttack2 );
    }

    public void OnAttackFinished()
    {
        IsAttacking = false;
    }

    #endregion

    #region Guarding
    public void StartGuarding()
    {
        if (!IsGuarding)
        {
            IsGuarding = true;
            animator.SetBool(AnimatorIsGuarding, true);
        }
    }

    public void EndGuarding()
    {
        animator.SetBool(AnimatorIsGuarding, false);
    }

    public void OnGuardFinished()
    {
        IsGuarding = false;
    }

    #endregion
    
    #region Impact

    public void Impact(bool guard, HitDirection direction)
    {
        IsInterrupted = true;
        if (guard)
        {
            animator.SetTrigger(direction == HitDirection.Back ? AnimatorGuardImpactBack : AnimatorGuardImpactFront);
        }
        else 
        {
            switch (direction)
            {
                case HitDirection.Back:
                    animator.SetTrigger(AnimatorImpactBack);
                    break;
                case HitDirection.FrontLeft:
                    animator.SetTrigger(AnimatorImpactFrontLeft);
                    break;
                case HitDirection.FrontRight:
                    animator.SetTrigger(AnimatorImpactFrontRight);
                    break;
                default:
                    Debug.LogWarning("Invalid HitDirection received when trying to trigger impact animation.");
                    break;
            }
        }
    }

    public void OnImpactFinished()
    {
        IsInterrupted = false;
    }

    #endregion

    #region Die

    public void Die(HitDirection direction)
    {
        animator.SetTrigger(direction == HitDirection.Back ? AnimatorDieBack : AnimatorDieFront);
    }

    #endregion

    #endregion

}
