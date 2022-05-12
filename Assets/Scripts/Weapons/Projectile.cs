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

    [Tooltip("The attack trigger of the projectile.")]
    [SerializeField]
    private AttackTrigger projectileTrigger;

    [Tooltip("The character which spawns the arrows.")]
    [SerializeField]
    private GameObject character;

    [Tooltip("Represents the zone where this arrow should be spawned at.")]
    [SerializeField]
    private GameObject spawnZone;

    /// <summary>
    /// Indicates the starting force of the arrow.
    /// </summary>
    public float Force { get; set; }

    /// <summary>
    /// Indicates the maximum distance until an arrow can travel.
    /// After that the arrow will be deactivated.
    /// </summary>
    private const float distanceMaximum = 20;

    private bool hasInitialized = false;

    #endregion

    #region Methods

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
            rb.isKinematic = true;
            hasInitialized = true;
        }
        Fire();
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == character)
        {
            Physics.IgnoreCollision(collider, collision.collider);
            return;
        }
        else
        {
            Stop();
            rb.position = collision.contacts[0].point;
            gameObject.transform.parent = collision.transform;
        }
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic == false && (rb.position - character.transform.position).magnitude > distanceMaximum)
        {
            Debug.Log("Arrow went too far, it's been deactivated.");
            Stop();
            gameObject.SetActive(false);
        }
    }
    private void Fire()
    {
        gameObject.transform.parent = null;
        projectileTrigger.IsActive = false; // TODO
        rb.position = spawnZone.transform.position;
        rb.rotation = spawnZone.transform.rotation;
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;
        projectileTrigger.IsActive = true;
        rb.AddForce(character.transform.forward * Force, ForceMode.Impulse);
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
