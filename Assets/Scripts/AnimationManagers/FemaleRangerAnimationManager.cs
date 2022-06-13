using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations of a <see cref="FemaleRangerCharacter"/>.
/// </summary>
public class FemaleRangerAnimationManager : CharacterAnimationManager
{
    #region Fields and Properties

    #region Animator Constants

    private const string AnimatorDash = "Dash";
    private const string AnimatorSmoke = "Smoke";
    private const string AnimatorTrap = "Trap";

    #endregion

    #endregion

    #region Methods

    #region Init


    protected override void Start()
    {
        base.Start();
    }

    #endregion

    #region Dash

    public void Dash()
    {
        IsUsingSkill = true;
        animator.SetTrigger(AnimatorDash);
    }

    #endregion


    #region Smoke

    public void Smoke()
    {
        IsUsingSkill = true;
        animator.SetTrigger(AnimatorSmoke);
    }

    #endregion


    #region PlaceTrap

    public void PlaceTrap()
    {
        IsUsingSkill = true;
        animator.SetTrigger(AnimatorTrap);
    }

    #endregion

    #endregion
}
