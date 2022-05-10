using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male ranger character.
/// </summary>
public class MaleRangerCharacter : Character
{
    #region Properties and Fields

    public Projectile boltProjectile;

    public AttackTrigger boltTrigger;

    public Crossbow crossbow;

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

    private bool isArrowLoaded; 

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        boltTrigger.MinimumDamage = boltMinimumDamage;
        boltTrigger.MaximumDamage = boltMaximumDamage;
        boltProjectile.Force = boltForce;
        if (boltMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Bolt maximum damage for a male ranger character is set to a non-positive value.");
        }
        if (boltMaximumDamage < boltMinimumDamage)
        {
            Debug.LogWarning("Bolt maximum damage for a male ranger character is set to a lesser value than the minimum.");
        }
    }

    #region Attack

    public override bool TryAttack(Vector3 attackTarget)
    {
        if (!animationManager.IsInterrupted && !animationManager.IsAttacking && !crossbow.IsReloading)
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
            ClearNextPosition();
            animationManager.CustomTrigger(AnimatorReload);
            crossbow.Reload();
            isArrowLoaded = true;
        }
        StartCoroutine(RotateToAttackDirection(attackTarget));
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
