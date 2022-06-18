/// <summary>
/// Manages the animations of a <see cref="MaleRangerCharacter"/>.
/// </summary>
public class MaleRangerAnimationManager : RangerAnimationManager
{
    #region Fields and Properties

    #region Animator Constants

    private const string AnimatorReload = "Reload";

    #endregion

    #endregion

    #region Methods

    #region Reload

    public void Reload()
    {
        animator.SetTrigger(AnimatorReload);
    }

    #endregion

    #endregion
}
