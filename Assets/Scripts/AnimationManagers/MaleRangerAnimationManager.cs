using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations of a <see cref="MaleRangerCharacter"/>.
/// </summary>
public class MaleRangerAnimationManager : CharacterAnimationManager
{
    #region Fields and Properties

    #region Animator Constants

    private const string AnimatorDash = "Dash";
    private const string AnimatorReload = "Reload";

    #endregion

    #endregion

    #region Methods

    #region Init


    protected override void Start()
    {
        base.Start();
    }

    #endregion

    #region Reload

    public void Reload()
    {
        animator.SetTrigger(AnimatorReload);
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
