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

    private float musicVolumeDefaultValue = 1;
    private float soundVolumeDefaultValue = 1;

    #endregion

    #region Methods

    private void Start()
    {
        float value = 0;
        mainAudioMixer.GetFloat(Globals.AudioMixerMusicVolume, out value);
        musicVolumeSlider.value = Mathf.Pow(Mathf.InverseLerp(Globals.AudioMixerMinimumDecibel, Globals.AudioMixerMaximumDecibel, value), 4);
        mainAudioMixer.GetFloat(Globals.AudioMixerSFXVolume, out value);
        soundVolumeSlider.value = Mathf.Pow(Mathf.InverseLerp(Globals.AudioMixerMinimumDecibel, Globals.AudioMixerMaximumDecibel, value), 4);
    }

    public void RestoreDefaults()
    {
        musicVolumeSlider.value = musicVolumeDefaultValue;
        soundVolumeSlider.value = soundVolumeDefaultValue;
    }

    public void OnMusicVolumeSliderChanged(float value)
    {
        mainAudioMixer.SetFloat(Globals.AudioMixerMusicVolume, Mathf.Lerp(Globals.AudioMixerMinimumDecibel, Globals.AudioMixerMaximumDecibel, Mathf.Pow(value, 0.25f)));
    }
    public void OnSFXVolumeSliderChanged(float value)
    {
        mainAudioMixer.SetFloat(Globals.AudioMixerSFXVolume, Mathf.Lerp(Globals.AudioMixerMinimumDecibel, Globals.AudioMixerMaximumDecibel, Mathf.Pow(value, 0.25f)));
    }

    #endregion
}
