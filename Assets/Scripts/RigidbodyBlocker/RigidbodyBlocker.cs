using UnityEngine;

/// <summary>
/// Prevents the rigidbody blocker to collide with it's parent rigidbody.
/// </summary>
public class RigidbodyBlocker : MonoBehaviour
{
    [SerializeField] private Collider colliderA;
    [SerializeField] private Collider colliderB;
    void Awake()
    {
        Physics.IgnoreCollision(colliderA, colliderB, true);
    }
}
