using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack trigger for any kind object which deals damage.
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("The character which uses this attack trigger. Required to calculate hit directions and to prevent self harm.")]
    public GameObject character;

    /// <summary>
    /// The minimum possible damage of this attack trigger.
    /// </summary>
    public float MinimumDamage { get; set; }

    /// <summary>
    /// The maximum possible damage of this attack trigger.
    /// </summary>
    public float MaximumDamage { get; set; }

    /// <summary>
    /// A list of gameobjects representing the previously damaged character.
    /// </summary>
    private List<GameObject> DamagedCharacters { get; } = new List<GameObject>();

    private bool _isActive;

    /// <summary>
    /// Indicates whether this trigger is active or not.
    /// Usually the trigger should be activated for as long as the attack animation takes.
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

    #endregion

    #region Methods

    private void Start()
    {
        if (character == null)
        {
            Debug.LogWarning("An attack trigger's character field is null.");
        }
    }

    private HitDirection CalculateHitDirection(Vector3 otherForward)
    {
        float angle = Vector3.SignedAngle(character.transform.forward, otherForward, Vector3.up);
        if (Mathf.Abs(angle) < 90)
        {
            return HitDirection.Back;
        }
        else if (angle > 0)
        {
            return HitDirection.FrontLeft;
        }
        else
        {
            return HitDirection.FrontRight;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsActive && other.tag.Contains(Globals.CharacterTag) && !DamagedCharacters.Contains(other.gameObject) && other.gameObject != character)
        {
            if (other.GetComponent<Character>().TryTakeDamage(Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(other.transform.forward)))
            {
                DamagedCharacters.Add(other.gameObject);
            }
        }
    }

    #endregion
}
