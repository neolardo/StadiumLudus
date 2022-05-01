using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack trigger for any kind object which deals damage.
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    private float minimumDamage;
    private float maximumDamage;

    /// <summary>
    /// A list of gameobjects representing the previously damaged character.
    /// </summary>
    private List<GameObject> DamagedCharacters { get; } = new List<GameObject>();

    private bool _isActive;

    /// <summary>
    /// Indicates whether this trigger is active or not.
    /// The trigger should be activated for as long as the attack animation takes.
    /// </summary>
    public bool IsActive {
        get 
        {
            return _isActive; 
        }
            
        set
        {
            _isActive = value;
            if (!value)
            {
                DamagedCharacters.Clear();
            }
        }
    }

    public void SetDamage(float minimum, float maximum)
    {
        minimumDamage = minimum;
        maximumDamage = maximum;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsActive && other.tag.Contains(Globals.CharacterTag) && !DamagedCharacters.Contains(other.gameObject))
        {
            other.GetComponent<Character>().TakeDamage(Random.Range(minimumDamage, maximumDamage), HitDirection.Back);
            DamagedCharacters.Add(other.gameObject);
        }
    }
}
