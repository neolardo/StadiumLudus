using UnityEngine;
/// <summary>
/// Manages the animations of a <see cref="RangerCharacter"/>.
/// </summary>
public class RangerAnimationManager : CharacterAnimationManager
{
    #region Fields and Properties

    public bool IsDrawing { get; private set; }

    #region Animator Constants

    private const string AnimatorIsDrawing = "IsDrawing";
    private const string AnimatorDash = "Dash";
    private const string AnimatorIsDashing = "IsDashing";
    private const string AnimatorSmoke = "Smoke";
    private const string AnimatorTrap = "Trap";

    #endregion

    #endregion

    #region Methods

    #region Draw

    public void SetIsDrawing(bool value)
    {
        animator.SetBool(AnimatorIsDrawing, value);
        IsDrawing = value;
    }

    #endregion

    #region Dash

    public void Dash()
    {
        IsUsingSkill = true;
        animator.SetTrigger(AnimatorDash);
        SetIsDashing(true);
    }

    public void SetIsDashing(bool value)
    {
        animator.SetBool(AnimatorIsDashing, value);
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
