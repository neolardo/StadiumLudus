using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the in-game UI of a character.
/// </summary>
public class CharacterUI : MonoBehaviour
{
    /// <summary>
    /// The character depends on the corresponding <see cref="CharacterController"/> so it's set by that.
    /// </summary>
    [HideInInspector]
    public Character character;
    public PauseMenuUI pauseMenuUI;
    public EndGameUI endGameUI;
    public Material healthBarMaterial;
    public Material staminaBarMaterial;
    public List<SkillSlotUI> skillSlots;
    public bool isUIVisible { get; private set; } = true;

    private const float endScreenDelay = 0.5f;

    private const string valueShaderPropertyReference = "Vector1_0ae9bdea3f184704b6c11dd6513db5a4";

    #region Methods

    private void Start()
    {
        InitializeSkillIcons();
    }

    private void InitializeSkillIcons()
    {
        if (character is MaleWarriorCharacter || character is FemaleWarriorCharacter)
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
    }

    private void Update()
    {
        UpdateShaders();
    }

    private void UpdateShaders()
    {
        healthBarMaterial.SetFloat(valueShaderPropertyReference, character.HealthRatio);
        staminaBarMaterial.SetFloat(valueShaderPropertyReference, character.StaminaRatio);
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

    #endregion

    #region Pause Menu

    public void ShowHidePauseMenu()
    {
        isUIVisible = !isUIVisible;
        gameObject.SetActive(isUIVisible);
        pauseMenuUI.gameObject.SetActive(!isUIVisible);
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
        isUIVisible = !isUIVisible;
        gameObject.SetActive(isUIVisible);
        endGameUI.SetMainText(win);
        endGameUI.gameObject.SetActive(!isUIVisible);
    }

    #endregion

    #endregion

}
