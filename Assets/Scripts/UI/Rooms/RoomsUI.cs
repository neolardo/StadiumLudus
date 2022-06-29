using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the rooms UI screen.
/// </summary>
public class RoomsUI : MonoBehaviour
{
    #region Properties and Fields

    public MainMenuUI mainMenuUI;
    public RectTransform roomButtonContainer;
    public RoomButton roomButtonPrefab;
    public GameObject createRoomPopUp;
    public JoinRoomPopUpUI joinRoomPopUpUI;
    public Button joinRoomButton;
    public GameObject noRoomsText;
    public GameObject loadingPopUp;
    public TextMeshProUGUI infoText;
    public CanvasGroup infoCanvasGroup;

    private List<RoomInfo> localRooms;
    private List<RoomButton> roomButtons;

    private RoomInfo _selectedRoom;
    private RoomInfo SelectedRoom 
    {
        get 
        {
            return _selectedRoom;
        }
        set
        {
            if (_selectedRoom != null)
            {
                var rb = roomButtons.FirstOrDefault(_ => _.RoomInfo == _selectedRoom);
                if (rb != null)
                {
                    rb.ChangeSelectionColor(false);
                }
            }
            _selectedRoom = value;
            if (_selectedRoom != null)
            {
                var rb = roomButtons.FirstOrDefault(_ => _.RoomInfo == _selectedRoom);
                if (rb != null)
                {
                    rb.ChangeSelectionColor(true);
                }
            }
            joinRoomButton.interactable = _selectedRoom != null;
        }
    }

    private const float infoFadeDuration = 0.5f;
    private const float infoShowDuration = 2.2f;
    private bool IsInfoBeingAnimated { get; set; }
    private bool RequestInfoRefresh { get; set; }

    #endregion

    #region Methods

    #region Init

    private void Start()
    {
        localRooms = new List<RoomInfo>();
        roomButtons = new List<RoomButton>();
        SelectedRoom = null;
        NetworkLauncher.Instance.roomsUI = this;
    }

    #endregion
  
    #region Rooms List

    public void RefreshRooms(List<RoomInfo> serverRooms)
    {
        var removables = new List<RoomInfo>();
        foreach(var r in localRooms)
        {
            if (!serverRooms.Any(_=>_.Name == r.Name))
            {
                removables.Add(r);
            }
        }
        foreach(var r in removables)
        {
            RemoveLocalRoom(r);
        }
        foreach (var r in serverRooms)
        {
            if (!localRooms.Any(_=>_.Name == r.Name))
            {
                AddLocalRoom(r);
            }
            else
            {
                UpdateLocalRoom(r);
            }
        }
        noRoomsText.SetActive(localRooms.Count == 0);
    }

    private void AddLocalRoom(RoomInfo roomInfo)
    {
        var roomButton = Instantiate(roomButtonPrefab, roomButtonContainer);
        roomButton.RoomInfo = roomInfo;
        roomButton.MainMenuUI = mainMenuUI;
        roomButton.RoomsUI = this;
        localRooms.Add(roomInfo);
        roomButtons.Add(roomButton);
    }
    private void UpdateLocalRoom(RoomInfo newRoomInfo)
    {
        var btn = roomButtons.FirstOrDefault(_ => _.RoomInfo.Name == newRoomInfo.Name);
        if (btn != null)
        {
            if (SelectedRoom == btn.RoomInfo)
            {
                SelectedRoom = newRoomInfo;
            }
            btn.RoomInfo = newRoomInfo;
        }
        else
        {
            Debug.LogError("A room's name has been modified which is not expected.");
        }
    }

    private void RemoveLocalRoom(RoomInfo roomInfo)
    {
        localRooms.Remove(roomInfo);
        if (roomInfo == SelectedRoom)
        {
            SelectedRoom = null;
        }
        var rb = roomButtons.FirstOrDefault(_ => _.RoomInfo == roomInfo);
        if(rb!=null)
        {
            roomButtons.Remove(rb);
            Destroy(rb.gameObject);
        }
    }

