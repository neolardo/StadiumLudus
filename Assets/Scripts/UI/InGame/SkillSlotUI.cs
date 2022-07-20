using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI of a skill slot.
/// </summary>
public class SkillSlotUI : MonoBehaviour
{
    #region Properties and Fields

    [SerializeField] private RawImage pressedBackgroundRawImage;
    [SerializeField] private RawImage iconRawImage;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Texture2D warriorIconTexture2D;
    [SerializeField] private Texture2D rangerIconTexture2D;
    [SerializeField] private TextMeshProUGUI chargesText;
    [SerializeField] private RawImage highlightRawImage;
    [SerializeField] private Material sourceMaterial;
    private Material highlightMaterial;
    private int currentChargeCount;
    private bool isHighlightBeingAnimated;
    private bool requestHighlightRefresh;

    private const float highlightFadeInDuration = 0.1f;
    private const float highlightFadeOutDuration = 0.3f;
    private const float highlightShowDuration = .2f;
    private const string highlightAplhaPropertyName = "_Alpha";

    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
        highlightMaterial = Instantiate(sourceMaterial);
        highlightRawImage.material = highlightMaterial;
    }

    public void InitializeAsWarrior(bool isChargesVisible = false, int charges = 0)
    {
        iconRawImage.texture = warriorIconTexture2D;
        InitializeCharges(isChargesVisible, charges);
    }

    public void InitializeAsRanger(bool isChargesVisible = false, int charges = 0)
    {
        iconRawImage.texture = rangerIconTexture2D;
        InitializeCharges(isChargesVisible, charges);
    }

    private void InitializeCharges(bool isChargesVisible, int initialCharges)
    {
        currentChargeCount = initialCharges;
        chargesText.gameObject.SetActive(isChargesVisible);
        chargesText.text = initialCharges.ToString();
    }

    #endregion

    #region Cooldown

    public void ChangeSkillButtonPress(bool isPressed)
    {
        var color = pressedBackgroundRawImage.color;
        pressedBackgroundRawImage.color = new Color(color.r, color.g, color.b, isPressed ? 1 : 0);
    }

    public void StartSkillCooldown(float cooldownSeconds)
    {
        StartCoroutine(AnimateRadialFillCooldown(cooldownSeconds));
    }

    private IEnumerator AnimateRadialFillCooldown(float cooldownSeconds)
    {
        float elapsedTime = 0;
        while (elapsedTime < cooldownSeconds)
        {
            cooldownImage.fillAmount = Mathf.Lerp(1, 0, elapsedTime / cooldownSeconds);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cooldownImage.fillAmount = 0;
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

    #region Charges

    public void RemoveCharge()
    {
        currentChargeCount -= 1;
        chargesText.text = currentChargeCount.ToString();
    }

    public void AddCharge()
    {
        currentChargeCount += 1;
        chargesText.text = currentChargeCount.ToString();
    }

    #endregion

    #endregion
}
