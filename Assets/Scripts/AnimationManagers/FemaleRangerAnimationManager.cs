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

    protected const string AnimatorDash = "Dash";

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
        animator.SetTrigger(AnimatorDash);
    }

    #endregion

    #endregion
}
