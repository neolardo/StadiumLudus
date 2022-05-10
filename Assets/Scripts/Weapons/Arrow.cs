using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the fireing of an arrow.
/// </summary>
public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private new CapsuleCollider collider;

    [Tooltip("The character which spawns the arrows.")]
    [SerializeField]
    private GameObject character;

    [Tooltip("Represents the zone where this arrow should be spawned at.")]
    [SerializeField]
    private GameObject spawnZone;

    /// <summary>
    /// Indicates the starting force of the arrow.
    /// </summary>
    public float Force {get;set;}

    /// <summary>
    /// Indicates the maximum distance until an arrow can travel.
    /// After that the arrow will be deactivated.
    /// </summary>
    private const float distanceMaximum = 20;

    private bool hasInitialized = false;

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
        rb.position = spawnZone.transform.position;
        rb.rotation = spawnZone.transform.rotation;
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;
        rb.AddForce(character.transform.forward * Force, ForceMode.Impulse);
    }

    private void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        rb.isKinematic = true;
        hasInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == character) 
        {
            Physics.IgnoreCollision(collider, other);
            return;
        }
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic == false && (rb.position - character.transform.position).magnitude > distanceMaximum)
        {
            Debug.Log("Arrow went too far, it's been deactivated.");
            rb.isKinematic = true;
            gameObject.SetActive(false);
        }
    }
}
