using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a valuebar UI element.
/// </summary>
public class ValueBarUI : MonoBehaviour
{
    public Material valueBarMaterial;
    public Material highlightSourceMaterial;
    public RawImage highlightRawImage;
    private Material highlightMaterial;

    private const string valueShaderPropertyReference = "Vector1_0ae9bdea3f184704b6c11dd6513db5a4";
    private const float highlightFadeInDuration = 0.1f;
    private const float highlightFadeOutDuration = 0.3f;
    private const float highlightShowDuration = .2f;
    private const string highlightAplhaPropertyName = "_Alpha";
    private bool IsHighlightBeingAnimated { get; set; }
    private bool RequestHighlightRefresh { get; set; }

    #region Methods
    void Start()
    {
        highlightMaterial = Instantiate(highlightSourceMaterial);
        highlightRawImage.material = highlightMaterial;
    }

    public void UpdateValue(float value)
    {
        valueBarMaterial.SetFloat(valueShaderPropertyReference, value);
    }

    #region Highlight

    public void ShowHideHighlight()
    {
        StartCoroutine(FadeInAndOutHighlight());
    }

    private IEnumerator FadeInAndOutHighlight()
    {
        if (IsHighlightBeingAnimated)
        {
            RequestHighlightRefresh = true;
        }
        yield return new WaitUntil(() => !IsHighlightBeingAnimated);
        IsHighlightBeingAnimated = true;
        float alpha = highlightMaterial.GetFloat(highlightAplhaPropertyName);
        while (alpha < 1 && !RequestHighlightRefresh)
        {
            alpha += Time.deltaTime / highlightFadeInDuration;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
            yield return null;
        }
        if (!RequestHighlightRefresh)
        {
            alpha = 1;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
        }
        float elapsedSeconds = 0;
        while (elapsedSeconds < highlightShowDuration && !RequestHighlightRefresh)
        {
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        while (alpha > 0 && !RequestHighlightRefresh)
        {
            alpha -= Time.deltaTime / highlightFadeOutDuration;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
            yield return null;
        }
        if (!RequestHighlightRefresh)
        {
            alpha = 0;
            highlightMaterial.SetFloat(highlightAplhaPropertyName, alpha);
        }
        RequestHighlightRefresh = false;
        IsHighlightBeingAnimated = false;
    }


    #endregion

    #endregion

}
