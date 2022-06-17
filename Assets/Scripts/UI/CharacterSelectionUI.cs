using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI of the character selection scene.
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    #region Fields and Properties

    private CharacterFightingStyle currentFightingStyle = CharacterFightingStyle.Heavy;
    private CharacterClass currentClass = CharacterClass.Barbarian;

    public GameObject femaleWarriorGameObject;
    public GameObject maleWarriorGameObject;
    public GameObject femaleRangerGameObject;
    public GameObject maleRangerGameObject;

    public Button lightButton;
    public Button heavyButton;
    public Button barbarianButton;
    public Button rangerButton;
    public Color buttonNormalColor;
    public Color buttonSelectedColor;


    #endregion

    #region Methods

    private void Start()
    {
        RefreshCharacterGameObject();
    }

    public void OnLightFightingStyleSelected()
    {
        currentFightingStyle = CharacterFightingStyle.Light;
        RefreshCharacterGameObject();
    }

    public void OnHeavyFightingStyleSelected()
    {
        currentFightingStyle = CharacterFightingStyle.Heavy;
        RefreshCharacterGameObject();
    }

    public void OnBarbarianClassSelected()
    {
        currentClass = CharacterClass.Barbarian;
        RefreshCharacterGameObject();
    }

    public void OnRangerClassSelected()
    {
        currentClass = CharacterClass.Ranger;
        RefreshCharacterGameObject();
    }

    private void RefreshCharacterGameObject()
    {
        femaleWarriorGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Light && currentClass == CharacterClass.Barbarian);
        maleWarriorGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Heavy && currentClass == CharacterClass.Barbarian);
        femaleRangerGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Light && currentClass == CharacterClass.Ranger);
        maleRangerGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Heavy && currentClass == CharacterClass.Ranger);
        var colors = lightButton.colors;
        colors.normalColor = currentFightingStyle == CharacterFightingStyle.Light ? buttonSelectedColor : buttonNormalColor;
        lightButton.colors = colors;
        colors = heavyButton.colors;
        colors.normalColor = currentFightingStyle == CharacterFightingStyle.Heavy ? buttonSelectedColor : buttonNormalColor;
        heavyButton.colors = colors;
        colors = barbarianButton.colors;
        colors.normalColor = currentClass == CharacterClass.Barbarian ? buttonSelectedColor : buttonNormalColor;
        barbarianButton.colors = colors;
        colors = rangerButton.colors;
        colors.normalColor = currentClass == CharacterClass.Ranger ? buttonSelectedColor : buttonNormalColor;
        rangerButton.colors = colors;
    }

    #endregion
}
