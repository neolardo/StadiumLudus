using UnityEngine;

/// <summary>
/// Constrols the main camera of the game.
/// </summary>
public class CameraController : MonoBehaviour
{
    public Transform character;
    [Tooltip("Represents the relative position from the character.")]
    public Vector3 relativePosition;

    void FixedUpdate()
    {
        transform.position = character.position + relativePosition;
    }
}
