using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the UI of the main menu.
/// </summary>
public class MainMenuUI : MonoBehaviour, IDropReceiver, IPointerEnterReceiver
{
    #region Fields and Properties

    public AudioSource menuAudioSource;
    public AudioMixer mainAudioMixer;
    public RectTransform mainMenuPageRectTransform;
    public RectTransform settingsPageRectTransform;
    public RectTransform roomsPageRectTransform;
    public CanvasGroup titleCanvasGroup;
    public TextMeshProUGUI loadingText;
    public CanvasGroup buttonContainerCanvasGroup;
    public CanvasGroup exitButtonCanvasGroup;
    private MainMenuPage currentPage;

    public const float slideDuration = 0.35f;
    private const float fadeDuration = 0.8f;
    private bool IsNavigating { get; set; }
    private bool IsFading { get; set; }

    public const string NetworkErrorMessage = "Network error.\nPlease check your internet connection and try again.";

    #endregion

    #region Methods

    private void Start()
    {
        currentPage = MainMenuPage.MainMenu;
        NetworkLauncher.Instance.Connected += OnLoaded;
        NetworkLauncher.Instance.Disconnected += OnConnectionFailed;
        if (PhotonNetwork.InLobby)
        {
            InitializeAsLoaded();
        }
        else if (NetworkLauncher.Instance.IsDisconnected)
        {
            SetLoadingText(NetworkErrorMessage);
            StartCoroutine(FadeInTitleAndLoadingText());
            StartCoroutine(FadeInExitButton());
        }
        else
        {
            StartCoroutine(FadeInTitleAndLoadingText());
        }
        AudioManager.Instance.PlayBGM(BGM.Menu);
    }

    #region Loading

