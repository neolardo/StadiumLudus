using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract generic pool of <see cref="PoolableObject"/>s.
/// </summary>
/// <typeparam name="T">The type of the pooled objects.</typeparam>

public abstract class ObjectPoolManager<T> : MonoBehaviour where T:PoolableObject
{
    #region Fields and Properties

    [Tooltip("The transform of the spawn zone with the correct starting position and rotation of the object.")]
    public Transform spawnZone;

    [Tooltip("The transform of the object container.")]
    public Transform container;

    protected bool isPhotonViewMine;

    /// <summary>
    /// The list containing the currently inactive <see cref="PoolableObject"/>s.
    /// </summary>
    protected List<T> inactiveObjects;

    /// <summary>
    /// The list containing the currently active <see cref="PoolableObject"/>s.
    /// </summary>
    protected List<T> activeObjects;

    #endregion

    #region Methods

    protected virtual void Start()
    {
        inactiveObjects = new List<T>();
        activeObjects = new List<T>();
        if (container.childCount == 0)
        {
            Debug.LogWarning($"The number of poolable objects is set to 0.");
        }
        InitializePoolableObjects();
    }

    protected abstract void InitializePoolableObjects();

    /// <summary>
    /// Spawns an object from the pool.
    /// </summary>
    public virtual void SpawnObject()
    {
        if (isPhotonViewMine)
        {
            var obj = GetNextAvailableObject();
            obj.photonView.RPC(nameof(PoolableObject.EnableObject), Photon.Pun.RpcTarget.All);
        }
    }

    /// <summary>
    /// Gets the next availabe <see cref="PoolableObject"/> if one exists, otherwise get's the earliest used <see cref="PoolableObject"/>.
    /// </summary>
    /// <returns>The next available <see cref="PoolableObject"/>.</returns>
    protected T GetNextAvailableObject()
    {
        if (inactiveObjects.Count == 0)
        {
            var obj = activeObjects[0];
            activeObjects.Remove(obj);
            obj.photonView.RPC(nameof(PoolableObject.DisableObject), Photon.Pun.RpcTarget.All);
            activeObjects.Add(obj);
            return obj;
        }
        else
        {
            var obj = inactiveObjects[0];
            inactiveObjects.Remove(obj);
            activeObjects.Add(obj);
            return obj;
        }
    }

    /// <summary>
    /// Called whenever a <see cref="PoolableObject"/> has got deactivated.
    /// </summary>
    /// <param name="obj">The <see cref="T"/> object.</param>
    public void OnObjectDisappeared(T obj)
    {
        if (isPhotonViewMine)
        {
            activeObjects.Remove(obj);
            inactiveObjects.Add(obj);
        }
    }

    #endregion
}