    #endregion

    #region Room Selection

    public void OnRoomSelected(RoomInfo r)
    {
        SelectedRoom = r;
    }

    #endregion

    #region Create and Join Room

    public void TryCreateRoom(string roomName, string roomPassword, string username)
    {
        if (!NetworkLauncher.Instance.IsNewRoomNameValid(roomName))
        {
            OnIncorrectRoomName();
        }
        else if (string.IsNullOrWhiteSpace(username))
        {
            OnIncorrectUsername();
        }
        else
        {
            NetworkLauncher.Instance.CreateRoom(roomName, roomPassword, username);
        }
    }

    public void TryJoinRoom(string roomPassword, string username)
    {
        if (SelectedRoom == null)
        {
            Debug.LogError("SelectedRoom is null.");
        }
        else
        {
            NetworkLauncher.Instance.JoinRoomAs(SelectedRoom, roomPassword, username);
        }
    }

    public void OnSuccessfullyJoinedRoom()
    {
        ShowLoadingPopUp();
        NetworkLauncher.Instance.roomsUI = null; 
        StartCoroutine(LoadCharacterSelectionSceneAsync());
    }

    private IEnumerator LoadCharacterSelectionSceneAsync()
    {
        yield return new WaitForSeconds(Globals.LoadingDelay);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Globals.CharacterSelectionScene, LoadSceneMode.Single);
    }

    #region Info

    public void OnIncorrectUsername()
    {
        ShowHideInfoMessage("Username is invalid or already exists in the room.");
    }
    public void OnIncorrectRoomName()
    {
        ShowHideInfoMessage("Room name is invalid or a room already exists with this name.");
    }
    public void OnIncorrectRoomPassword()
    {
        ShowHideInfoMessage("Incorrect room password.");
    }
    private void ShowHideInfoMessage(string message)
    {
        infoText.text = message;
        StartCoroutine(FadeInAndOutInfoMessage());
    }

    private IEnumerator FadeInAndOutInfoMessage()
    {
        if (IsInfoBeingAnimated)
        {
            RequestInfoRefresh = true;
        }
        yield return new WaitUntil(() => !IsInfoBeingAnimated);
        IsInfoBeingAnimated = true;
        infoCanvasGroup.gameObject.SetActive(true);
        while (infoCanvasGroup.alpha < 1 && !RequestInfoRefresh)
        {
            infoCanvasGroup.alpha += Time.deltaTime / infoFadeDuration;
            yield return null;
        }
        if (!RequestInfoRefresh)
        {
            infoCanvasGroup.alpha = 1;
        }
        float elapsedSeconds = 0;
        while (elapsedSeconds < infoShowDuration && !RequestInfoRefresh)
        {
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        while (infoCanvasGroup.alpha > 0 && !RequestInfoRefresh)
        {
            infoCanvasGroup.alpha -= Time.deltaTime / infoFadeDuration;
            yield return null;
        }
        if (!RequestInfoRefresh)
        {
            infoCanvasGroup.alpha = 0;
            infoCanvasGroup.gameObject.SetActive(true);
        }
        RequestInfoRefresh = false;
        IsInfoBeingAnimated = false;
    }

    public void OnNetworkError(string errorMessage)
    {
        HideLoadingPopUp();
    }

    #endregion

    #endregion

    #region Loading Popup

    private void ShowLoadingPopUp()
    {
        loadingPopUp.SetActive(true);
        joinRoomPopUpUI.gameObject.SetActive(false);
        createRoomPopUp.SetActive(false);
    }

    private void HideLoadingPopUp()
    {
        loadingPopUp.SetActive(false);
    }

    #endregion

    #region Button Click Handlers

    public void OpenCreateRoomPopUp()
    {
        createRoomPopUp.SetActive(true);
    }

    public void OpenJoinRoomPopUp()
    {
        joinRoomPopUpUI.gameObject.SetActive(true);
    }

    public void BackToMainMenu()
    {
        mainMenuUI.NavigateToMainMenuPage();
    }

    #endregion

    #endregion
}