    private void InitializeAsLoaded()
    {
        titleCanvasGroup.alpha = 1;
        var loadingCanvasGroup = loadingText.GetComponent<CanvasGroup>();
        loadingCanvasGroup.alpha = 0;
        loadingText.gameObject.SetActive(false);
        buttonContainerCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeInTitleAndLoadingText()
    {
        yield return new WaitUntil(() => !IsFading);
        IsFading = true;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            titleCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        titleCanvasGroup.alpha = 1;
        elapsedTime = 0;
        var loadingCanvasGroup = loadingText.GetComponent<CanvasGroup>();
        while (elapsedTime < fadeDuration)
        {
            loadingCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        loadingCanvasGroup.alpha = 1;
        IsFading = false;
    }

    private void SetLoadingText(string text)
    {
        loadingText.text = text;
    }

    public void OnLoaded()
    {
        StartCoroutine(FadeOutLoadingTextAndFadeInButtons());  
    }

    public void OnConnectionFailed(string errorMessage)
    {
        SetLoadingText(errorMessage);
        var loadingCanvasGroup = loadingText.GetComponent<CanvasGroup>();
        loadingCanvasGroup.alpha = 1;
        loadingCanvasGroup.gameObject.SetActive(true);
        buttonContainerCanvasGroup.alpha = 0;
        if (currentPage != MainMenuPage.MainMenu)
        {
            StartCoroutine(NavigateTo(MainMenuPage.MainMenu));
        }
        StartCoroutine(FadeInExitButton());
        Debug.Log("Disconnected.");
    }

    private IEnumerator FadeInExitButton()
    {
        yield return new WaitUntil(() => !IsFading);
        IsFading = true;
        exitButtonCanvasGroup.gameObject.SetActive(true);
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            exitButtonCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        exitButtonCanvasGroup.alpha = 1;
        IsFading = false;
    }

    private IEnumerator FadeOutLoadingTextAndFadeInButtons()
    {
        yield return new WaitUntil(() => !IsFading);
        IsFading = true;
        float elapsedTime = 0;
        var loadingCanvasGroup = loadingText.GetComponent<CanvasGroup>();
        while (elapsedTime < fadeDuration)
        {
            loadingCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        loadingCanvasGroup.alpha = 0;
        loadingText.gameObject.SetActive(false);
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            buttonContainerCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        buttonContainerCanvasGroup.alpha = 1;
        IsFading = false;
    }

    #endregion

    #region Button Sounds

    public void OnDrop()
    {
        OnMenuButtonClick();
    }

    public void OnPointerEnter()
    {
        OnMenuButtonHover();
    }
    public void OnMenuButtonHover()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuButtonHover);
    }

    public void OnMenuButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuButtonClick);
    }
    public void OnButtonClickAlt()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuButtonClick);
    }

    public void OnBackButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuBack);
    }

    #endregion

    #region Navigation

    public void NavigateToMainMenuPage()
    {
        StartCoroutine(NavigateTo(MainMenuPage.MainMenu));
    }

    public void NavigateToRoomsPage()
    {
        StartCoroutine(NavigateTo(MainMenuPage.Rooms));
    }

    public void NavigateToSettingsPage()
    {
        StartCoroutine(NavigateTo(MainMenuPage.Settings));
    }

    public void Exit()
    {
        Application.Quit();
    }

    private RectTransform RectTransformOf( MainMenuPage page)
    {
        RectTransform rectTransform = null;
        switch (page)
        { 
            case MainMenuPage.MainMenu:
                rectTransform = mainMenuPageRectTransform;
                break;

            case MainMenuPage.Rooms:
                rectTransform = roomsPageRectTransform;
                break;

            case MainMenuPage.Settings:
                rectTransform = settingsPageRectTransform;
                break;

            default:
                Debug.LogWarning("Invalid MainMenuPage.");
                break;
        }
        return rectTransform;
    }

    private IEnumerator NavigateTo(MainMenuPage targetPage)
    {
        yield return new WaitUntil(() => !IsNavigating);
        if(targetPage != currentPage)
        {
            AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuProceed);
            IsNavigating = true;
            var currentPageRect = RectTransformOf(currentPage); 
            var targetPageRect = RectTransformOf(targetPage);
            if (targetPage == MainMenuPage.MainMenu)
            {
                float elapsedTime = slideDuration;
                float anchorMaxY = currentPageRect.anchorMax.y;
                float anchorMinY = currentPageRect.anchorMin.y;
                while (elapsedTime > 0)
                {
                    currentPageRect.anchorMax = new Vector2(Mathf.Lerp(1f, 2f, Mathf.Sqrt((slideDuration - elapsedTime) / slideDuration)), anchorMaxY);
                    currentPageRect.anchorMin = new Vector2(Mathf.Lerp(0f, 1f, Mathf.Sqrt((slideDuration - elapsedTime) / slideDuration)), anchorMinY);
                    elapsedTime -= Time.deltaTime;
                    yield return null;
                }
                currentPageRect.anchorMax = new Vector2(2f, anchorMaxY);
                currentPageRect.anchorMin = new Vector2(1f, anchorMinY);
                currentPage = targetPage;
            }
            else
            {
                float elapsedTime = slideDuration;
                float anchorMaxY = targetPageRect.anchorMax.y;
                float anchorMinY = targetPageRect.anchorMin.y;
                while (elapsedTime > 0)
                {
                    targetPageRect.anchorMax = new Vector2(Mathf.Lerp(2f, 1f, Mathf.Sqrt((slideDuration - elapsedTime) / slideDuration)), anchorMaxY);
                    targetPageRect.anchorMin = new Vector2(Mathf.Lerp(1f, 0f, Mathf.Sqrt((slideDuration - elapsedTime) / slideDuration)), anchorMinY);
                    elapsedTime -= Time.deltaTime;
                    yield return null;
                }
                targetPageRect.anchorMax = new Vector2(1f, anchorMaxY);
                targetPageRect.anchorMin = new Vector2(0f, anchorMinY);
                currentPage = targetPage;
            }
            IsNavigating = false;
        }
    }

    #endregion

    #region Destroy

    private void OnDestroy()
    {
        if (NetworkLauncher.Instance != null)
        {
            NetworkLauncher.Instance.Connected -= OnLoaded;
            NetworkLauncher.Instance.Disconnected -= OnConnectionFailed;
        }
    }

    #endregion

    #endregion
}
