using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages an interactable fountain.
/// </summary>
public class Fountain :  Interactable
{
    #region Properties and Fields

    [Tooltip("The container of the interaction points.")]
    [SerializeField]
    private GameObject interactionPointContainer;

    private List<Vector3> interactionPoints;

    private bool _isFilled;

    /// <summary>
    /// Inidicates whether this fountain is filled with water, and is ready to be interacted with or not.
    /// </summary>
    private bool IsFilled
    {
        get
        {
            return _isFilled;
        }
        set 
        {
            if (value != _isFilled)
            {
                _isFilled = value;
                if (!_isFilled)
                {
                    StartCoroutine(WaitUntilRefill());
                    StartCoroutine(AnimateEmpty());
                }
                else
                {
                    StartCoroutine(AnimateFill());
                }
            }
        }
    }

    [Tooltip("Represents how many seconds does it take to refill the fountain.")]
    [SerializeField]
    private float refillTime = 30f;

    [SerializeField]
    private GameObject smallWater;

    [SerializeField]
    private GameObject largeWater;

    [SerializeField]
    private Material smallWaterMaterial;

    private const string waterShaderAlphaReference = "Vector1_ebe175a0fc8a44548e7e79ef349e1724";
    private const float waterAnimationDelay = .9f;
    private const float waterAnimationSpeed = 1;
    private const float waterSizeFilled = .8f;
    private const float waterSizeEmpty = .05f;
    private const float waterAlphaFilled = .3f;


    #endregion


    void Start()
    {
        interactionPoints = new List<Vector3>();
        IsFilled = true;
        for (int i = 0; i < interactionPointContainer.transform.childCount; i++)
        {
            interactionPoints.Add(interactionPointContainer.transform.GetChild(i).position);
        }
    }

    /// <inheritdoc/>
    public override Vector3 GetClosestInteractionPoint(Vector3 point)
    {
        float dist = (interactionPoints[0] - point).magnitude;
        Vector3 closestPoint = interactionPoints[0];
        for (int i = 1; i < interactionPoints.Count; i++)
        {
            var newDist = (interactionPoints[i] - point).magnitude;
            if (newDist < dist)
            {
                dist = newDist;
                closestPoint = interactionPoints[i];
            }
        }
        return closestPoint;
    }

    /// <inheritdoc/>
    public override bool TryInteract(Character character)
    {
        if (IsFilled)
        {
            IsFilled = false;
            character.DrinkFromFountain(transform.position);
            return true;
        }
        return false;
    }

    private IEnumerator WaitUntilRefill()
    {
        yield return new WaitForSeconds(refillTime);
        IsFilled = true;
    }

    private IEnumerator AnimateEmpty()
    {
        yield return new WaitForSeconds(waterAnimationDelay);
        var scale = largeWater.transform.localScale;
        var scaleZ = largeWater.transform.localScale.z;
        while (scaleZ > waterSizeEmpty && !IsFilled)
        {
            scaleZ -= waterAnimationSpeed * Time.deltaTime;
            largeWater.transform.localScale = new Vector3(scale.x, scale.y, scaleZ);
            smallWaterMaterial.SetFloat(waterShaderAlphaReference, Mathf.Lerp(waterAlphaFilled, 0, (Mathf.InverseLerp(waterSizeFilled, waterSizeEmpty, scaleZ))));
            yield return null;
        }
        if (!IsFilled)
        {
            largeWater.transform.localScale = new Vector3(scale.x, scale.y, waterSizeEmpty);
        }
    }

    private IEnumerator AnimateFill()
    {
        var scale = largeWater.transform.localScale;
        var scaleZ = largeWater.transform.localScale.z;
        while (scaleZ < waterSizeFilled && IsFilled)
        {
            scaleZ += waterAnimationSpeed * Time.deltaTime;
            largeWater.transform.localScale = new Vector3(scale.x, scale.y, scaleZ);
            smallWaterMaterial.SetFloat(waterShaderAlphaReference, Mathf.Lerp(0, waterAlphaFilled, (Mathf.InverseLerp(waterSizeEmpty, waterSizeFilled, scaleZ))));
            yield return null;
        }
        if (IsFilled)
        {
            largeWater.transform.localScale = new Vector3(scale.x, scale.y, waterSizeFilled);
        }
    }
}