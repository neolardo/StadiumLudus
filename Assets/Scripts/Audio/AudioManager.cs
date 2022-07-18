using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// A manager for playing audio sound effects and background music.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Properties and Fields

    private const string AudioResourcesFolderPath = "Audio";
    private static readonly string BGMFolderPath = Path.Combine(AudioResourcesFolderPath, "BGM");
    private static readonly string SFXFolderPath = Path.Combine(AudioResourcesFolderPath, "SFX");
    private const char SeparatorCharacter = '_';

    public static AudioManager Instance { get; private set; }
    private Dictionary<SFX, List<AudioClip>> SFXDictionary;
    private Dictionary<SFX, int> LastSFXIndexDictionary;
    private Dictionary<BGM, AudioClip> BGMDictionary;
    public AudioSource MusicAudioSource { get; private set; }
    private const float MusicFadeOutDuration = 2.5f;

    #endregion

    #region Methods

    #region Init

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            LoadResources();
            MusicAudioSource = GetComponent<AudioSource>();
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private void LoadResources()
    {
        ParseSFXs();
        ParseBGMs();
    }

    private void ParseSFXs()
    {
        SFXDictionary = new Dictionary<SFX, List<AudioClip>>();
        LastSFXIndexDictionary = new Dictionary<SFX, int>();
        AudioClip[] clipArray = Resources.LoadAll<AudioClip>(SFXFolderPath);
        foreach (var clip in clipArray)
        {
            var fileNameRoot = clip.name.Split(SeparatorCharacter)[0];
            if (Enum.TryParse(fileNameRoot, out SFX sfxEnum))
            {
                if (!SFXDictionary.ContainsKey(sfxEnum))
                {
                    SFXDictionary.Add(sfxEnum, new List<AudioClip>());
                }
                SFXDictionary[sfxEnum].Add(clip);
            }
            else
            {
                Debug.LogWarning("Could not load audio resource: " + clip.name);
            }
        }
        var sfxValues = Enum.GetValues(typeof(SFX)).Cast<SFX>().ToList();
        foreach (var sfx in sfxValues)
        {
            LastSFXIndexDictionary.Add(sfx, -1);
        }
    }

    private void ParseBGMs()
    {
        BGMDictionary = new Dictionary<BGM, AudioClip>();
        AudioClip[] clipArray = Resources.LoadAll<AudioClip>(BGMFolderPath);
        foreach (var clip in clipArray)
        {
            if (Enum.TryParse(clip.name, out BGM bgmEnum))
            {
                BGMDictionary.Add(bgmEnum, clip);
            }
        }
    }

    #endregion

    #region Play

    public void PlaySFX(AudioSource source, SFX sfx, float delay = 0, bool doNotRepeat = false)
    {
        int index = UnityEngine.Random.Range(0, SFXDictionary[sfx].Count);
        while (doNotRepeat && SFXDictionary[sfx].Count > 1 && index == LastSFXIndexDictionary[sfx])
        {
            index = UnityEngine.Random.Range(0, SFXDictionary[sfx].Count);
        }
        LastSFXIndexDictionary[sfx] = index;
        source.clip = SFXDictionary[sfx][index];
        source.PlayDelayed(delay);
    }

    public void PlayOneShotSFX(AudioSource source, SFX sfx, float delay = 0, bool doNotRepeat = false)
    {
        StartCoroutine(PlayOneShotSFXDelayed(source, sfx, delay, doNotRepeat));
    }

    private IEnumerator PlayOneShotSFXDelayed(AudioSource source, SFX sfx, float delay = 0, bool doNotRepeat = false)
    {
        yield return new WaitForSeconds(delay);
        int index = UnityEngine.Random.Range(0, SFXDictionary[sfx].Count);
        while (doNotRepeat && SFXDictionary[sfx].Count > 1 && index == LastSFXIndexDictionary[sfx])
        {
            index = UnityEngine.Random.Range(0, SFXDictionary[sfx].Count);
        }
        LastSFXIndexDictionary[sfx] = index;
        source.PlayOneShot(SFXDictionary[sfx][index]);
    }

    public void PlayBGM(BGM bgm)
    {
        StartCoroutine(FadeOutOldBGMThenPlayNewBGM(bgm));
    }

    private IEnumerator FadeOutOldBGMThenPlayNewBGM(BGM bgm)
    {
        if (MusicAudioSource.clip != BGMDictionary[bgm])
        {
            if (MusicAudioSource.isPlaying)
            {
                yield return ManageFadeOut(MusicAudioSource, MusicFadeOutDuration);
            }
            MusicAudioSource.clip = BGMDictionary[bgm];
            MusicAudioSource.loop = true;
            MusicAudioSource.Play();
        }
    }

    #endregion

    #region Stop and Fade Out

    public void Stop(AudioSource source)
    {
        source.Stop();
    }

    public void FadeOut(AudioSource source, float fadeDuration)
    {
        StartCoroutine(ManageFadeOut(source, fadeDuration));
    }

    private IEnumerator ManageFadeOut(AudioSource source, float fadeDuration)
    {
        float elapsedSeconds = 0;
        while (elapsedSeconds < fadeDuration)
        {
            source.volume = Mathf.Lerp(1, 0, elapsedSeconds / fadeDuration);
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        source.volume = 0;
        source.Stop();
        source.volume = 1;
    }


    #endregion 

    #endregion
}
