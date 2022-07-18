using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Manages the setting page.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    #region Properties and Fields

    public AudioMixer mainAudioMixer;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;
    public CheckBox fullscreenCheckBox;
    public CheckBox tutorialOverlayCheckBox;

    private SerializableSettings settings;

    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
        LoadSettings();
        musicVolumeSlider.value = settings.musicVolume;
        soundVolumeSlider.value = settings.musicVolume;
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

    #region Serialization

    private void LoadSettings()
    {
        if (File.Exists(Globals.SettingsDataPath))
        {
            string jsonString = File.ReadAllText(Globals.SettingsDataPath);
            settings = JsonUtility.FromJson<SerializableSettings>(jsonString);
        }
        else
        {
            settings = new SerializableSettings();
            SaveSettings();
        }
    }

    public void SaveSettings()
    {
        string jsonString = JsonUtility.ToJson(settings, true);
        File.WriteAllText(Globals.SettingsDataPath, jsonString);
    }

    #endregion

    #region OnValueChanged

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

    #region Restore Defaults
    public void RestoreDefaults()
    {
        musicVolumeSlider.value = SerializableSettings.defaultMusicVolume;
        soundVolumeSlider.value = SerializableSettings.defaultSoundVolume;
        tutorialOverlayCheckBox.SetIsTicked(SerializableSettings.defaultShowTutorialOverlay);
    }

    #endregion

    #endregion
}
