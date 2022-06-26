using UnityEngine;

/// <summary>
/// Represents a storage of hitbox infos.
/// </summary>
public class HitBoxInfoStorage 
{
    #region Properties and Fields
    public (Vector3 position, Quaternion rotation)[] ColliderTransformArray { get; }
    public bool CanBeInterrupted { get; set; }

    public static int MaximumHitBoxCount = 100;

    #endregion

    #region Constructor

    public HitBoxInfoStorage()
    {
        ColliderTransformArray = new (Vector3, Quaternion)[MaximumHitBoxCount];
    }

    #endregion

}
