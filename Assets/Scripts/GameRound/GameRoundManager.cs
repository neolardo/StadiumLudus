using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages an in-game round.
/// </summary>
public class GameRoundManager : MonoBehaviourPunCallbacks
{
    #region Properties and Fields
    public static GameRoundManager Instance { get; private set; }

    [SerializeField] private CharacterUI characterUI;
    [SerializeField] private BlackScreenUI blackScreenUI;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private List<Transform> spawnPoints;
    private Character localCharacter;
    public bool RoundStarted { get; private set; } = false;
    public bool RoundEnded { get; private set; } = false;

    private const string PhotonPrefabsFolder = "PhotonPrefabs";
    private const string CharactersFolder = "Characters";
    private const string MaleWarriorPrefabName = "MaleWarrior";
    private const string FemaleWarriorPrefabName = "FemaleWarrior";
    private const string MaleRangerPrefabName = "MaleRanger";
    private const string FemaleRangerPrefabName = "FemaleRanger";

    #endregion

    #region Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    #region Initialize

    public override void OnEnable()
    {
        base.OnEnable();
        InitializeGameRound();
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }

    private string GetCharacterPrefabNameOfPlayer(Player player)
    {
        var fightingStyle = (CharacterFightingStyle)player.CustomProperties[Globals.PlayerFightingStyleCustomPropertyKey];
        var classs = (CharacterClass)player.CustomProperties[Globals.PlayerClassCustomPropertyKey];
        var path = Path.Combine(PhotonPrefabsFolder, CharactersFolder);
        if (fightingStyle == CharacterFightingStyle.Heavy && classs == CharacterClass.Barbarian)
        {
            return Path.Combine(path, MaleWarriorPrefabName);
        }
        else if (fightingStyle == CharacterFightingStyle.Light && classs == CharacterClass.Barbarian)
        {
            return Path.Combine(path, FemaleWarriorPrefabName);
        }
        else if (fightingStyle == CharacterFightingStyle.Light && classs == CharacterClass.Ranger)
        {
            return Path.Combine(path, FemaleRangerPrefabName);
        }
        else if (fightingStyle == CharacterFightingStyle.Heavy && classs == CharacterClass.Ranger)
        {
            return Path.Combine(path, MaleRangerPrefabName);
        }
        else
        {
            Debug.LogError("Unable to get character prefab name.");
            return "";
        }
    }

    private void InitializeGameRound()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Game loaded.");
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            SpawnCharacters();
        }
    }

    private void SpawnCharacters()
    {
        Debug.Log("Spawning characters...");
        var randomNumberList = Globals.GenerateRandomIndexes(0, PhotonNetwork.CurrentRoom.PlayerCount, spawnPoints.Count);
        var dictionary = new Dictionary<string, int>();
        var playerList = PhotonNetwork.PlayerList;
        for (int i = 0; i < playerList.Length; i++)
        {
            dictionary.Add(playerList[i].NickName, randomNumberList[i]);
        }
        photonView.RPC(nameof(InstantiateCharacter), RpcTarget.AllBuffered, dictionary);
    }


    [PunRPC]
    public void InstantiateCharacter(Dictionary<string, int> playerNameSpawnIndexDictionary)
    {
        Debug.Log($"Initialiazing the character of {PhotonNetwork.LocalPlayer.NickName}...");
        int spawnIndex = playerNameSpawnIndexDictionary[PhotonNetwork.LocalPlayer.NickName];
        var characterPrefab = PhotonNetwork.Instantiate(GetCharacterPrefabNameOfPlayer(PhotonNetwork.LocalPlayer), spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
        localCharacter = characterPrefab.GetComponent<Character>();
        characterPrefab.AddComponent<AudioListener>();
        var characterController = characterPrefab.AddComponent<CharacterController>();
        localCharacter.InitializeAsLocalCharacter(characterUI);
        characterUI.Initialize(localCharacter);
        characterController.Initialize(characterUI);
        cameraController.Initialize(localCharacter);
        SetPlayerIsInitialized(PhotonNetwork.LocalPlayer, true);
        SetPlayerIsAlive(PhotonNetwork.LocalPlayer, true);
        Debug.Log($"The character of {PhotonNetwork.LocalPlayer.NickName} has been initialized.");
    }

    #endregion

    #region Left Room

    public override void OnLeftRoom()
    {
        SetPlayerIsInitialized(PhotonNetwork.LocalPlayer, false);
        SetPlayerIsAlive(PhotonNetwork.LocalPlayer, false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(Globals.MainMenuScene);
        Debug.Log("Left room.");
    }

    #endregion

    #region Player Properties

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (CanRoundStart())
        {
            StartRound();
        }
        else if (CanRoundEnd())
        {
            EndRound();
        }
    }

    #endregion

    #region Round Start

    private void SetPlayerIsInitialized(Player player, bool value)
    {
        var hashtable = player.CustomProperties == null ? new ExitGames.Client.Photon.Hashtable() : player.CustomProperties;
        if (hashtable.ContainsKey(Globals.PlayerIsInitializedKey))
        {
            hashtable[Globals.PlayerIsInitializedKey] = value;
        }
        else
        {
            hashtable.Add(Globals.PlayerIsInitializedKey, value);
        }
        player.SetCustomProperties(hashtable);
    }

    private bool CanRoundStart()
    {
        if (RoundStarted)
        {
            return false;
        }
        else
        {
            var playerList = PhotonNetwork.PlayerList;
            int aliveCount = 0;
            int initializedCount = 0;
            foreach (var p in playerList)
            {
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsAliveKey) && (bool)p.CustomProperties[Globals.PlayerIsAliveKey])
                {
                    aliveCount++;
                }
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsInitializedKey) && (bool)p.CustomProperties[Globals.PlayerIsInitializedKey])
                {
                    initializedCount++;
                }
            }
            return (aliveCount == initializedCount) && (initializedCount == playerList.Length);
        }
    }

    private void StartRound()
    {
        Debug.Log("Game round started.");
        RoundStarted = true;
        characterUI.SetUIVisiblity(true);
        blackScreenUI.FadeOutAndDisable();
    }

    #endregion

    #region Round End

    private void SetPlayerIsAlive(Player player, bool value)
    {
        var hashtable = player.CustomProperties == null ? new ExitGames.Client.Photon.Hashtable() : player.CustomProperties;
        if (hashtable.ContainsKey(Globals.PlayerIsAliveKey))
        {
            hashtable[Globals.PlayerIsAliveKey] = value;
        }
        else
        {
            hashtable.Add(Globals.PlayerIsAliveKey, value);
        }
        player.SetCustomProperties(hashtable);
    }

    public void OnLocalCharacterDied()
    {
        SetPlayerIsAlive(PhotonNetwork.LocalPlayer, false);
    }

    private bool CanRoundEnd()
    {
        if (RoundEnded || !RoundStarted)
        {
            return false;
        }
        else
        {
            var playerList = PhotonNetwork.PlayerList;
            int aliveCount = 0;
            foreach (var p in playerList)
            {
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsAliveKey) && (bool)p.CustomProperties[Globals.PlayerIsAliveKey])
                {
                    aliveCount++;
                }
            }
            return aliveCount <= 1;
        }
    }

    private void EndRound()
    {
        Debug.Log("Game round ended.");
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Globals.PlayerIsAliveKey) && (bool)PhotonNetwork.LocalPlayer.CustomProperties[Globals.PlayerIsAliveKey])
        {
            localCharacter.OnWin();
        }
        RoundEnded = true;
    }

    #endregion

    #endregion
}
