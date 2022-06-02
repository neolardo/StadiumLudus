using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male ranger character.
/// </summary>
public class MaleRangerCharacter : Character
{
    #region Properties and Fields

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

    private const string AnimatorReload = "Reload";

    protected override bool CanMove => base.CanMove && !crossbow.IsReloading;

    private bool isArrowLoaded;

    private bool hasInitialized;

    #endregion

    #region Methods

    #region Init

    protected void OnEnable()
    {
        // order is important
        if (!hasInitialized)
        {
            Initialize();
            hasInitialized = true;
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
    }

    #endregion

    #region Skills

    public override void FireSkill(int skillNumber, Vector3 clickPosition)
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Attack

    public override bool TryAttack(Vector3 attackTarget)
    {
        if (!animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !crossbow.IsReloading)
        {
            OnAttack(attackTarget);
            return true;
        }
        return false;
    }

    protected override void OnAttack(Vector3 attackTarget)
    {
        if (isArrowLoaded)
        {
            base.OnAttack(attackTarget);
            crossbow.Attack();
            isArrowLoaded = false;
        }
        else
        {
            ClearDestination();
            //animationManager.SetCustomTrigger(AnimatorReload);
            crossbow.Reload();
            isArrowLoaded = true;
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
