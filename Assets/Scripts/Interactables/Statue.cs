using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Manages an interactable statue.
/// </summary>
public class Statue : Interactable
{
    #region Properties and Fields

    [Tooltip("The distance of the range where a player can interact with the statue.")]
    [SerializeField]
    private float interactionRangeDistance = 1f;

    private bool IsNextBuffAvailable;

    [SerializeField]
    private Buff buff;

    #endregion

    #region Methods

    private void Start()
    {
        SpawnBuff();
    }

    private void SpawnBuff()
    {
        IsNextBuffAvailable = true;
        buff.gameObject.SetActive(true);
    }

    /// <inheritdoc/>
    public override Vector3 GetClosestInteractionPoint(Vector3 point)
    {
        var dir = (point - transform.position).normalized;
        var distance = (point - transform.position).magnitude;
        return distance > interactionRangeDistance ? transform.position + dir * interactionRangeDistance : point;
    }

    /// <inheritdoc/>
    public override bool TryInteract(Character character)
    {
        if (IsNextBuffAvailable)
        {
            IsNextBuffAvailable = false;
            character.KneelBeforeStatue(transform.position);
            buff.UseOn(character);
            return true;
        }
        return false;
    }

    #endregion
}
