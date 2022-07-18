using System;

/// <summary>
/// Represents the serializable settings of the application.
/// </summary>
[Serializable]
public class SerializableSettings
{
    public float musicVolume;
    public float soundVolume;
    public bool showTutorialOverlay;

    public const float defaultMusicVolume = 1;
    public const float defaultSoundVolume = 1;
    public const bool defaultShowTutorialOverlay = true;

    public SerializableSettings()
    {
        musicVolume = defaultMusicVolume;
        soundVolume = defaultSoundVolume;
        showTutorialOverlay = defaultShowTutorialOverlay;
    }
}
