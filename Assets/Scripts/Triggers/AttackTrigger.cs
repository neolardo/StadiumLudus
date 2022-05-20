using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack trigger for any kind object which deals damage.
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("The trasnform of the character which uses this attack trigger. Required to calculate hit directions and to prevent self harm.")]
    public Transform characterTransform;

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
    private List<Character> DamagedCharacters { get; } = new List<Character>();

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
        if (characterTransform == null)
        {
            Debug.LogWarning("An attack trigger's character field is null.");
        }
    }

    private HitDirection CalculateHitDirection(Vector3 otherForward)
    {
        float angle = Vector3.SignedAngle(characterTransform.forward, otherForward, Vector3.up);
        if (Mathf.Abs(angle) < 90)
        {
            return HitDirection.Back;
        }
        else if (angle > 0)
        {
            return HitDirection.FrontRight;
        }
        else
        {
            return HitDirection.FrontLeft;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DealDamage(other);
    }

    private void DealDamage(Collider other)
    {
        if (IsActive && other.tag.Contains(Globals.HitBoxTag) && !other.gameObject.transform.IsChildOf(characterTransform))
        {
            var hitBox = other.GetComponent<HitBox>();
            if (!DamagedCharacters.Contains(hitBox.character))
            {
                if (hitBox.character.TryTakeDamage(Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(hitBox.character.transform.forward)))
                {
                    DamagedCharacters.Add(hitBox.character);
                }
            }
        }
    }

    #endregion
}
