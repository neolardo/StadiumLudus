using UnityEngine;

/// <summary>
/// Represents the settings data of the application which is stored and retrieved from the <see cref="PlayerPrefs"/>.
/// </summary>
public class SettingsData
{
    #region Fields and Properties

    public float musicVolume;
    public float soundVolume;
    public bool showTutorialOverlay;

    private const string musicVolumeKey = "MusicVolume";
    private const string soundVolumeKey = "SoundVolume";
    private const string showTutorialOverlayKey = "ShowTutorialOverlay";

    private const float defaultMusicVolume = 1;
    private const float defaultSoundVolume = 1;
    private const bool defaultShowTutorialOverlay = true;

    private bool CanLoadSettings => PlayerPrefs.HasKey(musicVolumeKey) && PlayerPrefs.HasKey(soundVolumeKey) && PlayerPrefs.HasKey(showTutorialOverlayKey);

    #endregion

    #region Methods

    private void Load()
    {
        musicVolume = PlayerPrefs.GetFloat(musicVolumeKey);
        soundVolume = PlayerPrefs.GetFloat(soundVolumeKey);
        showTutorialOverlay = PlayerPrefs.GetInt(showTutorialOverlayKey) == 1;
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(musicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(soundVolumeKey, soundVolume);
        PlayerPrefs.SetInt(showTutorialOverlayKey, showTutorialOverlay ? 1 : 0);
        Debug.Log("Settings saved.");
    }

    public void Reset()
    {
        musicVolume = defaultMusicVolume;
        soundVolume = defaultSoundVolume;
        showTutorialOverlay = defaultShowTutorialOverlay;
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new settings data by retrieving it from the <see cref="PlayerPrefs"/> if possible, otherwise creates a new instance with default values.
    /// </summary>
    public SettingsData()
    {
        if (CanLoadSettings)
        {
            Load();
        }
        else
        {
            Reset();
            Save();
        }
    }

    #endregion
}
