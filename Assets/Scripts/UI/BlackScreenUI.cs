using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the black UI screen which shows up when loading up the game scene.
/// </summary>
public class BlackScreenUI : MonoBehaviour
{
    [SerializeField] private RawImage rawImageBackground;
    [SerializeField] private float fadeOutSeconds;

    public void Awake()
    {
        gameObject.SetActive(true);
    }

    public void FadeOutAndDisable()
    {
        StartCoroutine(AnimateFadeOut());
    }

    private IEnumerator AnimateFadeOut()
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeOutSeconds)
        {
            rawImageBackground.color = new Color(rawImageBackground.color.r, rawImageBackground.color.g, rawImageBackground.color.b, Mathf.Lerp(1f, 0f, elapsedTime / fadeOutSeconds)); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}