using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the in-game pause menu UI.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private InGameUIManager uiManager;

    #endregion

    #region Methods

    #region Show Hide

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region Handle Button Clicks

    public void OnResumeClicked()
    {
        uiManager.ResumeToGame();
    }

    public void OnSettingsClicked()
    {
        uiManager.NavigateToSettings();
    }

    public void OnExitClicked()
    {
        StartCoroutine(LeaveRoomAfterDelay());
    }

    #endregion

    #region Exit

    private IEnumerator LeaveRoomAfterDelay()
    {
        yield return new WaitForSeconds(Globals.LoadingDelay);
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #endregion
}
