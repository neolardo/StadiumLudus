using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private Dictionary<BGM, AudioClip> BGMDictionary;

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

    public void PlaySFX(AudioSource source, SFX sfx, float delay = 0)
    {
        source.clip = SFXDictionary[sfx][UnityEngine.Random.Range(0, SFXDictionary[sfx].Count)];
        source.PlayDelayed(delay);
    }

    public void PlayOneShotSFX(AudioSource source, SFX sfx, float delay = 0)
    {
        StartCoroutine(PlayOneShotSFXDelayed(source, sfx, delay));
    }

    private IEnumerator PlayOneShotSFXDelayed(AudioSource source, SFX sfx, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(SFXDictionary[sfx][UnityEngine.Random.Range(0, SFXDictionary[sfx].Count)]);
    }

    public void PlayBGM(AudioSource source, BGM bgm)
    {
        source.clip = BGMDictionary[bgm];
        source.Play();
    }

    #endregion

    #region Stop

    public void Stop(AudioSource source)
    {
        source.Stop();
    }

    #endregion 

    #endregion
}
