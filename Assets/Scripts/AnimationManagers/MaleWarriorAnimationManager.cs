using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations of a <see cref="MaleWarriorCharacter"/>.
/// </summary>
public class MaleWarriorAnimationManager : CharacterAnimationManager
{
    #region Fields and Properties

    /// <summary>
    /// Indicates whether this character is currently jumping or not.
    /// </summary>
    public bool IsJumping { get; private set; }

    #region Animator Constants

    protected const string AnimatorLeapAttack = "LeapAttack";

    #endregion

    #endregion

    #region Methods

    #region Init


    protected override void Start()
    {
        base.Start();
    }

    #endregion

    #region Leap Attack

    public void LeapAttack()
    {
        IsAttacking = true;
        animator.SetTrigger(AnimatorLeapAttack);
    }

    public void OnJumpingStarted()
    {
        IsJumping = true;
    }

    public void OnJumpingEnded()
    {
        IsJumping = false;
    }

    #endregion

    #endregion
}
