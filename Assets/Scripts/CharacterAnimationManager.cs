using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations of a <see cref="Character"/>.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterAnimationManager : MonoBehaviour
{
    #region Properties and Fields

    private Animator animator;

    [Tooltip("Indicates the number of available attack animations for this character.")]
    [SerializeField]
    private int attackAnimationCount;

    /// <summary>
    /// Indicates whether the character can be interrupted or not.
    /// </summary>
    public bool CanBeInterrupted => !IsAttacking && !IsInterrupted;

    /// <summary>
    /// Indicates whether the character can move or not.
    /// </summary>
    public bool CanMove => !IsInterrupted && !IsAttacking && !IsGuarding;

    /// <summary>
    /// Indicates whether the character can deal damage currntly or not.
    /// </summary>
    public bool CanDealDamage {get; private set; }

    /// <summary>
    /// Indicates whether the character is currently interruped or not.
    /// </summary>
    public bool IsInterrupted { get; private set; }

    /// <summary>
    /// Indicates whether the character is currently attacking or not.
    /// </summary>
    public bool IsAttacking { get; private set; }

    /// <summary>
    /// Indicates whether the character is currently guarding or not.
    /// </summary>
    public bool IsGuarding { get; private set; }

    /// <summary>
    /// Represents the list of currently active custom states of the character's animations.
    /// </summary>
    public List<string> CustomStates { get; private set; }

    #region Animator Constants

    private const string AnimatorMovementSpeed = "MovementSpeed";
    private const string AnimatorIsGuarding = "IsGuarding";
    private const string AnimatorAttack = "Attack";
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

    #region Init

    private void Start()
    {
        animator = GetComponent<Animator>();
        CustomStates = new List<string>();
    }

    #endregion

    #region Moving

    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat(AnimatorMovementSpeed, speed);
    }

    #endregion

    #region Attack

    public void Attack()
    {
        IsAttacking = true;
        animator.SetTrigger($"{AnimatorAttack}{Random.Range(1, attackAnimationCount+1)}");
    }

    public void OnAttackCanDealDamage()
    {
        CanDealDamage = true;
    }

    public void OnAttackCannotDealDamage()
    {
        CanDealDamage = false;
    }

    public void OnAttackFinished()
    {
        IsAttacking = false;
        CanDealDamage = false;
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
                    Debug.LogWarning($"Invalid {nameof(HitDirection)} received when trying to trigger impact animation.");
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

    #region Custom

    /// <summary>
    /// Sets a custom trigger for the animator.
    /// </summary>
    /// <param name="animatorTrigger">The name of the animation trigger.</param>
    /// <param name="storeState">Indicates whether the this animation begins a new custom state which should be stored.</param>
    public void SetCustomTrigger(string animatorTrigger, bool storeState = false)
    {
        animator.SetTrigger(animatorTrigger);
        if(storeState)
        {
            CustomStates.Add(animatorTrigger); 
        }
    }


    /// <summary>
    /// Sets a custom boolean for the animator.
    /// </summary>
    /// <param name="propertyName">The name of the boolean.</param>
    /// <param name="value">The value of the boolean.</param>
    /// <param name="storeState">Indicates whether the this animation begins a new custom state which should be stored.</param>
    public void SetCustomBoolean(string propertyName, bool value, bool storeState = false)
    {
        animator.SetBool(propertyName, value);
        if (storeState)
        {
            CustomStates.Add(propertyName);
        }
    }

    /// <summary>
    /// Turns off a custom boolean from the animator.
    /// </summary>
    /// <param name="propertyName">The name of the boolean.</param>
    public void TurnOffBoolean(string propertyName)
    {
        animator.SetBool(propertyName, false);
    }

    /// <summary>
    /// Called whenever a custom stated was left.
    /// </summary>
    /// <param name="animatorTrigger">The name of the animation trigger which started the state.</param>
    public void OnCustomStateLeft(string animatorTrigger)
    {
        CustomStates.Remove(animatorTrigger);
    }

    #endregion

    #endregion

}
