using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a pool of <see cref="Projectile"/>s.
/// </summary>
public class ProjectilePoolManager : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("The transform of the character which fires projectiles.")]
    public Transform characterTransform;

    [Tooltip("The transform of the spawn zone with the correct starting position and rotation of the projectile.")]
    public Transform spawnZone;

    [Tooltip("The transform of the projectile container.")]
    public Transform projectileContainer;

    /// <summary>
    /// Represents the minimum damage of a fired projectile.
    /// </summary>
    public float MinimumDamage { get; set; }

    /// <summary>
    /// Represents the maximum damage of a fired projectile.
    /// </summary>
    public float MaximumDamage { get; set; }

    /// <summary>
    /// The list contianing the currently inactive <see cref="Projectile"/>s.
    /// </summary>
    private List<Projectile> inactiveProjectiles;

    /// <summary>
    /// The list contianing the currently active <see cref="Projectile"/>s.
    /// </summary>
    private List<Projectile> activeProjectiles;

    private bool isPhotonViewMine;

    #endregion

    #region Methods

    private void Start()
    {
        inactiveProjectiles = new List<Projectile>();
        activeProjectiles = new List<Projectile>();
        if (projectileContainer.childCount == 0)
        {
            Debug.LogWarning($"The number of projectiles of a {characterTransform.name} is 0.");
        }
        isPhotonViewMine = characterTransform.GetComponent<Character>().PhotonView.IsMine;
        InitializeProjectiles();
    }

    /// <summary>
    /// Instantiates the <see cref="Projectile"/>s of this pool.
    /// </summary>
    private void InitializeProjectiles()
    {
        for (int i = 0; i < projectileContainer.childCount; i++)
        {
            var proj = projectileContainer.GetChild(i).GetComponent<Projectile>();
            proj.ProjectilePool = this;
            proj.projectileTrigger.MinimumDamage = MinimumDamage;
            proj.projectileTrigger.MaximumDamage = MaximumDamage;
            inactiveProjectiles.Add(proj);
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
            var proj = GetNextAvailableProjectile();
            proj.photonView.RPC(nameof(Projectile.EnableProjectile), Photon.Pun.RpcTarget.All, attackTarget == null ? -1 : attackTarget.PhotonView.ViewID);
        }
    }

    /// <summary>
    /// Gets the next availabe <see cref="Projectile"/> if one exists, otherwise get's the earliest used <see cref="Projectile"/>.
    /// </summary>
    /// <returns>The next available <see cref="Projectile"/>.</returns>
    private Projectile GetNextAvailableProjectile()
    {
        if (inactiveProjectiles.Count == 0)
        {
            var projectile = activeProjectiles[0];
            activeProjectiles.Remove(projectile);
            projectile.photonView.RPC(nameof(Projectile.DisableProjectile), Photon.Pun.RpcTarget.All);
            activeProjectiles.Add(projectile);
            return projectile;
        }
        else
        {
            var projectile = inactiveProjectiles[0];
            inactiveProjectiles.Remove(projectile);
            activeProjectiles.Add(projectile);
            return projectile;
        }
    }

    /// <summary>
    /// Called whenever a <see cref="Projectile"/> went too far so it got deactivated.
    /// </summary>
    /// <param name="projectile">The <see cref="Projectile"/>.</param>
    public void OnProjectileDisappeared(Projectile projectile)
    {
        if (isPhotonViewMine)
        {
            activeProjectiles.Remove(projectile);
            inactiveProjectiles.Add(projectile);
        }
    }

    #endregion
}
