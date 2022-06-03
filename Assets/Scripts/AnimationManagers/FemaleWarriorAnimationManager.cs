using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations of a <see cref="FemaleWarriorCharacter"/>.
/// </summary>
public class FemaleWarriorAnimationManager : CharacterAnimationManager
{
    #region Fields and Properties

    /// <summary>
    /// Indicates whether continuing the combo attack is requested or not.
    /// </summary>
    public bool IsContinueAttackRequested { get; private set; }

    #region Animator Constants

    protected const string AnimatorLeapAttack = "LeapAttack";
    protected const string AnimatorContinueComboAttack = "ContinueAttack";

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


    #endregion

    #region Combo Attack

    public void SetContinueComboAttack(bool value)
    {
        animator.SetBool(AnimatorContinueComboAttack, value);
        IsContinueAttackRequested = value;
    }


    public void OnComboAttackContinued()
    {
        SetContinueComboAttack(false);
    }

    #endregion

    #endregion
}
