using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

/// <summary>
/// Manages the initialization of the photon network at the application's start.
/// </summary>
public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    #region Properties and Fields

    public static NetworkLauncher Instance { get; private set; }
    public RoomsUI roomsUI;
    private List<RoomInfo> networkRooms;
    private bool isPlayerTheCreatorOfTheRoom = false;
    private string tempRoomPassword;
    private bool shouldRejoinLobby = false;

    #endregion

    #region Events

    public event Action<Player> PlayerEnteredRoom;
    public event Action<Player> PlayerLeftRoom;

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

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby.");
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left lobby.");
        if (shouldRejoinLobby)
        {
            shouldRejoinLobby = false;
            PhotonNetwork.JoinLobby();
        }
    }

    public void TryRejoinLobby()
    {
        if (!PhotonNetwork.InRoom && PhotonNetwork.InLobby)
        {
            shouldRejoinLobby = true;
            PhotonNetwork.LeaveLobby();
        }
        else if(!PhotonNetwork.InRoom && !PhotonNetwork.InLobby)
        { 
            PhotonNetwork.JoinLobby();
        }
    }


    #endregion

    #region Room List

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("RoomList updated.");
        networkRooms.Clear();
        networkRooms.AddRange(roomList.Where(_ => !_.RemovedFromList));
        if (roomsUI != null)
        {
            roomsUI.RefreshRooms(networkRooms);
        }
    }

    #endregion

    #region Create Room

    public void CreateRoom(string roomName, string roomPassword, string firstUser)
    {
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
        roomsUI.OnNetworkError(message);
    }

    public bool IsNewRoomNameValid(string roomName)
    {
        return !string.IsNullOrWhiteSpace(roomName) && !networkRooms.Any(_ => _.Name == roomName);
    }

    private void SetRoomPassword()
    {
        var hashtable = PhotonNetwork.CurrentRoom.CustomProperties == null ? new Hashtable() : PhotonNetwork.CurrentRoom.CustomProperties;
        if (hashtable.ContainsKey(Globals.RoomPasswordCustomPropertyKey))
        {
            hashtable[Globals.RoomPasswordCustomPropertyKey] = tempRoomPassword;
        }
        else
        {
            hashtable.Add(Globals.RoomPasswordCustomPropertyKey, tempRoomPassword);
        }
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
        if (isPlayerTheCreatorOfTheRoom)
        {
            isPlayerTheCreatorOfTheRoom = false;
            SetRoomPassword();
            Debug.Log($"Created and joined room: {PhotonNetwork.CurrentRoom.Name}, password:{PhotonNetwork.CurrentRoom.CustomProperties[Globals.RoomPasswordCustomPropertyKey]}  username: {PhotonNetwork.NickName}");
            roomsUI.OnSuccesfullyJoinedRoom();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName) || PhotonNetwork.PlayerListOthers.Any(_ => _.NickName == PhotonNetwork.NickName))
            {
                roomsUI.OnIncorrectUsername();
                PhotonNetwork.LeaveRoom();
            }
            else if (tempRoomPassword != PhotonNetwork.CurrentRoom.CustomProperties[Globals.RoomPasswordCustomPropertyKey].ToString())
            {
                roomsUI.OnIncorrectRoomPassword();
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}, password:{PhotonNetwork.CurrentRoom.CustomProperties[Globals.RoomPasswordCustomPropertyKey]},  username: {PhotonNetwork.NickName}");
                roomsUI.OnSuccesfullyJoinedRoom();
            }
            tempRoomPassword = "";
        }
    }

    #endregion

    #region Left Room

    public override void OnLeftRoom()
    {
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
        if (PlayerEnteredRoom != null)
        {
            PlayerEnteredRoom(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerIsCharacterConfirmedCustomProperty(otherPlayer, false);
        if (PlayerLeftRoom != null)
        {
            PlayerLeftRoom(otherPlayer);
        }
    }

    #endregion

    #region Character Selection

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"{targetPlayer.NickName}'s properties has changed.");
        StartGameIfEveryPlayerIsReady();
    }

    public void StartGameIfEveryPlayerIsReady()
    {
        if (IsEveryPlayerConfirmedTheirCharacter())
        {
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
        var hashtable = player.CustomProperties == null ? new Hashtable() : player.CustomProperties;
        // fighting style
        if (hashtable.ContainsKey(Globals.PlayerFightingStyleCustomPropertyKey))
        {
            hashtable[Globals.PlayerFightingStyleCustomPropertyKey] = (int)fs;
        }
        else
        {
            hashtable.Add(Globals.PlayerFightingStyleCustomPropertyKey, (int)fs);
        }
        // class
        if (hashtable.ContainsKey(Globals.PlayerClassCustomPropertyKey))
        {
            hashtable[Globals.PlayerClassCustomPropertyKey] = (int)c;
        }
        else
        {
            hashtable.Add(Globals.PlayerClassCustomPropertyKey, (int)c);
        }
        player.SetCustomProperties(hashtable);
    }

    private void UpdatePlayerIsCharacterConfirmedCustomProperty(Player player, bool isCharacterConfirmed)
    {
        var hashtable = player.CustomProperties == null ? new Hashtable() : player.CustomProperties;
        if (hashtable.ContainsKey(Globals.PlayerFightingStyleCustomPropertyKey))
        {
            hashtable[Globals.PlayerIsCharacterConfirmedKey] = isCharacterConfirmed;
        }
        else
        {
            hashtable.Add(Globals.PlayerIsCharacterConfirmedKey, isCharacterConfirmed);
        }
        player.SetCustomProperties(hashtable);
    }

    public bool IsEveryPlayerConfirmedTheirCharacter()
    {
        var players = PhotonNetwork.PlayerList;
        if (players.Length < 2)
        {
            return false;
        }
        foreach (var p in players)
        {
            if (!p.CustomProperties.ContainsKey(Globals.PlayerIsCharacterConfirmedKey) || !(bool)p.CustomProperties[Globals.PlayerIsCharacterConfirmedKey])
            {
                return false;
            }
        }
        return true;
    }

    #endregion
}
