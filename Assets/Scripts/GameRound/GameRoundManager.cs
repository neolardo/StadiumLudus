using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

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
    [SerializeField] private CharacterAudioListener characterAudioListenerPrefab;
    [SerializeField] private GameObject lightPrefab;
    [SerializeField] private List<Transform> spawnPoints;
    private Character localCharacter;
    private List<Player> RematchRequestingPlayers { get; } = new List<Player>();
    private List<Player> DeadCharacters { get; } = new List<Player>();
    public Dictionary<int, Character> LocalCharacterReferenceDictionary { get; } = new Dictionary<int, Character>();
    public bool RoundStarted { get; private set; } = false;
    public bool RoundEnded { get; private set; } = false;
    public bool RematchStarted { get; private set; } = false;

    private const string PhotonPrefabsFolderName = "PhotonPrefabs";
    private const string CharactersFolderName = "Characters";
    private const string WeaponsFolderName = "Weapons";
    private const string MaleWarriorPrefabName = "MaleWarrior";
    private const string FemaleWarriorPrefabName = "FemaleWarrior";
    private const string MaleRangerPrefabName = "MaleRanger";
    private const string FemaleRangerPrefabName = "FemaleRanger";

    private const string EnvironmentRootGameObjectName = "Environment";

    #endregion

    #region Methods

    private void Awake()
    {
        if (Instance != this)
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;
        }
    }

    #region Prefab Name

    public string GetWeaponPrefabName(GameObject weaponPrefab)
    {
        return Path.Combine(PhotonPrefabsFolderName, WeaponsFolderName, weaponPrefab.name);
    }

    private string GetCharacterPrefabNameOfPlayer(Player player)
    {
        var fightingStyle = (CharacterFightingStyle)player.CustomProperties[Globals.PlayerFightingStyleCustomPropertyKey];
        var classs = (CharacterClass)player.CustomProperties[Globals.PlayerClassCustomPropertyKey];
        var path = Path.Combine(PhotonPrefabsFolderName, CharactersFolderName);
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

    #endregion

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

    private void InitializeGameRound()
    {
        if (PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name == Globals.GameScene)
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
        var characterController = characterPrefab.AddComponent<CharacterController>();
        characterUI.Initialize(localCharacter);
        characterController.Initialize(characterUI);
        cameraController.Initialize(localCharacter);
        var characterAudioListener = Instantiate(characterAudioListenerPrefab, null);
        characterAudioListener.SetTarget(localCharacter.transform);
        Instantiate(lightPrefab, localCharacter.transform);
        localCharacter.InitializeAsLocalCharacter(characterUI);
        SetPlayerIsCharacterConfirmed(PhotonNetwork.LocalPlayer, false);
        SetPlayerIsInitialized(PhotonNetwork.LocalPlayer, true);
        SetPlayerIsRematchRequested(PhotonNetwork.LocalPlayer, false);
        Debug.Log($"The character of {PhotonNetwork.LocalPlayer.NickName} has been initialized.");
    }

    #endregion

    #region Left Room

    public override void OnLeftRoom()
    {
        SetPlayerIsCharacterConfirmed(PhotonNetwork.LocalPlayer, false);
        SetPlayerIsInitialized(PhotonNetwork.LocalPlayer, false);
        SetPlayerIsRematchRequested(PhotonNetwork.LocalPlayer, false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(Globals.MainMenuScene);
        Debug.Log("Left room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            PhotonNetwork.LeaveRoom();
        }
        else if (LocalCharacterReferenceDictionary != null)
        {
            var otherCharacter = LocalCharacterReferenceDictionary.Values.Where(_ => _.PhotonView.Controller == otherPlayer).FirstOrDefault();
            if (otherCharacter != null)
            {
                var key = LocalCharacterReferenceDictionary.First(_ => _.Value == otherCharacter).Key;
                LocalCharacterReferenceDictionary.Remove(key);
            }
        }
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
        else if (CanRematch())
        {
            Rematch();
        }
    }

    #endregion

    #region Round Start

    private void SetPlayerIsCharacterConfirmed(Player player, bool value)
    {
        var hashtable = Globals.SetHash(player.CustomProperties, Globals.PlayerIsCharacterConfirmedKey, value);
        player.SetCustomProperties(hashtable);
    }

    private void SetPlayerIsInitialized(Player player, bool value)
    {
        var hashtable = Globals.SetHash(player.CustomProperties, Globals.PlayerIsInitializedKey, value);
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
            int initializedCount = 0;
            int rematchNotRequestedCount = 0;
            foreach (var p in playerList)
            {
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsInitializedKey) && (bool)p.CustomProperties[Globals.PlayerIsInitializedKey])
                {
                    initializedCount++;
                }
                if (!p.CustomProperties.ContainsKey(Globals.PlayerIsRematchRequestedKey) || (p.CustomProperties.ContainsKey(Globals.PlayerIsRematchRequestedKey) && !(bool)p.CustomProperties[Globals.PlayerIsRematchRequestedKey]))
                {
                    rematchNotRequestedCount++;
                }
            }
            return  (initializedCount == rematchNotRequestedCount) && (initializedCount == playerList.Length);
        }
    }

    private void StartRound()
    {
        Debug.Log("Game round started.");
        var characters = FindObjectsOfType<Character>();
        foreach (var c in characters)
        {
            LocalCharacterReferenceDictionary.Add(c.PhotonView.ViewID, c);
        }
        foreach (var c in characters)
        {
            c.InitializeCharacterList();
        }
        RoundStarted = true;
        characterUI.SetUIVisiblity(true);
        blackScreenUI.FadeOutAndDisable();
    }

    #endregion

    #region Round End

    public void OnCharacterDied()
    {
        OnPlayerPropertiesUpdate(null, null);
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
            var characters = LocalCharacterReferenceDictionary.Values;
            int aliveCount = 0;
            foreach(var c in characters)
            {
                if (c.IsAlive)
                {
                    aliveCount++;
                }
                else if(!DeadCharacters.Contains(c.PhotonView.Owner))
                {
                    DeadCharacters.Add(c.PhotonView.Owner);
                    characterUI.AddInfo($"{c.PhotonView.Owner.NickName} has been slain.");
                }
            }
            int initializedCount = 0;
            foreach (var p in playerList)
            {
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsInitializedKey) && (bool)p.CustomProperties[Globals.PlayerIsInitializedKey])
                {
                    initializedCount++;
                }
            }
            return (aliveCount <= 1) && (initializedCount == playerList.Length);
        }
    }

    private void EndRound()
    {
        Debug.Log("Game round ended.");
        if (localCharacter.IsAlive)
        {
            localCharacter.OnWin();
        }
        RoundEnded = true;
    }

    #endregion

    #region Rematch

    public void OnLocalPlayerRequestedRematch()
    {
        SetPlayerIsRematchRequested(PhotonNetwork.LocalPlayer, true);
        SetPlayerIsInitialized(PhotonNetwork.LocalPlayer, false);
    }

    private void SetPlayerIsRematchRequested(Player player, bool value)
    {
        var hashtable = Globals.SetHash(player.CustomProperties, Globals.PlayerIsRematchRequestedKey, value);
        player.SetCustomProperties(hashtable);
    }

    private bool CanRematch()
    {
        if (RematchStarted || !RoundEnded)
        {
            return false;
        }
        else
        {
            var playerList = PhotonNetwork.PlayerList;
            int rematchCount = 0;
            int notInitializedCount = 0;
            foreach (var p in playerList)
            {
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsRematchRequestedKey) && (bool)p.CustomProperties[Globals.PlayerIsRematchRequestedKey])
                {
                    if (!RematchRequestingPlayers.Contains(p))
                    {
                        RematchRequestingPlayers.Add(p);
                        characterUI.AddInfo($"{p.NickName} requested a rematch.");
                    }
                    rematchCount++;
                }
                if (p.CustomProperties.ContainsKey(Globals.PlayerIsInitializedKey) && !(bool)p.CustomProperties[Globals.PlayerIsInitializedKey])
                {
                    notInitializedCount++;
                }
            }
            return RoundEnded && playerList.Length > 1 && (rematchCount == playerList.Length) && (notInitializedCount == rematchCount);
        }
    }

    private void Rematch()
    {
        Debug.Log("Starting a rematch...");
        RematchStarted = true;
        StartCoroutine(FadeInBlackScreenAndReloadScene());
    }

    private IEnumerator FadeInBlackScreenAndReloadScene()
    {
        blackScreenUI.EnableAndFadeIn();
        yield return new WaitForSeconds(blackScreenUI.fadeSeconds*2);
        PhotonNetwork.LoadLevel(Globals.GameScene);
    }


    #endregion

    #endregion
}
