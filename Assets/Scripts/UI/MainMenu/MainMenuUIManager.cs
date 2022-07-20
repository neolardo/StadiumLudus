using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the navigation between <see cref="MainMenuPage"/>s and handles the common use-cases of them.
/// </summary>
public class MainMenuUIManager : MonoBehaviour, IDropReceiver, IPointerEnterReceiver
{
    #region Properties and Fields

    public MainMenuPage CurrentPage { get; private set; } = MainMenuPage.MainMenu;

    [SerializeField] private AudioSource menuAudioSource;
    [SerializeField] private RectTransform mainMenuPageRectTransform;
    [SerializeField] private RectTransform settingsPageRectTransform;
    [SerializeField] private RectTransform roomsPageRectTransform;
    private SettingsUI settingsUI;
    private bool isNavigating;

    #endregion

    #region Methods

    #region Initialize

    private void Awake()
    {
        settingsUI = settingsPageRectTransform.GetComponent<SettingsUI>();
    }

    #endregion

    #region Update

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isNavigating)
        {
            NavigateTo(MainMenuPage.MainMenu);
        }
    }

    #endregion

    #region Navigation

    public void NavigateToMainMenu()
    {
        NavigateTo(MainMenuPage.MainMenu);
    }

    public void NavigateTo(MainMenuPage targetPage)
    {
        StartCoroutine(AnimateNavigationTo(targetPage));
    }

    private IEnumerator AnimateNavigationTo(MainMenuPage targetPage)
    {
        yield return new WaitUntil(() => !isNavigating);
        if (targetPage != CurrentPage)
        {
            if (CurrentPage == MainMenuPage.Settings)
            {
                settingsUI.SaveSettings();
            }
            AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuProceed);
            isNavigating = true;
            var currentPageRect = RectTransformOf(CurrentPage);
            var targetPageRect = RectTransformOf(targetPage);
            if (targetPage == MainMenuPage.MainMenu)
            {
                float elapsedTime = 0;
                float anchorMaxY = currentPageRect.anchorMax.y;
                float anchorMinY = currentPageRect.anchorMin.y;
                while (elapsedTime < Globals.MenuSlideDuration)
                {
                    currentPageRect.anchorMax = new Vector2(Mathf.Lerp(1f, 2f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMaxY);
                    currentPageRect.anchorMin = new Vector2(Mathf.Lerp(0f, 1f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMinY);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                currentPageRect.anchorMax = new Vector2(2f, anchorMaxY);
                currentPageRect.anchorMin = new Vector2(1f, anchorMinY);
                CurrentPage = targetPage;
            }
            else
            {
                float elapsedTime = 0;
                float anchorMaxY = targetPageRect.anchorMax.y;
                float anchorMinY = targetPageRect.anchorMin.y;
                while (elapsedTime < Globals.MenuSlideDuration)
                {
                    targetPageRect.anchorMax = new Vector2(Mathf.Lerp(2f, 1f, Mathf.Sqrt(elapsedTime / Globals.MenuSlideDuration)), anchorMaxY);
                    targetPageRect.anchorMin = new Vector2(Mathf.Lerp(1f, 0f, Mathf.Sqrt(elapsedTime  / Globals.MenuSlideDuration)), anchorMinY);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                targetPageRect.anchorMax = new Vector2(1f, anchorMaxY);
                targetPageRect.anchorMin = new Vector2(0f, anchorMinY);
                CurrentPage = targetPage;
            }
            isNavigating = false;
        }
    }

    private RectTransform RectTransformOf(MainMenuPage page)
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
                Debug.LogWarning($"Invalid {nameof(MainMenuPage)}.");
                break;
        }
        return rectTransform;
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
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuButtonHover);
    }

    public void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuButtonClick);
    }

    public void OnBackButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuBack);
    }

    #endregion

    #endregion
}
