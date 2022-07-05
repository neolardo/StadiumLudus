using UnityEngine;

/// <summary>
/// A game round manager for the test scene.
/// </summary>

public class TestGameRoundManager : MonoBehaviour
{
    [SerializeField] private Character localCharacter;
    [SerializeField] private CharacterUI characterUI;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CharacterAudioListener characterAudioListenerPrefab;


    void Start()
    {
        characterUI.Initialize(localCharacter);
        var characterController = localCharacter.GetComponent<CharacterController>();
        characterController.Initialize(characterUI);
        cameraController.Initialize(localCharacter);
        var characterAudioListener = Instantiate(characterAudioListenerPrefab, null);
        characterAudioListener.SetTarget(localCharacter.transform);
        localCharacter.InitializeAsLocalCharacter(characterUI);
        characterUI.SetUIVisiblity(true);
    }
}
