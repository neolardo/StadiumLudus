using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    private List<TextMeshProUGUI> playerNames;
    public GameObject confirmButton;
    public TextMeshProUGUI confirmedText;
    public GameObject loadingPopUp;

    private const string femaleBarbarianDescription = "Female barbarians militate quickly wielding a pair of axes and are able to perform combo attacks with them.";
    private const string maleBarbarianDescription = "Male barbarians strike slow but strong with their large battle-axe.";
    private const string femaleRangerDescription = "Female rangers use a shortbow to quickly damage their enemies from the distance.";
    private const string maleRangerDescription = "Male rangers fight with a heavy crossbow to deal decent damage but the additional reloading time makes them somewhat delayed.";

    #endregion

    #region Methods

    private void Start()
    {
        playerNames = new List<TextMeshProUGUI>();
        NetworkLauncher.Instance.PlayerEnteredRoom += OnPlayerEnteredRoom;
        NetworkLauncher.Instance.PlayerLeftRoom += OnPlayerLeftRoom;
        NetworkLauncher.Instance.StartingGame += OnGameStarting;
        RefreshCharacterGameObject();
        LoadPlayerNames();
    }

    #region Add, Remove players

    private void LoadPlayerNames()
    {
        var currentPlayers = PhotonNetwork.PlayerList;
        foreach (var p in currentPlayers)
        {
            AddPlayerName(p);
        }
    }
    private void AddPlayerName(Player player)
    {
        var playerNameText = Instantiate(playerNamePrefab, playerNameContainer);
        playerNameText.text = player.NickName;
        playerNames.Add(playerNameText);
    }
    private void RemovePlayerName(Player player)
    {
        var playerNameText = playerNames.FirstOrDefault(_ => _.text == player.NickName);
        if (playerNameText != null)
        {
            playerNames.Remove(playerNameText);
            Destroy(playerNameText.gameObject);
        }
    }

    private void OnPlayerEnteredRoom(Player player)
    {
        AddPlayerName(player);
    }
    private void OnPlayerLeftRoom(Player player)
    {
        RemovePlayerName(player);
    }

    #endregion

    #region Change Character

    public void OnFigthingStyleClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonClickAlt);
        currentFightingStyle = currentFightingStyle == CharacterFightingStyle.Light ? CharacterFightingStyle.Heavy : CharacterFightingStyle.Light;
        fightingStyleValueText.text = currentFightingStyle.ToString();
        RefreshCharacterGameObject();
    }

    public void OnClassClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonClickAlt);
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

    #endregion

    #region Starting Game

    private void OnGameStarting()
    {
        confirmedText.text = "Loading game...";
    }

    #endregion

    #region Handle Clicks

    public void OnButtonClicked()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonClick);
    }

    public void OnButtonHovered()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonHover);
    }

    public void OnLeaveClicked()
    {
        loadingPopUp.SetActive(true);
        StartCoroutine(LeaveRoomAndLoadMainSceneAfterDelay());
    }

    private IEnumerator LeaveRoomAndLoadMainSceneAfterDelay()
    {
        yield return new WaitForSeconds(Globals.LoadingDelay);
        PhotonNetwork.LeaveRoom();
    }

    public void OnConfirmClicked()
    {
        confirmButton.SetActive(false);
        confirmedText.gameObject.SetActive(true);
        NetworkLauncher.Instance.OnCharacterConfirmed(PhotonNetwork.LocalPlayer, currentFightingStyle, currentClass);
    }

    #endregion

    #region Destroy

    private void OnDestroy()
    {
        if (NetworkLauncher.Instance != null)
        {
            NetworkLauncher.Instance.PlayerEnteredRoom -= OnPlayerEnteredRoom;
            NetworkLauncher.Instance.PlayerLeftRoom -= OnPlayerLeftRoom;
        }
    }

    #endregion

    #endregion
}