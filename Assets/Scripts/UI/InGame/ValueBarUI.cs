using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a value bar UI element.
/// </summary>
public class ValueBarUI : MonoBehaviour
{
    #region Properties and Fields

    public Material valueBarMaterial;
    public Material highlightSourceMaterial;
    public RawImage highlightRawImage;
    private Material highlightMaterial;
    private bool isHighlightBeingAnimated;
    private bool requestHighlightRefresh;

    private const string valueShaderPropertyReference = "Vector1_0ae9bdea3f184704b6c11dd6513db5a4";
    private const float highlightFadeInDuration = 0.1f;
    private const float highlightFadeOutDuration = 0.3f;
    private const float highlightShowDuration = .2f;
    private const string highlightAplhaPropertyName = "_Alpha";

    #endregion

    #region Methods

    void Start()
    {
        highlightMaterial = Instantiate(highlightSourceMaterial);
        highlightRawImage.material = highlightMaterial;
    }

    #region Update Value

    public void UpdateValue(float value)
    {
        valueBarMaterial.SetFloat(valueShaderPropertyReference, value);
    }

    #endregion

    #region Highlight

    public void ShowHideHighlight()
    {
        StartCoroutine(FadeInAndOutHighlight());
    }

    private IEnumerator FadeInAndOutHighlight()
    {
        if (isHighlightBeingAnimated)
        {
            requestHighlightRefresh = true;
        }
        yield return new WaitUntil(() => !isHighlightBeingAnimated);
        isHighlightBeingAnimated = true;
        float alpha = highlightMaterial.GetFloat(highlightAplhaPropertyName);
        while (alpha < 1 && !requestHighlightRefresh)
        {
            alpha += Time.deltaTime / highlightFadeInDuration;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
            yield return null;
        }
        if (!requestHighlightRefresh)
        {
            alpha = 1;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
        }
        float elapsedSeconds = 0;
        while (elapsedSeconds < highlightShowDuration && !requestHighlightRefresh)
        {
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        while (alpha > 0 && !requestHighlightRefresh)
        {
            alpha -= Time.deltaTime / highlightFadeOutDuration;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
            yield return null;
        }
        if (!requestHighlightRefresh)
        {
            alpha = 0;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
        }
        requestHighlightRefresh = false;
        isHighlightBeingAnimated = false;
    }

    #endregion

    #endregion

}
