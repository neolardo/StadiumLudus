using UnityEngine;

/// <summary>
/// A game round manager for the test scene.
/// </summary>

public class TestGameRoundManager : MonoBehaviour
{
    #region Properties and Fields

    [SerializeField] private Character localCharacter;
    [SerializeField] private InGameUIManager uiManager;
    [SerializeField] private CharacterHUDUI characterUI;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CharacterAudioListener characterAudioListenerPrefab;

    #endregion

    #region Methods

    void Start()
    {
        characterUI.Initialize(localCharacter);
        var characterController = localCharacter.gameObject.AddComponent<CharacterController>();
        characterController.Initialize(characterUI);
        cameraController.Initialize(localCharacter);
        var characterAudioListener = Instantiate(characterAudioListenerPrefab, null);
        characterAudioListener.SetTarget(localCharacter.transform);
        localCharacter.InitializeAsLocalCharacter(characterUI, uiManager);
        uiManager.OnRoundStarted();
    }

    #endregion
}
