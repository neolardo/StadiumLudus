using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameRoundManager : MonoBehaviour
{
    [SerializeField] private Character localCharacter;
    [SerializeField] private CharacterUI characterUI;
    [SerializeField] private CameraController cameraController;


    void Start()
    {
        localCharacter.InitializeAsLocalCharacter(characterUI);
        characterUI.Initialize(localCharacter);
        var characterController = localCharacter.GetComponent<CharacterController>();
        characterController.Initialize(characterUI);
        cameraController.Initialize(localCharacter);
    }

}
