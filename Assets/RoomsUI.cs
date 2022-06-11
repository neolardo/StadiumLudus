using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the rooms UI screen.
/// </summary>
public class RoomsUI : MonoBehaviour
{
    #region Properties and Fields

    public MainMenuUI mainMenuUI;
    public RectTransform roomButtonContainer;
    public GameObject roomButtonPrefab;
    public GameObject createRoomPopUp;

    #endregion

    #region Methods

    private void Start()
    {
        for (int i = 0; i < 30; i++)
        {
            Instantiate(roomButtonPrefab, roomButtonContainer);
        }
    }

    public void CreateRoom()
    {
        createRoomPopUp.SetActive(true);
    }

    public void JoinRoom()
    {
        Debug.Log("JoinRoom");
    }

    public void BackToMainMenu()
    {
        mainMenuUI.NavigateToMainMenuPage();
    }

    #endregion
}
