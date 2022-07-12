using Photon.Pun;
using UnityEngine;

/// <summary>
/// Represents an abstract poolable object of an <see cref="ObjectPoolManager{T}"/>.
/// </summary>
public abstract class PoolableObject : MonoBehaviour
{
    #region Fields And Properties

    public PhotonView photonView;

    #endregion

    #region Methods

    [PunRPC]
    public virtual void EnableObject()
    {
        gameObject.SetActive(true);
    }

    [PunRPC]
    public virtual void DisableObject()
    {
        gameObject.SetActive(true);
    }

    #endregion
}
