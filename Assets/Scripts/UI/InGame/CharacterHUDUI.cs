using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the in-game HUD.
/// </summary>
public class CharacterHUDUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private InGameUIManager uiManager;
    [SerializeField] private AudioSource cannotPerformSkillOrAttackAudioSource;
    [SerializeField] private List<SkillSlotUI> skillSlots;
    [SerializeField] private ValueBarUI healthBarUI;
    [SerializeField] private ValueBarUI staminaBarUI;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform infoContainer;
    [SerializeField] private InfoTextUI infoPrefab;
    private Character character;

    public bool IsVisible { get; private set; } = true;
    private bool hasInitialized = false;

    #endregion

    #region Methods

    #region Initialize

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

    #endregion

    #region Update Valuebar Shaders

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

    #endregion

    #region Show Hide

    public void Show()
    {
        IsVisible = true;
        canvasGroup.alpha = IsVisible ? 1 : 0;
    }

    public void Hide()
    {
        IsVisible = false;
        canvasGroup.alpha = IsVisible ? 1 : 0;
    }

    #endregion

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

    public void OnCannotPerformSkillOrAttack(bool notEnoughStamina, bool stillOnCooldown = false, int skillNumber = -1)
    {
        if (!cannotPerformSkillOrAttackAudioSource.isPlaying)
        {
            AudioManager.Instance.PlayOneShotSFX(cannotPerformSkillOrAttackAudioSource, SFX.CannotPerformSkillOrAttack);
        }
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

    #region Info

    public void AddInfo(string info)
    {
        var text = Instantiate(infoPrefab, infoContainer);
        text.SetInfoText(info);
        text.gameObject.SetActive(true);
    }

    #endregion

    #endregion
}
