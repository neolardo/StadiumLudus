using UnityEngine;

/// <summary>
/// Manages a male ranger character.
/// </summary>
public class MaleRangerCharacter : Character
{
    #region Properties and Fields

    private MaleRangerAnimationManager maleRangerAnimationManager;
    [Header("Bolt")]
    public Crossbow crossbow;

    [Tooltip("The bolt pool manager.")]
    [SerializeField]
    public ProjectilePoolManager boltPool;

    [Tooltip("Represents the minimum damage of a fired bolt.")]
    [SerializeField]
    private float boltMinimumDamage;

    [Tooltip("Represents the maximum damage of a fired bolt.")]
    [SerializeField]
    private float boltMaximumDamage;

    [Tooltip("Represents the force of a fired bolt.")]
    [SerializeField]
    private float boltForce = 3;

    private bool IsArrowLoaded { get; set; }

    private bool HasInitialized { get; set; }

    #endregion

    #region Methods

    #region Init

    protected void OnEnable()
    {
        // order is important
        if (!HasInitialized)
        {
            Initialize();
            HasInitialized = true;
        }
    }

    private void Initialize()
    {
        if (boltMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Bolt maximum damage for a male ranger character is set to a non-positive value.");
        }
        if (boltMaximumDamage < boltMinimumDamage)
        {
            Debug.LogWarning("Bolt maximum damage for a male ranger character is set to a lesser value than the minimum.");
        }
        if (boltForce <= 0)
        {
            Debug.LogWarning("Bolt force for a male ranger character is set to non-positive value.");
        }
        boltPool.MinimumDamage = boltMinimumDamage;
        boltPool.MaximumDamage = boltMaximumDamage;
        boltPool.Force = boltForce;
        maleRangerAnimationManager = animationManager as MaleRangerAnimationManager;
    }

    #endregion

    #region Skills

    public override void StartSkill(int skillNumber, Vector3 clickPosition)
    {
        throw new System.NotImplementedException();
    }

    public override int InitialChargeCountOfSkill(int skillNumber)
    {
        return 0;
    }

    public override bool IsSkillChargeable(int skillNumber)
    {
        return false;
    }

    #endregion

    #region Attack

    public override bool TryAttack(Vector3 attackTarget)
    {
        if (!maleRangerAnimationManager.IsInterrupted && !maleRangerAnimationManager.IsAttacking && !maleRangerAnimationManager.IsGuarding && !crossbow.IsReloading)
        {
            OnAttack(attackTarget);
            return true;
        }
        return false;
    }

    protected override void OnAttack(Vector3 attackTarget)
    {
        if (IsArrowLoaded)
        {
            base.OnAttack(attackTarget);
            crossbow.Attack();
            IsArrowLoaded = false;
        }
        else
        {
            ClearDestination();
            maleRangerAnimationManager.Reload(); ;
            crossbow.Reload();
            IsArrowLoaded = true;
        }
    }


    #endregion

    #region Die

    protected override void OnDie(HitDirection direction)
    {
        base.OnDie(direction);
        crossbow.Die(direction);
    }

    #endregion

    #endregion
}
