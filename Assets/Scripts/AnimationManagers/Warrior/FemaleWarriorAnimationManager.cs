/// <summary>
/// Manages the animations of a <see cref="FemaleWarriorCharacter"/>.
/// </summary>
public class FemaleWarriorAnimationManager : WarriorAnimationManager
{
    #region Fields and Properties

    /// <summary>
    /// Indicates whether continuing the combo attack is requested or not.
    /// </summary>
    public bool IsContinueAttackRequested { get; private set; }
    public bool CanLeftWeaponDealDamage { get; private set; }
    public bool CanRightWeaponDealDamage { get; private set; }

    public override bool CanDealDamage => CanLeftWeaponDealDamage || CanRightWeaponDealDamage;

    #region Animator Constants

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

    #region Left Weapon

    public void OnLeftWeaponCanDealDamage()
    {
        CanLeftWeaponDealDamage = true;
    }
    public void OnLeftWeaponCannotDealDamage()
    {
        CanLeftWeaponDealDamage = false;
    }

    #endregion

    #region Right Weapon

    public void OnRightWeaponCanDealDamage()
    {
        CanRightWeaponDealDamage = true;
    }
    public void OnRightWeaponCannotDealDamage()
    {
        CanRightWeaponDealDamage = false;
    }

    #endregion

    #endregion

    #endregion
}
