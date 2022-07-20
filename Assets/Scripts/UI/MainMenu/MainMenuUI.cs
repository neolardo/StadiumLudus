using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the UI of the main menu.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private MainMenuUIManager menuManager;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private CanvasGroup buttonContainerCanvasGroup;
    [SerializeField] private CanvasGroup exitButtonCanvasGroup;
    [SerializeField] private CanvasGroup versionTextCanvasGroup;
    [SerializeField] private GameObject loadingPopup;
    private bool isFading;

    private const float fadeDuration = 0.8f;
    public const string NetworkErrorMessage = "Network error.\nPlease check your internet connection and try again.";

    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
        versionText.text = $"{Application.version}";
        NetworkLauncher.Instance.Connected += OnLoaded;
        NetworkLauncher.Instance.Disconnected += OnConnectionFailed;
        NetworkLauncher.Instance.CreateRoomFailed += OnCreateRoomFailed;
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

    #endregion

    #region Loading

    private void InitializeAsLoaded()
    {
        titleCanvasGroup.alpha = 1;
        var loadingCanvasGroup = loadingText.GetComponent<CanvasGroup>();
        loadingCanvasGroup.alpha = 0;
        loadingText.gameObject.SetActive(false);
        buttonContainerCanvasGroup.alpha = 1;
        versionTextCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeInTitleAndLoadingText()
    {
        yield return new WaitUntil(() => !isFading);
        isFading = true;
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
            versionTextCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        loadingCanvasGroup.alpha = 1;
        versionTextCanvasGroup.alpha = 1;
        isFading = false;
    }

    private void SetLoadingText(string text)
    {
        loadingText.text = text;
    }

    public void OnLoaded()
    {
        StartCoroutine(FadeOutLoadingTextAndFadeInButtons());
    }

    public void OnCreateRoomFailed()
    {
        loadingPopup.SetActive(false);
    }

    public void OnConnectionFailed(string errorMessage)
    {
        SetLoadingText(errorMessage);
        var loadingCanvasGroup = loadingText.GetComponent<CanvasGroup>();
        loadingCanvasGroup.alpha = 1;
        loadingCanvasGroup.gameObject.SetActive(true);
        buttonContainerCanvasGroup.alpha = 0;
        if (menuManager.CurrentPage != MainMenuPage.MainMenu)
        {
            menuManager.NavigateTo(MainMenuPage.MainMenu);
        }
        StartCoroutine(FadeInExitButton());
        Debug.Log("Disconnected.");
    }

    private IEnumerator FadeInExitButton()
    {
        yield return new WaitUntil(() => !isFading);
        isFading = true;
        exitButtonCanvasGroup.gameObject.SetActive(true);
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            exitButtonCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        exitButtonCanvasGroup.alpha = 1;
        isFading = false;
    }

    private IEnumerator FadeOutLoadingTextAndFadeInButtons()
    {
        yield return new WaitUntil(() => !isFading);
        isFading = true;
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
        isFading = false;
    }

    #endregion

    #region Handle Menu Button Clicks

    public void CreatePracticeRoom()
    {
        loadingPopup.SetActive(true);
        NetworkLauncher.Instance.CreatePracticeRoom();
    }

    public void NavigateToRoomsPage()
    {
        menuManager.NavigateTo(MainMenuPage.Rooms);
    }

    public void NavigateToSettingsPage()
    {
        menuManager.NavigateTo(MainMenuPage.Settings);
    }

    public void Exit()
    {
        Application.Quit();
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
