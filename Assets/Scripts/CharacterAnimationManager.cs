using UnityEngine;

/// <summary>
/// Manages the animatations of a <see cref="Character"/>.
/// </summary>
public class CharacterAnimationManager : MonoBehaviour
{
    public bool CanBeInterrupted => !IsAttacking && !IsInterrupted;
    public bool CanMove => !IsInterrupted && !IsAttacking && !IsGuarding;
    public bool IsInterrupted { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsGuarding { get; private set; }

    private const string AnimatorMovementSpeed = "MovementSpeed";
    private const string AnimatorIsMoving = "IsMoving";
    private const string AnimatorIsGuarding = "IsGuarding";
    private const string AnimatorAttack = "Attack";
    private const string AnimatorImpact = "Impact";
    private const string AnimatorGuardImpact = "GuardImpact";
    private const string AnimatorDead = "Die";
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    #region Attack

    public void Attack()
    {
        IsAttacking = true;
        animator.SetTrigger(AnimatorAttack);
    }

    public void OnAttackFinished()
    {
        IsAttacking = false;
    }

    #endregion

    #region Guarding

    public void Guard(bool isGuarding)
    {
        if (isGuarding)
        {
            IsGuarding = true;
        }
        animator.SetBool(AnimatorIsGuarding, isGuarding);
    }

    public void OnGuardFinished()
    {
        IsGuarding = false;
    }

    #endregion

    #region Move

    public void Move(bool isMoving)
    {
        animator.SetBool(AnimatorIsMoving, isMoving);
    }


    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat(AnimatorMovementSpeed, speed);
    }

    #endregion

    #region Impact and Die

    public void Impact(bool guard, HitDirection direction)
    {
        IsInterrupted = true;
        animator.SetTrigger(AnimatorImpact);
    }

    public void OnImpactFinished()
    {
        IsInterrupted = false;
    }

    public void Die(HitDirection direction)
    {
        animator.SetTrigger(AnimatorDead);
    }

    #endregion

}
