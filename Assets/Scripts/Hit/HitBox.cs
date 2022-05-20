using UnityEngine;

/// <summary>
/// A helper class to connect a <see cref="Character"/> with it's hit box.
/// </summary>
public class HitBox : MonoBehaviour
{
    [Tooltip("The character which contains this hitbox.")]
    public Character character;
}
