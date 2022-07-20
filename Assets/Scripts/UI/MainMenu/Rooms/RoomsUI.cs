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

    [SerializeField] private MainMenuUIManager menuManager;
    [SerializeField] private RectTransform roomButtonContainer;
    [SerializeField] private RoomButton roomButtonPrefab;
    [SerializeField] private GameObject createRoomPopUp;
    [SerializeField] private GameObject joinRoomPopUp;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private GameObject noRoomsText;
    [SerializeField] private GameObject loadingPopUp;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private CanvasGroup infoCanvasGroup;

    private bool isInfoBeingAnimated;
    private bool requestInfoRefresh;
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
            else
            {
                joinRoomPopUp.SetActive(false);
            }
            joinRoomButton.interactable = _selectedRoom != null;
        }
    }
    private const float infoFadeInDuration = 0.3f;
    private const float infoFadeOutDuration = 0.5f;
    private const float infoShowDuration = 2.5f;

    #endregion

    #region Methods

    #region Init

    private void Start()
    {
        localRooms = new List<RoomInfo>();
        roomButtons = new List<RoomButton>();
        SelectedRoom = null;
        NetworkLauncher.Instance.RoomListUpdated += RefreshRooms;
        NetworkLauncher.Instance.TriedToJoinRoom += OnTriedToJoinRoom;
    }

    private void OnDestroy()
    {
        NetworkLauncher.Instance.RoomListUpdated -= RefreshRooms;
        NetworkLauncher.Instance.TriedToJoinRoom -= OnTriedToJoinRoom;
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
        roomButton.Clicked += OnRoomSelected;
        roomButton.Hovered += menuManager.OnButtonHover;
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
            Debug.LogError("A room name has been modified which is not expected.");
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
        if(rb != null)
        {
            rb.Clicked -= OnRoomSelected;
            rb.Hovered -= menuManager.OnButtonHover;
            roomButtons.Remove(rb);
            Destroy(rb.gameObject);
        }
    }

    #endregion

    #region Room Selection

    public void OnRoomSelected(RoomInfo r)
    {
        SelectedRoom = r;
        if(SelectedRoom != null)
        {
            menuManager.OnButtonClick();
            OpenJoinRoomPopUp();
        }
    }

    #endregion

    #region Create and Join Room

    public void TryCreateRoom(string roomName, string roomPassword, string username)
    {
        if (!NetworkLauncher.Instance.IsNewRoomNameValid(roomName))
        {
            OnTriedToJoinRoom(JoinRoomResponse.IncorrectRoomName);
        }
        else if (string.IsNullOrWhiteSpace(username))
        {
            OnTriedToJoinRoom(JoinRoomResponse.IncorrectUserName);
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
            Debug.LogError($"{nameof(SelectedRoom)} is null.");
        }
        else
        {
            NetworkLauncher.Instance.JoinRoomAs(SelectedRoom, roomPassword, username);
        }
    }

    public void OnSuccessfullyJoinedRoom()
    {
        ShowLoadingPopUp();
        NetworkLauncher.Instance.RoomListUpdated -= RefreshRooms;
        NetworkLauncher.Instance.TriedToJoinRoom -= OnTriedToJoinRoom;
        StartCoroutine(LoadCharacterSelectionSceneAfterDelay());
    }

    private IEnumerator LoadCharacterSelectionSceneAfterDelay()
    {
        yield return new WaitForSeconds(Globals.LoadingDelay);
        SceneManager.LoadScene(Globals.CharacterSelectionScene, LoadSceneMode.Single);
    }

    public void OnTriedToJoinRoom(JoinRoomResponse response)
    {
        switch (response)
        {
            case JoinRoomResponse.IncorrectUserName:
                ShowHideInfoMessage("Username is invalid or already exists in the room.");
                break;
            case JoinRoomResponse.IncorrectRoomName:
                ShowHideInfoMessage("Room name is invalid or a room already exists with this name.");
                break;
            case JoinRoomResponse.IncorrectRoomPassword:
                ShowHideInfoMessage("Incorrect room password.");
                break;
            case JoinRoomResponse.Successful:
                OnSuccessfullyJoinedRoom();
                break;
            default:
                Debug.LogError($"Invalid {nameof(JoinRoomResponse)} value.");
                break;
        }
    }

    #region Info

    private void ShowHideInfoMessage(string message)
    {
        infoText.text = message;
        StartCoroutine(FadeInAndOutInfoMessage());
    }

    private IEnumerator FadeInAndOutInfoMessage()
    {
        if (isInfoBeingAnimated)
        {
            requestInfoRefresh = true;
        }
        yield return new WaitUntil(() => !isInfoBeingAnimated);
        isInfoBeingAnimated = true;
        infoCanvasGroup.gameObject.SetActive(true);
        while (infoCanvasGroup.alpha < 1 && !requestInfoRefresh)
        {
            infoCanvasGroup.alpha += Time.deltaTime / infoFadeInDuration;
            yield return null;
        }
        if (!requestInfoRefresh)
        {
            infoCanvasGroup.alpha = 1;
        }
        float elapsedSeconds = 0;
        while (elapsedSeconds < infoShowDuration && !requestInfoRefresh)
        {
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        while (infoCanvasGroup.alpha > 0 && !requestInfoRefresh)
        {
            infoCanvasGroup.alpha -= Time.deltaTime / infoFadeOutDuration;
            yield return null;
        }
        if (!requestInfoRefresh)
        {
            infoCanvasGroup.alpha = 0;
            infoCanvasGroup.gameObject.SetActive(true);
        }
        requestInfoRefresh = false;
        isInfoBeingAnimated = false;
    }

    public void OnNetworkError(string errorMessage)
    {
        HideLoadingPopUp();
        Debug.LogError(errorMessage);
    }

    #endregion

    #endregion

    #region Loading Popup

    private void ShowLoadingPopUp()
    {
        loadingPopUp.SetActive(true);
        joinRoomPopUp.SetActive(false);
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
        joinRoomPopUp.SetActive(true);
    }

    public void BackToMainMenu()
    {
        menuManager.NavigateTo(MainMenuPage.MainMenu);
    }

    #endregion

    #endregion
}
