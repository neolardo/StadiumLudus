using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Material healthBarMaterial;
    public Material staminaBarMaterial;
    public List<RawImage> skillBackgroundRawImages;
    public List<Image> skillCooldownImages;
    public List<RawImage> skillIconRawImages;
    public List<Texture2D> warriorSkillTextures;
    public List<Texture2D> rangerSkillTextures;


    private const string valueShaderPropertyReference = "Vector1_0ae9bdea3f184704b6c11dd6513db5a4";


    private void Start()
    {
        InitializeSkillIcons();
    }

    private void InitializeSkillIcons()
    {
        List<Texture2D> textures = null;
        if (character is MaleWarriorCharacter || character is FemaleWarriorCharacter)
        {
            textures = warriorSkillTextures;
        }
        else
        {
            textures = rangerSkillTextures;
        }
        for (int i = 0; i < skillIconRawImages.Count; i++)
        {
            skillIconRawImages[i].texture = textures[i];
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

    public void ChangeSkillButtonPress(int skillZeroBasedIndex, bool isPressed)
    {
        var color = skillBackgroundRawImages[skillZeroBasedIndex].color;
        skillBackgroundRawImages[skillZeroBasedIndex].color = new Color(color.r, color.g, color.b, isPressed ? 1 : 0);
    }

    public void StartSkillCooldown(int skillZeroBasedIndex, float cooldownSeconds)
    {
        StartCoroutine(AnimateRadialFillCooldown(skillZeroBasedIndex, cooldownSeconds));
    }

    private IEnumerator AnimateRadialFillCooldown(int skillZeroBasedIndex, float cooldownSeconds)
    {
        float elapsedTime = 0;
        while (elapsedTime < cooldownSeconds)
        {
            skillCooldownImages[skillZeroBasedIndex].fillAmount = Mathf.Lerp(1, 0, elapsedTime / cooldownSeconds);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        skillCooldownImages[skillZeroBasedIndex].fillAmount = 0;
    }
}
