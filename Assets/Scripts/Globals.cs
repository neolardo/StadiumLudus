using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
/// <summary>
/// A helper static class containing global variables.
/// </summary>
public static class Globals
{
    #region Global Constants

    // tags
    public const string CharacterTag = "Character";
    public const string HitBoxTag = "HitBox";
    public const string IgnoreBoxTag = "IgnoreBox";
    public const string AttackTriggerTag = "AttackTrigger";
    public const string WoodTag = "Wood";
    public const string StoneTag = "Stone";
    public const string MetalTag = "Metal";

    // delta
    public const float CompareDelta = 0.001f;
    public const float PositionThreshold = 0.1f;
    public const float LoadingDelay = 0.2f;
    public const float OutlineDelay = 0.05f;
    public const float RaycastDistance = 30f;

    // layers
    public const int IgnoreRaycastLayer = 2;
    public const int CharacterLayer = 6;
    public const int GroundLayer = 7;
    public const int InteractableLayer = 8;
    public const int HitBoxLayer = 10;
    public const int ValidationLayer = 11;
    public const int AttackTriggerLayer = 12;
    public const int RigidbodyLayer = 13;

    // audio mixer 
    public const string AudioMixerSFXVolume = "SFXVolume";
    public const string AudioMixerMusicVolume = "MusicVolume";
    public const float AudioMixerMaximumDecibel = 0;
    public const float AudioMixerMinimumDecibel = -80;

    //scenes
    public const string MainMenuScene = "MainMenuScene";
    public const string CharacterSelectionScene = "CharacterSelectionScene";
    public const string GameScene = "GameScene";
    public const string TestScene = "TestScene";

    // network
    public const int MaximumPlayerCountPerRoom = 4;
    public const string RoomPasswordCustomPropertyKey = "Password";
    public const string PlayerFightingStyleCustomPropertyKey = "FightingStyle";
    public const string PlayerClassCustomPropertyKey = "Class";
    public const string PlayerIsCharacterConfirmedKey = "IsCharacterConfirmed";
    public const string PlayerIsInitializedKey = "IsInitialized";
    public const string PlayerIsRematchRequestedKey = "IsRematchRequested";

    // transform
    public const char TransformHierarchySeparator = '/';

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generates a list of random indexes inside a given range.
    /// </summary>
    /// <param name="minInclusive">The minimum inclusive index of the range.</param>
    /// <param name="maxExclusive">The maximum exclusive index of the range.</param>
    /// <param name="numberOfIndexes">The number of indexes to generate from the range.</param>
    /// <param name="unique">True if the indexes should be unique, otherwise false.</param>
    /// <returns>The list of random indexes inside the given range.</returns>
    public static List<int> GenerateRandomIndexes(int minInclusive, int maxExclusive, int numberOfIndexes, bool unique = true)
    {
        var indexList = new List<int>();
        for (int i = minInclusive; i < maxExclusive; i++)
        {
            indexList.Add(i);
        }
        var resultList = new List<int>();
        int count = numberOfIndexes;
        if (unique)
        {
            count = Mathf.Min(numberOfIndexes, indexList.Count);
        }
        for (int i = 0; i < count; i++)
        {
            int ind = Random.Range(0, indexList.Count);
            resultList.Add(indexList[ind]);
            if (unique)
            {
                indexList.RemoveAt(ind);
            }
        }
        return resultList;
    }

    #region Hashtable

    public static Hashtable SetHash(Hashtable hashtable, string key, string value)
    {
        if (hashtable == null)
        {
            hashtable = new Hashtable();
        }
        if (hashtable.ContainsKey(key))
        {
            hashtable[key] = value;
        }
        else
        {
            hashtable.Add(key, value);
        }
        return hashtable;
    }

    public static Hashtable SetHash(Hashtable hashtable, string key, int value)
    {
        if (hashtable == null)
        {
            hashtable = new Hashtable();
        }
        if (hashtable.ContainsKey(key))
        {
            hashtable[key] = value;
        }
        else
        {
            hashtable.Add(key, value);
        }
        return hashtable;
    }

    public static Hashtable SetHash(Hashtable hashtable, string key, bool value)
    {
        if (hashtable == null)
        {
            hashtable = new Hashtable();
        }
        if (hashtable.ContainsKey(key))
        {
            hashtable[key] = value;
        }
        else
        {
            hashtable.Add(key, value);
        }
        return hashtable;
    }

    #endregion

    #endregion
}

