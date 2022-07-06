using Photon.Pun;
using UnityEngine;

/// <summary>
/// Manages a male ranger character.
/// </summary>
public class MaleRangerCharacter : RangerCharacter
{
    #region Properties and Fields

    private MaleRangerAnimationManager maleRangerAnimationManager;

    public override CharacterFightingStyle FightingStyle => CharacterFightingStyle.Heavy;

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

    protected override bool CanAttack => base.CanAttack && !crossbow.IsReloading && IsArrowLoaded;
    
    private bool CanReload => IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill && !animationManager.IsInteracting && !crossbow.IsReloading && !IsArrowLoaded;

    #region Skills

    #region Dash
    protected override float DashJumpingTime => 0.29f;

    #endregion

    #region Trap

    protected override float TrapPlacementDelay => 0.7f;

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Init

    protected override void Awake()
    {
        base.Awake();
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

    #region Attack

    public override bool TryAttack(Vector3 attackTarget)
    {
        if (CanAttack)
        {
            OnAttack(attackTarget);
            return true;
        }
        else if (CanReload)
        {
            Reload();
            return true;
        }
        return false;
    }

    protected override void OnAttack(Vector3 attackTarget)
    {
        FireBolt(attackTarget);
    }

    [PunRPC]
    public void FireBolt(Vector3 attackTarget)
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(FireBolt), RpcTarget.Others, attackTarget);
        }
        base.OnAttack(attackTarget);
        crossbow.Attack();
        IsArrowLoaded = false;
    }

    [PunRPC]
    public void Reload()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(Reload), RpcTarget.Others);
        }
        maleRangerAnimationManager.Reload();
        crossbow.Reload();
        IsArrowLoaded = true;
    }

    #endregion

    #region Die

    [PunRPC]
    protected override void OnDie(HitDirection direction)
    {
        base.OnDie(direction);
        crossbow.Die(direction);
    }

    #endregion

    #endregion
}
