using System;
using UnityEngine;

/// <summary>
/// Constrols the main camera of the game.
/// </summary>
public class CameraController : MonoBehaviour
{
    #region Properties and Fields

    private new Camera camera;
    private Transform characterTransform;
    [Header("Camera")]
    [Tooltip("Represents the camera's angle from the character.")]
    public float minimumAngleFromCharacter = 40;
    [Tooltip("Represents the camera's angle from the character.")]
    public float maximumAngleFromCharacter = 50;
    private float angleFromCharacter;
    [Tooltip("Represents the camera's distance from the character.")]
    public float minimumDistanceFromCharacter = 4f;
    [Tooltip("Represents the camera's distance from the character.")]
    public float maximumDistanceFromCharacter = 7;
    private float distanceFromCharacter;
    [Tooltip("Represents the height where the camera is focusing.")]
    public float lookAtHeight = 1.0f;
    [Tooltip("Represents angular speed of the rotation.")]
    public float angularSpeed = 10;
    [Tooltip("Represents strength of zooming.")]
    public float zoomMultiplier = 2f;

    [Header("Character Light")]
    [Tooltip("The transform of the character light.")]
    public Transform characterLightTransform;
    [Tooltip("Represents the character light's angle from the camera.")]
    public float lightAngleFromCamera = 50;
    [Tooltip("Represents the character light's vertical distance from the character.")]
    public float lightVerticalDistanceFromCharacter = 3;
    [Tooltip("Represents the character light's horizontal distance from the character.")]
    public float lightHorizontalDistanceFromCharacter = 2;

    private Vector3 relativePosition;
    private float lastMousePositionX;
    private bool hasInitialized = false;
    private float zoomValue = 1;
    private float zoomTarget = 1;
    private const float minimumZoomDelta = 0.01f;
    private const float minimumZoomStep = 0.01f;
    private float currentRotationAngle;

    #endregion

    #region Methods

    public void Initialize(Character character)
    {
        angleFromCharacter = Mathf.Lerp(minimumAngleFromCharacter, maximumAngleFromCharacter, zoomValue);
        distanceFromCharacter = Mathf.Lerp(minimumDistanceFromCharacter, maximumDistanceFromCharacter, zoomValue);
        camera = GetComponent<Camera>();
        characterTransform = character.GetComponent<Transform>();
        RefreshCameraTransform();
        hasInitialized = true;
    }

    void Update()
    {
        if (hasInitialized)
        {
            UpdateCameraPosition();
            UpdateCharacterLightPosition();
        }
    }

    #region Camera

    private void UpdateCameraPosition()
    {
        if (Input.mouseScrollDelta.y < -Globals.CompareDelta && zoomTarget < 1)
        {
            zoomTarget = Mathf.Min(1, zoomTarget + zoomMultiplier * Time.deltaTime);
        }
        else if (Input.mouseScrollDelta.y > Globals.CompareDelta && zoomTarget > 0)
        {
            zoomTarget = Mathf.Max(0, zoomTarget - zoomMultiplier * Time.deltaTime);
        }
        if (!Mathf.Approximately(zoomValue, zoomTarget))
        {
            float delta = Mathf.Abs(zoomValue - zoomTarget) / 5f;
            if (zoomValue > zoomTarget)
            {
                zoomValue = delta > minimumZoomDelta ? zoomValue - delta : Mathf.Min(zoomValue - minimumZoomStep, zoomTarget);
            }
            else
            {
                zoomValue = delta > minimumZoomDelta ? zoomValue + delta : Mathf.Max(zoomValue + minimumZoomDelta, zoomTarget);
            }
            angleFromCharacter = Mathf.Lerp(minimumAngleFromCharacter, maximumAngleFromCharacter, zoomValue);
            distanceFromCharacter = Mathf.Lerp(minimumDistanceFromCharacter, maximumDistanceFromCharacter, zoomValue);
            RefreshCameraTransform();
        }
        if (Input.GetMouseButton(2))
        {
            currentRotationAngle = (currentRotationAngle + angularSpeed * Time.fixedDeltaTime * (lastMousePositionX - Input.mousePosition.x)) % 360f;
            RefreshCameraTransform();
        }
        else
        {
            transform.position = characterTransform.position + relativePosition;
        }
        lastMousePositionX = Input.mousePosition.x;
    }

    private void RefreshCameraTransform()
    {
        relativePosition = Quaternion.AngleAxis(currentRotationAngle, Vector3.up) * (new Vector3(0, lookAtHeight, 0) + new Vector3(0, Mathf.Sin(angleFromCharacter / 180 * Mathf.PI), Mathf.Cos(angleFromCharacter / 180 * Mathf.PI)) * distanceFromCharacter);
        transform.position = characterTransform.position + relativePosition;
        transform.LookAt(characterTransform.position + new Vector3(0, lookAtHeight, 0));
        var characterScreenPosition = camera.WorldToScreenPoint(characterTransform.position);
    }

    #endregion

    #region Character Light

    private void UpdateCharacterLightPosition()
    {
        var dirToCamera = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(characterTransform.position.x, 0, characterTransform.position.z)).normalized;
        characterLightTransform.position = (characterTransform.position + Quaternion.AngleAxis(lightAngleFromCamera, Vector3.up) * dirToCamera * lightHorizontalDistanceFromCharacter + Vector3.up * lightVerticalDistanceFromCharacter);
        characterLightTransform.LookAt(characterTransform.position + lookAtHeight * Vector3.up);
    }

    #endregion

    #endregion
}
