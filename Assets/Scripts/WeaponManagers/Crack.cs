using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a crack of a warrior's ground slam animation.
/// </summary>
public class Crack : MonoBehaviour
{
    #region Properties and Fields

    public float length;
    public int blendShapeCount;
    public List<Transform> cornerPoints;
    [SerializeField] private SkinnedMeshRenderer crackRenderer;
    [SerializeField] private SkinnedMeshRenderer crackMaskRenderer;

    #endregion

    #region Methods

    public float GetBlendShape(int index)
    {
        return (100 - crackRenderer.GetBlendShapeWeight(index)) / 100f;
    }

    public void SetBlendShape(int index, float value)
    {
        crackRenderer.SetBlendShapeWeight(index, 100- value * 100);
        crackMaskRenderer.SetBlendShapeWeight(index, 100- value * 100);
    }

    #endregion
}
