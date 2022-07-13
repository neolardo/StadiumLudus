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
    public const float HighlightDelay = 0.05f;
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

    #region Range

    /// <summary>
    /// Clamps a point inside a given distance from the <see cref="Character"/>.
    /// </summary>
    /// <param name="origin">The origin point.</param>
    /// <param name="target">The target point.</param>
    /// <param name="range">The maximum range in the target point's direction.</param>
    /// <param name="forceOverwrite">True if the given target should be recalculated by raycasting.</param>
    /// <returns>The target point closer than the given range.</returns>
    public static Vector3 ClampPointInsideRange(Vector3 origin, Vector3 target, float range, bool forceOverwrite = false)
    {
        if ((target - origin).magnitude > range)
        {
            var edgePoint = origin + (target - origin).normalized * range;
            var raycastPoint = edgePoint + Vector3.up * 5;
            Ray ray = new Ray(raycastPoint, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10, 1 << Globals.GroundLayer))
            {
                edgePoint = hit.point;
            }
            target = edgePoint;
        }
        else if (forceOverwrite)
        {
            Ray ray = new Ray(target + Vector3.up, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10, 1 << Globals.GroundLayer))
            {
                target = hit.point;
            }
        }
        return target;
    }

    /// <summary>
    /// Gets a point at a given distance from the <see cref="Character"/>.
    /// </summary>
    /// <param name="target">A point towards the direction.</param>
    /// <param name="range">The maximum range in the target point's direction.</param>
    /// <returns>The target point closer than the given range.</returns>
    public static Vector3 GetPointAtRange(Vector3 origin, Vector3 target, float range, bool forceOverwrite = false)
    {
        var edgePoint = origin + (target - origin).normalized * range;
        var raycastPoint = edgePoint + Vector3.up * 5;
        Ray ray = new Ray(raycastPoint, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10, 1 << Globals.GroundLayer))
        {
            edgePoint = hit.point;
        }
        return edgePoint;
    }

    #endregion

    #region Random Index

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

    #endregion

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

