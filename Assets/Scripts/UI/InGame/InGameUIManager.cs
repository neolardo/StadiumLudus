using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the navigation between in-game UI pages and handles the common use-cases of them.
/// </summary>
public class InGameUIManager : MonoBehaviour, IDropReceiver, IPointerEnterReceiver
{
    #region Properties and Fields

    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private RectTransform settingsTransform;
    [SerializeField] private CharacterHUDUI characterHUDUI;
    [SerializeField] private BlackScreenUI blackScreenUI;
    [SerializeField] private TutorialPanelUI tutorialPanelUI;
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private EndGameUI endGameUI;
    private SettingsUI settingsUI;

    private bool isNavigating;
    private bool isCurrentPageThePauseMenu = true;

    #endregion

    #region Methods

    #region Initialize

    private void Awake()
    {
        settingsUI = settingsTransform.GetComponent<SettingsUI>();
    }

    #endregion

    #region Update

    private void Update()
    {
        if(GameRoundManager.Instance.RoundStarted && Input.GetKeyDown(KeyCode.Escape) && !endGameUI.gameObject.activeSelf)
        {
            if (characterHUDUI.IsVisible)
            {
                ShowPauseMenu();
            }
            else if (isCurrentPageThePauseMenu)
            {
                ResumeToGame();
            }
            else if(!isCurrentPageThePauseMenu && !isNavigating)
            {
                NavigateToPauseMenu();
            }
        }
    }

    #endregion

    #region Navigation

    public void OnRoundStarted()
    {
        characterHUDUI.Show();
        tutorialPanelUI.ShowIfEnabled();
        blackScreenUI.FadeOutAndDisable();
    }

    public void ResumeToGame()
    {
        pauseMenuUI.Hide();
        characterHUDUI.Show();
        tutorialPanelUI.ShowIfEnabled();
    }
    public void ShowPauseMenu()
    {
        characterHUDUI.Hide();
        tutorialPanelUI.Hide();
        pauseMenuUI.Show();
    }
    public void ShowEndScreen(bool win)
    {
        AudioManager.Instance.PlayOneShotSFX(uiAudioSource, win ? SFX.Win : SFX.Lose);
        characterHUDUI.Hide();
        tutorialPanelUI.Hide();
        endGameUI.SetTitleText(win);
        endGameUI.Show();
    }

    public void FadeInBlackScreen()
    {
        blackScreenUI.EnableAndFadeIn();
    }

    public void NavigateToSettings()
    {
        StartCoroutine(AnimateNavigateToSettings());
    }

    public void NavigateToPauseMenu()
    {
        StartCoroutine(AnimateNavigateToPauseMenu());
    }

    private IEnumerator AnimateNavigateToPauseMenu()
    {
        if (!isCurrentPageThePauseMenu)
        {
            yield return new WaitUntil(() => !isNavigating);
            isNavigating = true;
            AudioManager.Instance.PlayOneShotSFX(uiAudioSource, SFX.MenuProceed);
            float elapsedTime = 0;
            float anchorMaxY = settingsTransform.anchorMax.y;
            float anchorMinY = settingsTransform.anchorMin.y;
            while (elapsedTime < Globals.MenuSlideDuration)
            {
                settingsTransform.anchorMax = new Vector2(Mathf.Lerp(1f, 2f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMaxY);
                settingsTransform.anchorMin = new Vector2(Mathf.Lerp(0f, 1f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMinY);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            settingsTransform.anchorMax = new Vector2(2f, anchorMaxY);
            settingsTransform.anchorMin = new Vector2(1f, anchorMinY);
            isCurrentPageThePauseMenu = true;
            settingsUI.SaveSettings();
            isNavigating = false;
        }
    }

    private IEnumerator AnimateNavigateToSettings()
    {
        if (isCurrentPageThePauseMenu)
        {
            yield return new WaitUntil(() => !isNavigating);
            isNavigating = true;
            AudioManager.Instance.PlayOneShotSFX(uiAudioSource, SFX.MenuProceed);
            float elapsedTime = 0;
            float anchorMaxY = settingsTransform.anchorMax.y;
            float anchorMinY = settingsTransform.anchorMin.y;
            while (elapsedTime < Globals.MenuSlideDuration)
            {
                settingsTransform.anchorMax = new Vector2(Mathf.Lerp(2f, 1f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMaxY);
                settingsTransform.anchorMin = new Vector2(Mathf.Lerp(1f, 0f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMinY);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            settingsTransform.anchorMax = new Vector2(1f, anchorMaxY);
            settingsTransform.anchorMin = new Vector2(0f, anchorMinY);
            isCurrentPageThePauseMenu = false;
            isNavigating = false;
        }
    }

    #endregion

    #region Sounds

    public void OnDrop()
    {
        OnButtonClick();
    }

    public void OnPointerEnter()
    {
        OnButtonHover();
    }

    public void OnButtonHover()
    {
        AudioManager.Instance.PlayOneShotSFX(uiAudioSource, SFX.MenuButtonHover);
    }

    public void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(uiAudioSource, SFX.MenuButtonClick);
    }

    #endregion

    #endregion

}
