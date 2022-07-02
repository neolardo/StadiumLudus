using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a projectile weapon.
/// To fire a projectile set it's gameobject to active.
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Properties and Fields

    private Rigidbody rb;
    private new CapsuleCollider collider;
    private new Transform transform;

    [Tooltip("The attack trigger of the projectile.")]
    public AttackTrigger projectileTrigger;

    [Tooltip("The sound effect of the projectile.")]
    public SFX projectileSFX;

    /// <summary>
    /// The projectile pool.
    /// </summary>
    public ProjectilePoolManager ProjectilePool { get; set; }

    /// <summary>
    /// Indicates the maximum distance until an arrow can travel.
    /// After that the arrow will be deactivated.
    /// </summary>
    private const float distanceMaximum = 20;

    public PhotonView photonView;

    private bool hasInitialized = false;

    private bool doesRaycastTargetExists = false;

    private Vector3 potetentialHit;

    private const float triggerDelaySecondsAfterHit = 0.2f;

    private bool IsStopped => rb.isKinematic;

    #endregion

    #region Methods

    [PunRPC]
    public void EnableProjectile()
    {
        gameObject.SetActive(true);
    }

    [PunRPC]
    public void DisableProjectile()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            transform = GetComponent<Transform>();
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
            rb.isKinematic = true;
            if (!photonView.IsMine)
            {
                Destroy(rb);
            }
            hasInitialized = true;
        }
        Fire();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || other.gameObject.transform.IsChildOf(ProjectilePool.characterTransform) || other.CompareTag(Globals.CharacterTag) || other.CompareTag(Globals.IgnoreBoxTag))
        {
            return;
        }
        else if (!IsStopped)
        {
            Stop();
            if (doesRaycastTargetExists)
            {
                rb.position = potetentialHit;
                transform.position = potetentialHit;
            }
            else
            {
                rb.position = transform.position;
            }
            transform.parent = other.transform;
            string fullPath = GameRoundManager.Instance.GetTransformFullPath(other.transform);
            Debug.Log($"Projectile hit target: {fullPath}");
            photonView.RPC(nameof(SetParentTransform), RpcTarget.Others, fullPath);
            StartCoroutine(DelayAttackTriggerAfterHit());
        }
    }

    [PunRPC]
    public void SetParentTransform(string parentFullPath)
    {
        var newParent = GameRoundManager.Instance.FindTransformByFullPath(parentFullPath);
        if (newParent != null)
        {
            transform.parent = newParent;
        }
    }

    [PunRPC]
    public void OnCharacterDamaged(int characterPhotonViewID)
    {
        projectileTrigger.OnCharacterDamaged(characterPhotonViewID);
    }

    private IEnumerator DelayAttackTriggerAfterHit()
    {
        yield return new WaitForSeconds(triggerDelaySecondsAfterHit);
        projectileTrigger.IsActive = false;
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine && rb.isKinematic == false && (rb.position - ProjectilePool.characterTransform.position).magnitude > distanceMaximum)
        {
            photonView.RPC(nameof(OnProjectileWentTooFar), RpcTarget.All);
        }
    }

    [PunRPC]
    public void OnProjectileWentTooFar()
    {
        Debug.Log("Projectile went too far, it's been deactivated.");
        if (photonView.IsMine)
        {
            Stop();
            ProjectilePool.OnProjectileDisappeared(this);
        }
        gameObject.SetActive(false);
    }    

    private void Fire()
    {
        AudioManager.Instance.PlayOneShotSFX(projectileTrigger.audioSource, projectileSFX, doNotRepeat:true);
        gameObject.transform.parent = ProjectilePool.gameObject.transform;
        if(photonView.IsMine)
        {
            projectileTrigger.IsActive = false;
            gameObject.transform.parent = ProjectilePool.gameObject.transform;
            rb.constraints = RigidbodyConstraints.None;
            rb.position = ProjectilePool.spawnZone.transform.position;
            rb.rotation = ProjectilePool.spawnZone.transform.rotation;
            transform.position = ProjectilePool.spawnZone.transform.position;
            transform.rotation = ProjectilePool.spawnZone.transform.rotation;
            rb.isKinematic = false;
            RaycastHit hit;
            int layerMask = ~((1 << Globals.CharacterLayer) | (1 << Globals.IgnoreRaycastLayer)); //  ignore character rb and arrows
            var dir = (ProjectilePool.characterTransform.forward * distanceMaximum + 0.5f * Vector3.down).normalized;
            if (Physics.Raycast(rb.position, dir, out hit, distanceMaximum, layerMask))
            {
                doesRaycastTargetExists = true;
                potetentialHit = hit.point;
            }
            else
            {
                doesRaycastTargetExists = false;
            }
            projectileTrigger.IsActive = true;
            rb.AddForce(ProjectilePool.characterTransform.forward * ProjectilePool.Force, ForceMode.Impulse);
        }
    }

    private void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
    }

    #endregion

}
