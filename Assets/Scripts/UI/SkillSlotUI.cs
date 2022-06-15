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

    public RawImage pressedBackgroundRawImage;
    public RawImage iconRawImage;
    public Image cooldownImage;
    public Texture2D warriorIconTexture2D;
    public Texture2D rangerIconTexture2D;
    public TextMeshProUGUI chargesText;

    private int currentCharges;
    private bool isChargesVisible;

    #endregion

    #region Methods

    #region Initialize

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
        currentCharges = initialCharges;
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

    #region Charges

    public void RemoveCharge()
    {
        currentCharges -= 1;
        chargesText.text = currentCharges.ToString();
    }

    public void AddCharge()
    {
        currentCharges += 1;
        chargesText.text = currentCharges.ToString();
    }

    #endregion

    #endregion
}
