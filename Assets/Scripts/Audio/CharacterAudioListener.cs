using UnityEngine;

/// <summary>
/// Manages the <see cref="AudioListener"/> of the controller <see cref="Character"/>.
/// </summary>
[RequireComponent(typeof(AudioListener))]
public class CharacterAudioListener : MonoBehaviour
{
    #region Properties and Fields

    private new Transform transform;
    private Transform cameraTransform;
    private Transform target;
    private readonly Vector3 positionDelta = Vector3.up * 1;

    #endregion

    #region Methods

    void Start()
    {
        transform = GetComponent<Transform>();
        cameraTransform = Camera.main.transform;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void Update()
    {
        if(target != null)
        {
            transform.position = target.position + positionDelta;
            Vector3 dir = target.position - cameraTransform.position;
            transform.LookAt(transform.position + new Vector3(dir.x, transform.position.y, dir.z).normalized, Vector3.up);
        }
    }

    #endregion
}
