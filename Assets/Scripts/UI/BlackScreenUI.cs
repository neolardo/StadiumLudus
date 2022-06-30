using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the black UI screen which shows up when loading up the game scene.
/// </summary>
public class BlackScreenUI : MonoBehaviour
{
    [SerializeField] private RawImage rawImageBackground;
    [SerializeField] private float fadeSeconds;

    #region Fade In

    public void EnableAndFadeIn()
    {
        gameObject.SetActive(true);
        StartCoroutine(AnimateFadeIn());
    }
    private IEnumerator AnimateFadeIn()
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeSeconds)
        {
            rawImageBackground.color = new Color(rawImageBackground.color.r, rawImageBackground.color.g, rawImageBackground.color.b, Mathf.Lerp(0,1, elapsedTime / fadeSeconds));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region Fade Out

    public void FadeOutAndDisable()
    {
        StartCoroutine(AnimateFadeOut());
    }
    private IEnumerator AnimateFadeOut()
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeSeconds)
        {
            rawImageBackground.color = new Color(rawImageBackground.color.r, rawImageBackground.color.g, rawImageBackground.color.b, Mathf.Lerp(1f, 0f, elapsedTime / fadeSeconds)); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    #endregion
}
