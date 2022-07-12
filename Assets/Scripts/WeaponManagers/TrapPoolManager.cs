using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a pool of <see cref="Trap"/>s.
/// </summary>
public class TrapPoolManager : ObjectPoolManager<Trap>
{
    #region Properties and Fields

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

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        isPhotonViewMine = inactiveObjects[0].photonView.IsMine;
    }

    /// <summary>
    /// Initializes the <see cref="Trap"/>s of this pool.
    /// </summary>
    protected override void InitializePoolableObjects()
    {
        var traps = new List<Trap>();
        for (int i = 0; i < container.childCount; i++)
        {
            traps.Add(container.GetChild(i).GetComponent<Trap>());
        }
        foreach(var trap in traps)
        {
            trap.gameObject.transform.parent = null;
            trap.trapPool = this;
            trap.trapTrigger.MinimumDamage = MinimumDamage;
            trap.trapTrigger.MaximumDamage = MaximumDamage;
            trap.activeDuration = Duration;
            inactiveObjects.Add(trap);
        }
    }

    /// <summary>
    /// Places a new <see cref="Trap"/> on the ground and activates it.
    /// </summary>
    /// <param name="delaySeconds">The optional delay before placing the trap.</param>
    public void PlaceTrap(float delaySeconds = 0f)
    {
        if (isPhotonViewMine)
        {
            StartCoroutine(PlaceTrapAfterDelay(delaySeconds));
        }
    }

    private IEnumerator PlaceTrapAfterDelay(float delaySeconds)
    {
        var obj = GetNextAvailableObject();
        yield return new WaitForSeconds(delaySeconds);
        if (!activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);
            inactiveObjects.Remove(obj);
        }
        obj.photonView.RPC(nameof(PoolableObject.EnableObject), Photon.Pun.RpcTarget.All);
    }

    #endregion
}
