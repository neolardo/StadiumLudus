using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a pool of <see cref="Projectile"/>s.
/// </summary>
public class ProjectilePoolManager : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("Indicates how many projectiles should be in the pool.")]
    [SerializeField]
    private int numberOfProjectiles;

    [Tooltip("The prefab of a projectile.")]
    [SerializeField]
    private GameObject projectilePrefab;

    [Tooltip("The character which fires projectiles.")]
    public GameObject character;

    [Tooltip("The transform of the spawn zone with the correct starting position and rotation of the projectile.")]
    public Transform spawnZone;

    /// <summary>
    /// Indicates the starting force of the projectiles.
    /// </summary>
    public float Force { get; set; }

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

    #endregion

    #region Methods

    private void Start()
    {
        inactiveProjectiles = new List<Projectile>();
        activeProjectiles = new List<Projectile>();
        if (numberOfProjectiles <= 0)
        {
            Debug.LogWarning($"The number of projectiles of a {character.name} is set to a non-positive value.");
        }
        CreateProjectiles();
    }

    /// <summary>
    /// Instantiates the <see cref="Projectile"/>s of this pool.
    /// </summary>
    private void CreateProjectiles()
    {
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            var proj = Instantiate(projectilePrefab, transform).GetComponent<Projectile>();
            proj.ProjectilePool = this;
            proj.projectileTrigger.MinimumDamage = MinimumDamage;
            proj.projectileTrigger.MaximumDamage = MaximumDamage;
            proj.projectileTrigger.character = character;
            inactiveProjectiles.Add(proj);
        }
    }

    /// <summary>
    /// Fires an available <see cref="Projectile"/>.
    /// </summary>
    public void Fire()
    {
        GetNextAvailableProjectile().gameObject.SetActive(true);
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
            projectile.gameObject.SetActive(false);
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
        activeProjectiles.Remove(projectile);
        inactiveProjectiles.Add(projectile);
    }

    #endregion
}
