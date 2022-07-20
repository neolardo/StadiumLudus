using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages an in-game info text.
/// </summary>
public class InfoTextUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private TextMeshProUGUI text;
    private Color initialColor;

    private const float fadeInDuration = .2f;
    private const float showDuration = 4f;
    private const float fadeOutDuration = .8f;

    #endregion

    #region Methods

    private void Awake()
    {
        initialColor = text.color;
        text.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
    }

    public void SetInfoText(string text)
    {
        this.text.text = text;
    }

    private void OnEnable()
    {
        StartCoroutine(FadeInAndOut());
    }

    private IEnumerator FadeInAndOut()
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            text.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(0, initialColor.a, elapsedTime / fadeInDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        text.color = initialColor;
        yield return new WaitForSeconds(showDuration);
        elapsedTime = 0;
        while (elapsedTime < fadeOutDuration)
        {
            text.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0, elapsedTime / fadeOutDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    #endregion
}
