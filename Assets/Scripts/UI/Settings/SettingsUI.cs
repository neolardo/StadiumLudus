using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Manages the setting page.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    #region Properties and Fields

    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private CheckBox fullscreenCheckBox;
    [SerializeField] private CheckBox tutorialOverlayCheckBox;

    private SettingsData settings;

    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
        settings = new SettingsData();
        musicVolumeSlider.value = settings.musicVolume;
        soundVolumeSlider.value = settings.soundVolume;
        tutorialOverlayCheckBox.SetIsTicked(settings.showTutorialOverlay);
        tutorialOverlayCheckBox.IsTickedChanged += OnTutorialOverlayCheckBoxChanged;
        fullscreenCheckBox.SetIsTicked(Screen.fullScreen);
        fullscreenCheckBox.IsTickedChanged += OnFullScreenCheckBoxChanged;
    }

    private void OnDestroy()
    {
        if (tutorialOverlayCheckBox != null)
        {
            tutorialOverlayCheckBox.IsTickedChanged -= OnTutorialOverlayCheckBoxChanged;
        }
        if (fullscreenCheckBox != null)
        {
            fullscreenCheckBox.IsTickedChanged -= OnFullScreenCheckBoxChanged;
        }
    }

    #endregion

    #region Save

    public void SaveSettings()
    {
        settings.Save();
    }

    #endregion

    #region Handle Value Changes

    public void OnMusicVolumeSliderChanged(float value)
    {
        mainAudioMixer.SetFloat(Globals.AudioMixerMusicVolume, Mathf.Lerp(Globals.AudioMixerMinimumDecibel, Globals.AudioMixerMaximumDecibel, Mathf.Pow(value, 0.25f)));
        settings.musicVolume = value;
    }
    public void OnSFXVolumeSliderChanged(float value)
    {
        mainAudioMixer.SetFloat(Globals.AudioMixerSFXVolume, Mathf.Lerp(Globals.AudioMixerMinimumDecibel, Globals.AudioMixerMaximumDecibel, Mathf.Pow(value, 0.25f)));
        settings.soundVolume = value;
    }

    private void OnFullScreenCheckBoxChanged()
    {
        Screen.fullScreen = fullscreenCheckBox.IsTicked;
    }
    private void OnTutorialOverlayCheckBoxChanged()
    {
        settings.showTutorialOverlay = tutorialOverlayCheckBox.IsTicked;
    }

    #endregion

    #region Handle Button Clicks

    public void RestoreDefaults()
    {
        settings.Reset();
        musicVolumeSlider.value = settings.musicVolume;
        soundVolumeSlider.value = settings.soundVolume;
        tutorialOverlayCheckBox.SetIsTicked(settings.showTutorialOverlay);
    }

    #endregion

    #endregion
}
