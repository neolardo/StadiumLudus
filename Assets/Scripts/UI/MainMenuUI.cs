using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages the UI of the main menu.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    #region Fields and Properties

    public AudioSource menuAudioSource;
    public AudioMixer mainAudioMixer;
    public RectTransform mainMenuPageRectTransform;
    public RectTransform settingsPageRectTransform;
    public RectTransform roomsPageRectTransform;
    private MainMenuPage currentPage;

    public const float slideDuration = 0.35f;
    private bool IsNavigating { get; set; }


    #endregion

    #region Methods

    private void Start()
    {
        currentPage = MainMenuPage.MainMenu;
    }

    public void OnMenuButtonHover()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuButtonHover);
    }

    public void OnMenuButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuClick);
    }

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
        AudioManager.Instance.PlayOneShotSFX(menuAudioSource, SFX.MenuProceed);
        if(targetPage != currentPage)
        {
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
}
