using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game UI of a character.
/// </summary>
public class CharacterUI : MonoBehaviour
{
    public PauseMenuUI pauseMenuUI;
    public EndGameUI endGameUI;
    public List<SkillSlotUI> skillSlots;
    public ValueBarUI healthBarUI;
    public ValueBarUI staminaBarUI;
    public CanvasGroup canvasGroup;
    public AudioSource audioSource;
    private Character character;
    public bool IsUIVisible { get; private set; } = false;

    private const float endScreenDelay = 0.5f;

    private bool hasInitialized = false;

    #region Methods

    public void Initialize(Character character)
    {
        this.character = character;
        if (character.Class == CharacterClass.Barbarian)
        {
            for (int i = 0; i < skillSlots.Count; i++)
            {
                skillSlots[i].InitializeAsWarrior(character.IsSkillChargeable(i + 1), character.InitialChargeCountOfSkill(i + 1));
            }
        }
        else
        {
            for (int i = 0; i < skillSlots.Count; i++)
            {
                skillSlots[i].InitializeAsRanger(character.IsSkillChargeable(i + 1), character.InitialChargeCountOfSkill(i + 1));
            }
        }
        hasInitialized = true;
    }

    private void Update()
    {
        if (hasInitialized)
        {
            UpdateShaders();
        }
    }

    private void UpdateShaders()
    {
        healthBarUI.UpdateValue(character.HealthRatio);
        staminaBarUI.UpdateValue(character.StaminaRatio);
    }
    public void SetUIVisiblity(bool value)
    {
        IsUIVisible = value;
        canvasGroup.alpha = IsUIVisible ? 1 : 0;
    }

    #region Skills

    public void ChangeSkillButtonPress(int skillNumber, bool isPressed)
    {
        skillSlots[skillNumber - 1].ChangeSkillButtonPress(isPressed);
    }

    public void StartSkillCooldown(int skillNumber, float cooldownSeconds)
    {
        skillSlots[skillNumber - 1].StartSkillCooldown(cooldownSeconds);
    }

    public void AddSkillCharge(int skillNumber)
    {
        skillSlots[skillNumber - 1].AddCharge();
    }

    public void RemoveSkillCharge(int skillNumber)
    {
        skillSlots[skillNumber - 1].RemoveCharge();
    }

    public void OnCannotPerformSkillOrAttack(bool notEnoughStamina, bool stillOnCooldown, int skillNumber = -1)
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.CannotPerformSkillOrAttack);
        if (notEnoughStamina)
        {
            staminaBarUI.ShowHideHighlight();
        }
        if (stillOnCooldown)
        {
            skillSlots[skillNumber - 1].ShowHideHighlight();
        }
    }

    #endregion

    #region Pause Menu

    public void ShowHidePauseMenu()
    {
        IsUIVisible = !IsUIVisible;
        canvasGroup.alpha = IsUIVisible ? 1 : 0;
        pauseMenuUI.gameObject.SetActive(!IsUIVisible);
    }

    #endregion

    #region End Screen

    public void ShowEndScreen(bool win)
    {
        StartCoroutine(ShowEndScreenAfterDelay(win, endScreenDelay));
    }

    private IEnumerator ShowEndScreenAfterDelay(bool win, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        IsUIVisible = false;
        canvasGroup.alpha = 0;
        endGameUI.SetMainText(win);
        endGameUI.gameObject.SetActive(true);
    }

    #endregion

    #endregion

}
