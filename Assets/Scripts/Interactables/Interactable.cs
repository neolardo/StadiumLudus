using UnityEngine;
/// <summary>
/// An interface for any object that is interactable for the player.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    /// <summary>
    /// Gets the cloeset interaction point to a relative point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The cloeset interaction point to a relative point./returns>
    public abstract Vector3 GetClosestInteractionPoint(Vector3 point);


    /// <summary>
    /// Tries to interract with an <see cref="Interactable"/> element.
    /// </summary>
    /// <param name="character">The character which tries to interract.</param>
    /// <returns>True, if the interaction was successful, otherwise false.</returns>
    public abstract bool TryInteract(Character character);
}
