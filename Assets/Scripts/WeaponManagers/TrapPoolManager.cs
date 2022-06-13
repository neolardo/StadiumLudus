using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a pool of <see cref="Trap"/>s.
/// </summary>
public class TrapPoolManager : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("Indicates how many traps should be in the pool.")]
    [SerializeField]
    private int numberOfTraps;

    [Tooltip("The prefab of a trap.")]
    [SerializeField]
    private GameObject trapPrefab;

    [Tooltip("The transform of the character which places traps.")]
    public Transform characterTransform;

    /// <summary>
    /// Represents the minimum damage of a <see cref="Trap"/>.
    /// </summary>
    public float MinimumDamage { get; set; }

    /// <summary>
    /// Represents the maximum damage of a <see cref="Trap"/>.
    /// </summary>
    public float MaximumDamage { get; set; }

    /// <summary>
    /// Represents the active duration of a <see cref="Trap"/>.
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    /// The list contianing the currently inactive <see cref="Trap"/>s.
    /// </summary>
    private List<Trap> inactiveTraps;

    /// <summary>
    /// The list contianing the currently active <see cref="Trap"/>s.
    /// </summary>
    private List<Trap> activeTraps;

    #endregion

    #region Methods

    private void Start()
    {
        inactiveTraps = new List<Trap>();
        activeTraps = new List<Trap>();
        if (numberOfTraps <= 0)
        {
            Debug.LogWarning($"The number of traps of a {characterTransform.name} is set to a non-positive value.");
        }
        CreateTraps();
    }

    /// <summary>
    /// Instantiates the <see cref="Trap"/>s of this pool.
    /// </summary>
    private void CreateTraps()
    {
        for (int i = 0; i < numberOfTraps; i++)
        {
            var trap = Instantiate(trapPrefab, null).GetComponent<Trap>();
            trap.trapPool = this;
            trap.trapTrigger.MinimumDamage = MinimumDamage;
            trap.trapTrigger.MaximumDamage = MaximumDamage;
            trap.activeDuration = Duration;
            trap.trapTrigger.characterTransform = characterTransform;
            inactiveTraps.Add(trap);
        }
    }

    /// <summary>
    /// Places a new <see cref="Trap"/> on the ground and activates it.
    /// </summary>
    /// <param name="delaySeconds">The optional delay before placing the trap.</param>
    public void PlaceTrap(float delaySeconds = 0f)
    {
        StartCoroutine(PlaceTrapAfterDelay(delaySeconds));
    }

    private IEnumerator PlaceTrapAfterDelay(float delaySeconds)
    {
        var trap = GetNextAvailableTrap();
        yield return new WaitForSeconds(delaySeconds);
        trap.transform.position = characterTransform.position;
        trap.transform.rotation = characterTransform.rotation;
        trap.gameObject.SetActive(true);
    }

    /// <summary>
    /// Gets the next availabe <see cref="Trap"/> if one exists, otherwise get's the earliest used <see cref="Trap"/>.
    /// </summary>
    /// <returns>The next available <see cref="Trap"/>.</returns>
    private Trap GetNextAvailableTrap()
    {
        if (inactiveTraps.Count == 0)
        {
            var trap = activeTraps[0];
            activeTraps.Remove(trap);
            trap.DeactivateTrap(false);
            activeTraps.Add(trap);
            return trap;
        }
        else
        {
            var trap = inactiveTraps[0];
            inactiveTraps.Remove(trap);
            activeTraps.Add(trap);
            return trap;
        }
    }

    /// <summary>
    /// Called whenever a <see cref="Trap"/> got deactivated.
    /// </summary>
    /// <param name="trap">The <see cref="Trap"/>.</param>
    public void OnTrapDisappeared(Trap trap)
    {
        activeTraps.Remove(trap);
        inactiveTraps.Add(trap);
    }

    #endregion
}
