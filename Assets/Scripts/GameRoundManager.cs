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
    [SerializeField] private CameraController cameraController;
    [SerializeField] private List<Transform> spawnPoints;
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

    public void OnCharacterDied(Character character)
    {
        /*aliveCharacters.Remove(character);
        if (aliveCharacters.Count == 1)
        {
            aliveCharacters[0].OnWin();
            OnGameEnd();
        }*/
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
        for (int i = 0; i < randomNumberList.Count; i++)
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
        InitializeCharacter(characterPrefab);
    }

    private void InitializeCharacter(GameObject characterGameObject)
    {
        var character = characterGameObject.GetComponent<Character>();
        var characterController = characterGameObject.AddComponent<CharacterController>();
        character.InitializeAsLocalCharacter(characterUI);
        characterUI.Initialize(character);
        characterController.Initialize(characterUI);
        cameraController.Initialize(character);
        Debug.Log($"The character of {PhotonNetwork.LocalPlayer.NickName} has been initialized.");
    }


    #endregion

    #region Left Room

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(Globals.MainMenuScene);
        Debug.Log("Left room.");
    }

    #endregion

    private void OnGameEnd()
    {
        // TODO:
        // wait for rematch requests
        // or quit if at least on of them quitted
        Debug.Log("Game round ended.");
        RoundEnded = true;
    }

    #endregion
}
