/// <summary>
/// An <see cref="CharacterAnimationManager"/> for any <see cref="WarriorCharacter"/>.
/// </summary>
public class WarriorAnimationManager : CharacterAnimationManager
{
    #region Fields and Properties

    #region Animator Constants

    /// <summary>
    /// Indicates whether the whirlwind animation is ongoing or not.
    /// </summary>
    public bool IsWhirlwindOnGoing { get; private set; }

    protected const string AnimatorLeapAttack = "LeapAttack";
    protected const string AnimatorStartWhirlwind = "StartWhirlwind";
    protected const string AnimatorEndWhirlwind = "EndWhirlwind";
    protected const string AnimatorGroundSlam = "GroundSlam";

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
        IsUsingSkill = true;
        animator.SetTrigger(AnimatorLeapAttack);
    }

    #endregion

    #region Whirlwind

    public void StartWhirlwind()
    {
        IsUsingSkill = true;
        IsWhirlwindOnGoing = true;
        animator.SetTrigger(AnimatorStartWhirlwind);
    }


    public void EndWhirlwind()
    {
        animator.SetTrigger(AnimatorEndWhirlwind);
        IsWhirlwindOnGoing = false;
    }

    #endregion

    #region Ground Slam

    public void GroundSlam()
    {
        IsUsingSkill = true;
        animator.SetTrigger(AnimatorGroundSlam);
    }

    #endregion

    #endregion
}
