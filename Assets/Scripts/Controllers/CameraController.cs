using System;
using UnityEngine;

/// <summary>
/// Constrols the main camera of the game.
/// </summary>
public class CameraController : MonoBehaviour
{
    private Transform character;
    [Tooltip("Represents the camera's angle from the character.")]
    public float angleFromCharacter = 45;
    [Tooltip("Represents the camera's distance from the character.")]
    public float distanceFromCharacter = 5;
    [Tooltip("Represents the height where the camera is focusing.")]
    public float lookAtHeight = 1.0f;
    [Tooltip("Represents angular speed of the rotation.")]
    public float angularSpeed = 10;

    private Vector3 relativePosition;
    private float lastMousePositionX;

    private void Start()
    {
        try
        {
            character = FindObjectOfType<CharacterController>().gameObject.transform;
        }
        catch (Exception)
        {
            Debug.LogError("No character controller found.");
        }
        relativePosition = new Vector3(0, lookAtHeight, 0) + new Vector3(0, Mathf.Sin(angleFromCharacter / 180 * Mathf.PI), Mathf.Cos(angleFromCharacter/180 * Mathf.PI)) * distanceFromCharacter;
        transform.position = character.position + relativePosition;
        transform.LookAt(character.position + new Vector3(0, lookAtHeight, 0));
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(2))
        {
            relativePosition = Quaternion.AngleAxis(angularSpeed * Time.fixedDeltaTime * (lastMousePositionX - Input.mousePosition.x), Vector3.up) * relativePosition;
            transform.position = character.position + relativePosition;
            transform.LookAt(character.position + new Vector3(0, lookAtHeight, 0));
        }
        else
        {
            transform.position = character.position + relativePosition;
        }
        lastMousePositionX = Input.mousePosition.x;
    }
}
