using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

/// <summary>
/// Manages the initialization of the photon network at the start of the application.
/// </summary>
public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    #region Properties and Fields

    public static NetworkLauncher Instance { get; private set; }
    public bool IsDisconnected { get; private set; }
    public bool IsPracticeMode { get; private set; }

    private List<RoomInfo> networkRooms;
    private bool isPlayerTheCreatorOfTheRoom = false;
    private string tempRoomPassword;

    #endregion

    #region Events

    public event Action Connected;
    public event Action<string> Disconnected;
    public event Action<Player> PlayerEnteredRoom;
    public event Action<Player> PlayerLeftRoom;
    public event Action<Player> PlayerPropertiesChanged;
    public event Action<List<RoomInfo>> RoomListUpdated;
    public event Action<JoinRoomResponse> TriedToJoinRoom;
    public event Action StartingGame;
    public event Action CreateRoomFailed;

    #endregion

    #region Init

    private void Awake()
    {
        if (Instance == null)
        {
            Initialize();
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public void Initialize()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        networkRooms = new List<RoomInfo>();
        Debug.Log("Connecting to master...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        string message = cause switch
        {
            DisconnectCause.MaxCcuReached => "Servers are full.\nPlease try again later.",
            _ => MainMenuUI.NetworkErrorMessage,
        };
        IsDisconnected = true;
        Disconnected?.Invoke(message);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby.");
        Connected?.Invoke();
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left lobby.");
    }

    #endregion

    #region Room List

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated.");
        networkRooms.Clear();
        networkRooms.AddRange(roomList.Where(_ => !_.RemovedFromList));
        RoomListUpdated?.Invoke(networkRooms);
    }

    #endregion

    #region Create Room

    public void CreatePracticeRoom()
    {
        IsPracticeMode = true;
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1, IsVisible = false, IsOpen = false });
    }

    public void CreateRoom(string roomName, string roomPassword, string firstUser)
    {
        IsPracticeMode = false;
        isPlayerTheCreatorOfTheRoom = true;
        tempRoomPassword = roomPassword;
        PhotonNetwork.NickName = firstUser;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = Globals.MaximumPlayerCountPerRoom, IsVisible = true, IsOpen = true, BroadcastPropsChangeToAll = true });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError(message);
        tempRoomPassword = "";
        isPlayerTheCreatorOfTheRoom = false;
        CreateRoomFailed?.Invoke();
        IsPracticeMode = false;
    }

    public bool IsNewRoomNameValid(string roomName)
    {
        return !string.IsNullOrWhiteSpace(roomName) && !networkRooms.Any(_ => _.Name == roomName);
    }

    private void SetRoomPassword()
    {
        var hashtable = Globals.SetHash(PhotonNetwork.CurrentRoom.CustomProperties, Globals.RoomPasswordCustomPropertyKey, tempRoomPassword);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
        tempRoomPassword = "";
    }


    #endregion

    #region Join Room

    public void JoinRoomAs(RoomInfo room, string roomPassword, string username)
    {
        PhotonNetwork.NickName = username;
        tempRoomPassword = roomPassword;
        PhotonNetwork.JoinRoom(room.Name);
    }

    public override void OnJoinedRoom()
    {   
        if (IsPracticeMode)
        {
            Debug.Log($"Created and joined practice room: {PhotonNetwork.CurrentRoom.Name}");
            TriedToJoinRoom?.Invoke(JoinRoomResponse.Successful);
        }
        else if (isPlayerTheCreatorOfTheRoom)
        {
            isPlayerTheCreatorOfTheRoom = false;
            SetRoomPassword();
            Debug.Log($"Created and joined room: {PhotonNetwork.CurrentRoom.Name}");
            TriedToJoinRoom?.Invoke(JoinRoomResponse.Successful);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName) || PhotonNetwork.PlayerListOthers.Any(_ => _.NickName == PhotonNetwork.NickName))
            {
                TriedToJoinRoom?.Invoke(JoinRoomResponse.IncorrectUserName);
                PhotonNetwork.LeaveRoom();
            }
            else if (tempRoomPassword != PhotonNetwork.CurrentRoom.CustomProperties[Globals.RoomPasswordCustomPropertyKey].ToString())
            {
                TriedToJoinRoom?.Invoke(JoinRoomResponse.IncorrectRoomPassword);
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
                TriedToJoinRoom?.Invoke(JoinRoomResponse.Successful);
            }
            tempRoomPassword = "";
        }
    }

    #endregion

    #region Left Room

    public override void OnLeftRoom()
    {
        IsPracticeMode = false;
        UpdatePlayerIsCharacterConfirmedCustomProperty(PhotonNetwork.LocalPlayer, false);
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != Globals.MainMenuScene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Globals.MainMenuScene);
        }
        Debug.Log("Left room.");
    }

    #endregion

    #region Players In Room

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerEnteredRoom?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerLeftRoom?.Invoke(otherPlayer);
    }

    #endregion

    #region Character Selection

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        PlayerPropertiesChanged?.Invoke(targetPlayer);
        StartGameIfEveryPlayerIsReady();
    }

    public void StartGameIfEveryPlayerIsReady()
    {
        if (IsEveryPlayerConfirmedTheirCharacter())
        {
            StartingGame?.Invoke();
            PhotonNetwork.LoadLevel(Globals.GameScene);
            Debug.Log("Loading game...");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Not enough players or not enough confirmations yet to load game...");
        }
    }

    public void OnCharacterConfirmed(Player player, CharacterFightingStyle fs, CharacterClass c)
    {
        UpdatePlayerCharacterCustomProperties(player, fs, c);
        UpdatePlayerIsCharacterConfirmedCustomProperty(player, true);
    }

    private void UpdatePlayerCharacterCustomProperties(Player player, CharacterFightingStyle fs, CharacterClass c)
    {
        var hashtable = Globals.SetHash(player.CustomProperties, Globals.PlayerFightingStyleCustomPropertyKey, (int)fs);
        hashtable = Globals.SetHash(hashtable, Globals.PlayerClassCustomPropertyKey, (int)c);
        player.SetCustomProperties(hashtable);
    }

    private void UpdatePlayerIsCharacterConfirmedCustomProperty(Player player, bool isCharacterConfirmed)
    {
        var hashtable = Globals.SetHash(player.CustomProperties, Globals.PlayerIsCharacterConfirmedKey, isCharacterConfirmed);
        player.SetCustomProperties(hashtable);
    }

    public bool IsEveryPlayerConfirmedTheirCharacter()
    {
        var players = PhotonNetwork.PlayerList;
        if (players.Length < 2 && !IsPracticeMode)
        {
            return false;
        }
        foreach (var p in players)
        {
            if (!IsPracticeMode && (!p.CustomProperties.ContainsKey(Globals.PlayerIsCharacterConfirmedKey) || !(bool)p.CustomProperties[Globals.PlayerIsCharacterConfirmedKey]))
            {
                return false;
            }
            else if(IsPracticeMode && p.CustomProperties.ContainsKey(Globals.PlayerIsCharacterConfirmedKey) && (bool)p.CustomProperties[Globals.PlayerIsCharacterConfirmedKey])
            {
                return true;
            }
        }
        return true;
    }

    #endregion
}
