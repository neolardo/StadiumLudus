using System;
using UnityEngine;

/// <summary>
/// Constrols the main camera of the game.
/// </summary>
public class CameraController : MonoBehaviour
{ 
    #region Properties and Fields

    private Transform characterTransform;
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
    private bool hasInitialized = false;

    #endregion

    #region Methods

    public void Initialize(Character character)
    {
        this.characterTransform = character.GetComponent<Transform>();
        relativePosition = new Vector3(0, lookAtHeight, 0) + new Vector3(0, Mathf.Sin(angleFromCharacter / 180 * Mathf.PI), Mathf.Cos(angleFromCharacter/180 * Mathf.PI)) * distanceFromCharacter;
        transform.position = characterTransform.position + relativePosition;
        transform.LookAt(characterTransform.position + new Vector3(0, lookAtHeight, 0));
        hasInitialized = true;
    }

    void Update()
    {
        if(hasInitialized)
        {
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        if (Input.GetMouseButton(2))
        {
            relativePosition = Quaternion.AngleAxis(angularSpeed * Time.fixedDeltaTime * (lastMousePositionX - Input.mousePosition.x), Vector3.up) * relativePosition;
            transform.position = characterTransform.position + relativePosition;
            transform.LookAt(characterTransform.position + new Vector3(0, lookAtHeight, 0));
        }
        else
        {
            transform.position = characterTransform.position + relativePosition;
        }
        lastMousePositionX = Input.mousePosition.x;
    }

    #endregion
}
