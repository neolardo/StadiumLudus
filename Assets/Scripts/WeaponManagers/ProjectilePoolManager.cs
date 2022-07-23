using UnityEngine;
/// <summary>
/// Manages a pool of <see cref="Projectile"/>s.
/// </summary>
public class ProjectilePoolManager : ObjectPoolManager<Projectile>
{
    #region Properties and Fields

    [Tooltip("The transform of the owner character.")]
    public Transform ownerTransform;

    /// <summary>
    /// Represents the minimum damage of a fired projectile.
    /// </summary>
    public float MinimumDamage { get; set; }

    /// <summary>
    /// Represents the maximum damage of a fired projectile.
    /// </summary>
    public float MaximumDamage { get; set; }

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        isPhotonViewMine = inactiveObjects[0].photonView.IsMine;
    }

    /// <summary>
    /// Initializes the <see cref="Projectile"/>s of this pool.
    /// </summary>
    protected override void InitializePoolableObjects()
    {
        for (int i = 0; i < container.childCount; i++)
        {
            var proj = container.GetChild(i).GetComponent<Projectile>();
            proj.ProjectilePool = this;
            proj.projectileTrigger.MinimumDamage = MinimumDamage;
            proj.projectileTrigger.MaximumDamage = MaximumDamage;
            proj.projectileTrigger.ownerTransform = ownerTransform; 
            inactiveObjects.Add(proj);
        }
    }

    /// <summary>
    /// Fires an available <see cref="Projectile"/>.
    /// </summary>
    /// <param name="attackTarget">The potential target.</param>
    public void Fire(Character attackTarget = null)
    {
        if (isPhotonViewMine)
        {
            var proj = GetNextAvailableObject();
            proj.photonView.RPC(nameof(Projectile.EnableProjectile), Photon.Pun.RpcTarget.All, attackTarget == null ? -1 : attackTarget.PhotonView.ViewID);
        }
    }

    #endregion
}
