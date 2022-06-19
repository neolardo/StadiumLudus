using TMPro;
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

    public AudioSource audioSource;
    public GameObject femaleWarriorGameObject;
    public GameObject maleWarriorGameObject;
    public GameObject femaleRangerGameObject;
    public GameObject maleRangerGameObject;
    public TextMeshProUGUI playerNamePrefab;
    public Transform playerNameContainer;
    public TextMeshProUGUI fightingStyleValueText;
    public TextMeshProUGUI classValueText;
    public TextMeshProUGUI descriptionText;

    private const string femaleBarbarianDescription = "Female barbarians use two battleaxes and are able to perform agile combo attacks with them.";
    private const string maleBarbarianDescription = "Male barbarians wield a two-handed battleaxe to deal decent damage at a mediocre speed.";
    private const string femaleRangerDescription = "Female rangers wield a shortbow which is easy to use in ranged battle but deals mediocre damage.";
    private const string maleRangerDescription = "Male rangers wield a crossbow which deals much more damage than shortbows, but requires additional reloading to use.";

    #endregion

    #region Methods

    private void Start()
    {
        RefreshCharacterGameObject();
        LoadPlayerNames();
    }

    private void LoadPlayerNames()
    {
        // TODO
        var player = Instantiate(playerNamePrefab, playerNameContainer);
        player.text = "Lajos leves";
    }

    public void OnFigthingStyleClicked()
    {
        currentFightingStyle = currentFightingStyle == CharacterFightingStyle.Light ? CharacterFightingStyle.Heavy : CharacterFightingStyle.Light;
        fightingStyleValueText.text = currentFightingStyle.ToString();
        RefreshCharacterGameObject();
    }

    public void OnClassClicked()
    {
        currentClass = currentClass == CharacterClass.Ranger ? CharacterClass.Barbarian : CharacterClass.Ranger;
        classValueText.text = currentClass.ToString();
        RefreshCharacterGameObject();
    }

    private void RefreshCharacterGameObject()
    {
        femaleWarriorGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Light && currentClass == CharacterClass.Barbarian);
        maleWarriorGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Heavy && currentClass == CharacterClass.Barbarian);
        femaleRangerGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Light && currentClass == CharacterClass.Ranger);
        maleRangerGameObject.SetActive(currentFightingStyle == CharacterFightingStyle.Heavy && currentClass == CharacterClass.Ranger);
        if (currentFightingStyle == CharacterFightingStyle.Heavy && currentClass == CharacterClass.Ranger)
        {
            descriptionText.text = maleRangerDescription;
        }
        else if (currentFightingStyle == CharacterFightingStyle.Heavy && currentClass == CharacterClass.Barbarian)
        {
            descriptionText.text = maleBarbarianDescription;
        }
        else if (currentFightingStyle == CharacterFightingStyle.Light && currentClass == CharacterClass.Ranger)
        {
            descriptionText.text = femaleRangerDescription;
        }
        else if (currentFightingStyle == CharacterFightingStyle.Light && currentClass == CharacterClass.Barbarian)
        {
            descriptionText.text = femaleBarbarianDescription;
        }
    }

    public void OnButtonClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuClick);
    }
    public void OnButtonHovered()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonHover);
    }

    #endregion
}