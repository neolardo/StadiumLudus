using UnityEngine;

/// <summary>
/// Represents a hitbox info.
/// </summary>
public struct HitBoxInfo
{
    #region Properties and Fields

    public readonly Vector3 colliderPosition;
    public readonly Quaternion colliderRotation;
    public readonly bool canBeInterrupted;

    #endregion

    #region Constructor

    public HitBoxInfo(Vector3 colliderPosition, Quaternion colliderRotation, bool canBeInterrupted)
    {
        this.colliderPosition = colliderPosition;
        this.colliderRotation = colliderRotation;
        this.canBeInterrupted = canBeInterrupted;
    }

    #endregion
}
