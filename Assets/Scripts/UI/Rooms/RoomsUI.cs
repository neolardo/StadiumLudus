using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    private List<Room> rooms;
    private List<RoomButton> roomButtons;

    private Room _selectedRoom;
    private Room SelectedRoom 
    {
        get 
        {
            return _selectedRoom;
        }
        set
        {
            if (_selectedRoom != null)
            {
                var rb = roomButtons.FirstOrDefault(_ => _.Room == _selectedRoom);
                if (rb != null)
                {
                    rb.ChangeSelectionColor(false);
                }
            }
            _selectedRoom = value;
            if (_selectedRoom != null)
            {
                var rb = roomButtons.FirstOrDefault(_ => _.Room == _selectedRoom);
                if (rb != null)
                {
                    rb.ChangeSelectionColor(true);
                }
            }
            joinRoomButton.interactable = _selectedRoom != null;
        }
    }

    private const float roomRefreshRate = 10f;

    #endregion

    #region Methods

    private void Start()
    {
        rooms = new List<Room>();
        roomButtons = new List<RoomButton>();
        StartCoroutine(ManageRefreshRooms());
        SelectedRoom = null;
    }

    private IEnumerator ManageRefreshRooms()
    {
        while (true)
        {
            RefreshRooms();
            yield return new WaitForSeconds(roomRefreshRate);
        }
    }

    private void RefreshRooms()
    {
        var serverRooms = GetRoomsFromServer();
        var removables = new List<Room>();
        foreach(var r in rooms)
        {
            if (!serverRooms.Any(_=> _.IsSameAs(r)))
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
            if (!rooms.Any(_=>_.IsSameAs(r)))
            {
                AddLocalRoom(r);
            }
        }
        noRoomsText.SetActive(rooms.Count == 0);
    }

    public void OnRoomSelected(Room r)
    {
        SelectedRoom = r;
    }

    private List<Room> GetRoomsFromServer()
    {
        // todo
        Room a = new Room("my room", "my pass", "player");
        Room b = new Room("my second room", "my pass", "player");
        Room c = new Room("my third room", "my pass", "player");
        var list = new List<Room>();
        list.Add(a);
        list.Add(b);
        list.Add(c);
        return list;
    }

    private void AddLocalRoom(Room r)
    {
        var roomButton = Instantiate(roomButtonPrefab, roomButtonContainer);
        roomButton.Room = r;
        roomButton.MainMenuUI = mainMenuUI;
        roomButton.RoomsUI = this;
        rooms.Add(r);
        roomButtons.Add(roomButton);
    }

    private void RemoveLocalRoom(Room r)
    {
        rooms.Remove(r);
        if (r == SelectedRoom)
        {
            SelectedRoom = null;
        }
        var rb = roomButtons.FirstOrDefault(_ => _.Room == r);
        if(rb!=null)
        {
            roomButtons.Remove(rb);
            Destroy(rb.gameObject);
        }
    }
    private void AddRoomToServer(Room r)
    {
        //TODO
    }

    public void TryCreateRoom(string roomName, string roomPassword, string username)
    {
        if (rooms.Any(_ => _.Name == roomName))
        {
            //TODO: handle invalid roomname
        }
        else if(rooms.Any(_ =>_.Players.Any(_ => _ == username)))
        { 
            //TODO: handle invalid username
        }
        else
        {
            var r = new Room(roomName, roomPassword, username);
            AddLocalRoom(r);
            AddRoomToServer(r);
            // todo load character selection scene
        }
    }

    public void TryJoinRoom(string roomPassword, string username)
    {
        if (SelectedRoom != null && SelectedRoom.Password == roomPassword && !SelectedRoom.Players.Any(_ => _ == username) && SelectedRoom.Players.Count < Room.MaximumPlayerCount)
        {
            SelectedRoom.Players.Add(username);
            //TODO refresh on server side
            // todo load character selection scene
        }
    }

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
}
