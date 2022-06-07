using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Manages an interactable statue.
/// </summary>
public class Statue : Interactable
{
    #region Properties and Fields


    [Tooltip("The transform of the orb.")]
    [SerializeField]
    private Transform orbTransform;

    [Tooltip("The visual effect component of the orb.")]
    [SerializeField]
    private VisualEffect orbVFX;


    [Tooltip("The light component of the orb.")]
    [SerializeField]
    private Light orbLight;

    [Tooltip("The distance of the range where a player can interact with the statue.")]
    [SerializeField]
    private float interactionRangeDistance = 1f;

    [Tooltip("The speed of the orb moving to the character while kneeling.")]
    [SerializeField]
    private float orbMovingSpeed = 5f;

    [Tooltip("The scaling speed of the orb.")]
    [SerializeField]
    private float orbScalingSpeed = 5f;

    [Tooltip("The speed of the orb light changing it's intesity.")]
    [SerializeField]
    private float orbIntensityChangeSpeed = 5f;


    [Tooltip("The target height delta of the orb.")]
    [SerializeField]
    private float targetHeightDelta = 1f;

    private const float positionDelta = 0.1f;
    private const float animationDelay = 0.9f;
    private const float vfxInitialSize = 0.5f;
    private const float vfxTargetSize = 12f;
    private const float orbLightInitialInternsity = 0.5f;

    /// <summary>
    /// Indicates whether the effect is active or not.
    /// </summary>
    private bool IsEffectActive { get; set; }

    
    /// <summary>
    /// Represents the cached transform reference.
    /// </summary>
    [HideInInspector]
    public new Transform transform;

    #endregion

    #region Methods


    private void Start()
    {
        transform = GetComponent<Transform>();
        IsEffectActive = true;
    }

    /// <inheritdoc/>
    public override Vector3 GetClosestInteractionPoint(Vector3 point)
    {
        var dir = (point - transform.position).normalized;
        var distance = (point - transform.position).magnitude;
        return distance > interactionRangeDistance ? transform.position + dir * interactionRangeDistance : point;
    }

    /// <inheritdoc/>
    public override bool TryInteract(Character character)
    {
        if (IsEffectActive)
        {
            IsEffectActive = false;
            character.KneelBeforeStatue(transform.position);
            StartCoroutine(MoveOrbToCharacter(character));
            return true;
        }
        return false;
    }

    private IEnumerator MoveOrbToCharacter(Character character)
    {
        yield return new WaitForSeconds(animationDelay);
        while (((character.transform.position + targetHeightDelta * Vector3.up) - orbTransform.position).magnitude > positionDelta)
        {
            var dir = ((character.transform.position + targetHeightDelta * Vector3.up) - orbTransform.position).normalized;
            orbTransform.position += dir * orbMovingSpeed * Time.deltaTime;
            var orbSize = orbVFX.GetFloat("Size");
            if (orbSize < vfxTargetSize)
            {
                orbVFX.SetFloat("Size", orbSize + orbScalingSpeed * Time.deltaTime);
            }
            if (orbLight.intensity > 0)
            {
                orbLight.intensity -= orbIntensityChangeSpeed * Time.deltaTime;
            }
            yield return null;
        }
        orbTransform.parent = character.transform;
    }

    #endregion
}
