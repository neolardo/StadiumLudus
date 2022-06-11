using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the setting page.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    #region Properties and Fields

    public MainMenuUI mainMenuUI;

    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;

    private float musicVolumeDefaultValue = 1;
    private float soundVolumeDefaultValue = 1;

    #endregion

    #region Methods

    public void RestoreDefaults()
    {
        musicVolumeSlider.value = musicVolumeDefaultValue;
        soundVolumeSlider.value = soundVolumeDefaultValue;
    }
    public void BackToMainMenu()
    {
        mainMenuUI.NavigateToMainMenuPage();
    }

    #endregion
}
