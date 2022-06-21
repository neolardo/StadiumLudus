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

    /// <summary>
    /// The projectile pool.
    /// </summary>
    public ProjectilePoolManager ProjectilePool { get; set; }

    /// <summary>
    /// Indicates the maximum distance until an arrow can travel.
    /// After that the arrow will be deactivated.
    /// </summary>
    private const float distanceMaximum = 20;

    private bool hasInitialized = false;

    private bool doesRaycastTargetExists = false;

    private Vector3 potetentialHit;

    private const float triggerDelaySecondsAfterHit = 0.2f;

    private bool IsStopped => rb.isKinematic;

    #endregion

    #region Methods

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            transform = GetComponent<Transform>();
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
            rb.isKinematic = true;
            hasInitialized = true;
        }
        Fire();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.IsChildOf(ProjectilePool.characterTransform) || other.CompareTag(Globals.CharacterTag) || other.CompareTag(Globals.IgnoreBoxTag))
        {
            return;
        }
        else if(!IsStopped)
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
            StartCoroutine(DelayAttackTriggerAfterHit());
        }
    }

    private IEnumerator DelayAttackTriggerAfterHit()
    {
        yield return new WaitForSeconds(triggerDelaySecondsAfterHit);
        projectileTrigger.IsActive = false;
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic == false && (rb.position - ProjectilePool.characterTransform.position).magnitude > distanceMaximum)
        {
            Debug.Log("Projectile went too far, it's been deactivated.");
            Stop();
            ProjectilePool.OnProjectileDisappeared(this);
            gameObject.SetActive(false);
        }
    }
    private void Fire()
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
        int layerMask = ~ ((1 << Globals.CharacterLayer) | (1 << Globals.IgnoreRaycastLayer)); //  ignore character rb and arrows
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
        AudioManager.Instance.PlayOneShotSFX(projectileTrigger.audioSource, SFX.Projectile);
        rb.AddForce(ProjectilePool.characterTransform.forward * ProjectilePool.Force, ForceMode.Impulse);
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
