using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations of a <see cref="Character"/>.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterAnimationManager : MonoBehaviour
{
    #region Properties and Fields

    protected Animator animator;

    [Tooltip("Indicates the number of available attack animations for this character.")]
    [SerializeField]
    protected int attackAnimationCount;

    [Tooltip("The audio source of the character.")]
    [SerializeField]
    private AudioSource characterAudioSource;

    /// <summary>
    /// Indicates whether the character can be interrupted or not.
    /// </summary>
    public bool CanBeInterrupted { get; protected set; } = true;

    /// <summary>
    /// Indicates whether the movement is locked by an animation.
    /// </summary>
    public bool IsMovementLocked { get; protected set; }

    /// <summary>
    /// Indicates whether the character can deal damage currntly or not.
    /// </summary>
    public bool CanDealDamage { get; protected set; }

    /// <summary>
    /// Indicates whether the character is currently interruped or not.
    /// </summary>
    public bool IsInterrupted { get; protected set; }

    /// <summary>
    /// Indicates whether the character is currently attacking or not.
    /// </summary>
    public bool IsAttacking { get; protected set; }

    /// <summary>
    /// Indicates whether the character is currently guarding or not.
    /// </summary>
    public bool IsGuarding { get; protected set; }

    /// <summary>
    /// Indicates whether this character is currently jumping or not.
    /// </summary>
    public bool IsJumping { get; protected set; }

    /// <summary>
    /// Indicates whether this character is currently using of it's skills.
    /// </summary>
    public bool IsUsingSkill { get; protected set; }

    /// <summary>
    /// Indicates whether this character is currently interacting or not.
    /// </summary>
    public bool IsInteracting { get; protected set; }

    /// <summary>
    /// If the movement speed is bigger than this value than the character should be animatated as moving.
    /// </summary>
    protected const float MovementSpeedThreshold = 0.1f;

    /// <summary>
    /// If the movement speed is bigger than this value than the character should be animatated as sprinting.
    /// </summary>
    protected const float SprintingThreshold = 1f;

    private bool canPlayStepSound = true;
    private const float stepSoundCooldown = .15f;

    #region Animator Constants

    protected const string AnimatorIsMoving = "IsMoving";
    protected const string AnimatorIsSprinting = "IsSprinting";
    protected const string AnimatorIsGuarding = "IsGuarding";
    protected const string AnimatorStartGuard = "StartGuard";
    protected const string AnimatorAttack = "Attack";
    protected const string AnimatorImpactFrontLeft = "ImpactFrontLeft";
    protected const string AnimatorImpactFrontRight = "ImpactFrontRight";
    protected const string AnimatorImpactBack = "ImpactBack";
    protected const string AnimatorGuardImpactFront = "GuardImpactFront";
    protected const string AnimatorGuardImpactBack = "GuardImpactBack";
    protected const string AnimatorDieFront = "DieFront";
    protected const string AnimatorDieBack = "DieBack";
    protected const string AnimatorDrink = "Drink";
    protected const string AnimatorKneel = "Kneel";
    protected const string AnimatorMovementLayerName = "MovementLayer";
    protected int animatorMovementLayerIndex;

    #endregion

    #endregion

    #region Methods

    #region Init

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        animatorMovementLayerIndex = animator.GetLayerIndex(AnimatorMovementLayerName);
        StartCoroutine(ManageStepSoundCooldown());
    }

    #endregion

    #region Movement

    public void Move(float speed)
    {
        animator.SetLayerWeight(animatorMovementLayerIndex, Mathf.Min(1.0f, speed));
        animator.SetBool(AnimatorIsMoving, speed > MovementSpeedThreshold);
        animator.SetBool(AnimatorIsSprinting, speed > SprintingThreshold);
    }

    public void OnLockMovement()
    {
        IsMovementLocked = true;
    }

    public void OnUnlockMovement()
    {
        IsMovementLocked = false;
    }

    #endregion

    #region Step

    public void OnStep()
    {
        if (canPlayStepSound)
        {
            AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.StepOnStone, doNotRepeat: true);
            canPlayStepSound = false;
        }
    }

    private IEnumerator ManageStepSoundCooldown()
    {
        while (true)
        {
            if (!canPlayStepSound)
            {
                yield return new WaitForSeconds(stepSoundCooldown);
                canPlayStepSound = true;
            }
            yield return null;
        }
    }

    #endregion

    #region Jumping

    public void OnJumpingStarted()
    {
        IsJumping = true;
    }

    public void OnJumpingEnded()
    {
        IsJumping = false;
    }

    #endregion

    #region Attack

    public void Attack()
    {
        IsAttacking = true;
        animator.SetTrigger($"{AnimatorAttack}{Random.Range(1, attackAnimationCount + 1)}");
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
            animator.SetTrigger(AnimatorStartGuard);
        }
    }

    public void EndGuarding()
    {
        IsGuarding = false; 
        animator.SetBool(AnimatorIsGuarding, false);
    }

    public void OnGuardFinished()
    {
        IsGuarding = false;
    }

    #endregion

    #region Skill
    public void OnSkillFinished()
    {
        IsUsingSkill = false;
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
            IsGuarding = false;
        }
        IsAttacking = false;
        IsUsingSkill = false;
        IsInteracting = false;
        CanDealDamage = false;
        IsJumping = false;
    }

    public void OnImpactFinished()
    {
        IsInterrupted = false;
    }

    #endregion

    #region Interrupt

    public void OnCanBeInterrupted()
    {
        CanBeInterrupted = true;
    }
    public void OnCannotBeInterrupted()
    {
        CanBeInterrupted = false;
    }

    #endregion

    #region Die

    public void Die(HitDirection direction)
    {
        animator.SetTrigger(direction == HitDirection.Back ? AnimatorDieBack : AnimatorDieFront);
    }

    #endregion

    #region Interactions

    public void Drink()
    {
        animator.SetTrigger(AnimatorDrink);
        IsInteracting = true;
    }
    public void Kneel()
    {
        animator.SetTrigger(AnimatorKneel);
        IsInteracting = true;
    }

    public void OnInteractionFinished() 
    {
        IsInteracting = false;
    }

    #endregion

    #endregion
}
