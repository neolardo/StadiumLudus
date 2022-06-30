using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the in-game pause menu UI.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private AudioSource pauseMenuAudioSource;
    [SerializeField] private CharacterUI characterUI;
    [SerializeField] private RectTransform settingsTransform;
    private bool IsNavigating { get; set; } = false;

    #endregion

    #region Methods

    #region Resume

    public void OnResume()
    {
        characterUI.ShowHidePauseMenu();
    }

    #endregion

    #region Navigation

    public void OnOpenSettings()
    {
        StartCoroutine(AnimateNavigateToSettings());
    }

    public void NavigateToPauseMenu()
    {
        StartCoroutine(AnimateNavigateToPauseMenu());
    }

    private IEnumerator AnimateNavigateToPauseMenu()
    {
        yield return new WaitUntil(() => !IsNavigating);
        AudioManager.Instance.PlayOneShotSFX(pauseMenuAudioSource, SFX.MenuProceed);
        IsNavigating = true;
        float elapsedTime = MainMenuUI.slideDuration;
        float anchorMaxY = settingsTransform.anchorMax.y;
        float anchorMinY = settingsTransform.anchorMin.y;
        while (elapsedTime > 0)
        {
            settingsTransform.anchorMax = new Vector2(Mathf.Lerp(1f, 2f, Mathf.Sqrt((MainMenuUI.slideDuration - elapsedTime) / MainMenuUI.slideDuration)), anchorMaxY);
            settingsTransform.anchorMin = new Vector2(Mathf.Lerp(0f, 1f, Mathf.Sqrt((MainMenuUI.slideDuration - elapsedTime) / MainMenuUI.slideDuration)), anchorMinY);
            elapsedTime -= Time.deltaTime;
            yield return null;
        }
        settingsTransform.anchorMax = new Vector2(2f, anchorMaxY);
        settingsTransform.anchorMin = new Vector2(1f, anchorMinY);
        IsNavigating = false;
    }
    private IEnumerator AnimateNavigateToSettings()
    {
        yield return new WaitUntil(() => !IsNavigating);
        AudioManager.Instance.PlayOneShotSFX(pauseMenuAudioSource, SFX.MenuProceed);
        IsNavigating = true;
        float elapsedTime = MainMenuUI.slideDuration;
        float anchorMaxY = settingsTransform.anchorMax.y;
        float anchorMinY = settingsTransform.anchorMin.y;
        while (elapsedTime > 0)
        {
            settingsTransform.anchorMax = new Vector2(Mathf.Lerp(2f, 1f, Mathf.Sqrt((MainMenuUI.slideDuration - elapsedTime) / MainMenuUI.slideDuration)), anchorMaxY);
            settingsTransform.anchorMin = new Vector2(Mathf.Lerp(1f, 0f, Mathf.Sqrt((MainMenuUI.slideDuration - elapsedTime) / MainMenuUI.slideDuration)), anchorMinY);
            elapsedTime -= Time.deltaTime;
            yield return null;
        }
        settingsTransform.anchorMax = new Vector2(1f, anchorMaxY);
        settingsTransform.anchorMin = new Vector2(0f, anchorMinY);
        IsNavigating = false;
    }

    #endregion

    #region Exit

    public void OnExit()
    {
        StartCoroutine(LeaveRoomAndLoadMainSceneAfterDelay());
    }

    private IEnumerator LeaveRoomAndLoadMainSceneAfterDelay()
    {
        yield return new WaitForSeconds(Globals.LoadingDelay);
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Button Sounds

    public void OnButtonHover()
    {
        AudioManager.Instance.PlayOneShotSFX(pauseMenuAudioSource, SFX.MenuButtonHover);
    }
    public void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(pauseMenuAudioSource, SFX.MenuButtonClick);
    }

    #endregion

    #endregion
}
