using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public PlayerNameUI playerNamePrefab;
    public Transform playerNameContainer;
    public TextMeshProUGUI fightingStyleValueText;
    public TextMeshProUGUI classValueText;
    public TextMeshProUGUI descriptionText;
    private List<PlayerNameUI> playerNames;
    public GameObject confirmButton;
    public TextMeshProUGUI confirmedText;
    public GameObject loadingPopUp;
    public List<Button> arrowButtons;
    public GameObject playersText;
    public GameObject practiceModeText;
    public GameObject otherPlayersCannotJoinText;

    private const string femaleBarbarianDescription = "Female barbarians militate quickly wielding a pair of axes and are able to perform combo attacks with them.";
    private const string maleBarbarianDescription = "Male barbarians strike slow but strong with their large battle-axe.";
    private const string femaleRangerDescription = "Female rangers use a shortbow to quickly damage their enemies from the distance.";
    private const string maleRangerDescription = "Male rangers fight with a heavy crossbow to deal decent damage but the additional reloading time makes them somewhat sluggish.";

    #endregion

    #region Methods

    private void Start()
    {
        playerNames = new List<PlayerNameUI>();
        NetworkLauncher.Instance.PlayerEnteredRoom += OnPlayerEnteredRoom;
        NetworkLauncher.Instance.PlayerLeftRoom += OnPlayerLeftRoom;
        NetworkLauncher.Instance.PlayerPropertiesChanged += OnPlayerCharacterIsConfirmedChanged;
        NetworkLauncher.Instance.StartingGame += OnGameStarting;
        if (NetworkLauncher.Instance.IsPracticeMode)
        {
            InitializeAsPracticeMode();
        }
        RefreshCharacterGameObject();
        LoadPlayerNames();
    }

    #region Add, Remove players

    private void InitializeAsPracticeMode()
    {
        playersText.SetActive(false);
        practiceModeText.SetActive(true);
        otherPlayersCannotJoinText.SetActive(true);
        confirmedText.text = "Loading game...";
    }

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
        var playerName = Instantiate(playerNamePrefab, playerNameContainer);
        playerName.SetPlayerText(player.NickName);
        playerName.SetIsCharacterConfirmed(player.CustomProperties.ContainsKey(Globals.PlayerIsCharacterConfirmedKey) && (bool)player.CustomProperties[Globals.PlayerIsCharacterConfirmedKey]);
        playerNames.Add(playerName);
    }
    private void RemovePlayerName(Player player)
    {
        var playerName = playerNames.FirstOrDefault(_ => _.PlayerName == player.NickName);
        if (playerName != null)
        {
            playerNames.Remove(playerName);
            Destroy(playerName.gameObject);
        }
    }
    private void OnPlayerCharacterIsConfirmedChanged(Player player)
    {
        var playerName = playerNames.FirstOrDefault(_ => _.PlayerName == player.NickName);
        if (playerName != null)
        {
            playerName.SetIsCharacterConfirmed(player.CustomProperties.ContainsKey(Globals.PlayerIsCharacterConfirmedKey) && (bool)player.CustomProperties[Globals.PlayerIsCharacterConfirmedKey]);
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
        Color disabledColor = arrowButtons[0].colors.disabledColor;
        foreach (var b in arrowButtons)
        {
            b.interactable = false;
        }
        classValueText.color = disabledColor;
        fightingStyleValueText.color = disabledColor;
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
            NetworkLauncher.Instance.PlayerPropertiesChanged -= OnPlayerCharacterIsConfirmedChanged;
            NetworkLauncher.Instance.StartingGame -= OnGameStarting;
        }
    }

    #endregion

    #endregion
}