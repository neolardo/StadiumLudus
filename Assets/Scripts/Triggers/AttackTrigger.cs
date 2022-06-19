using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack trigger for any kind object which deals damage.
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("The transform of the character which uses this attack trigger. Required to calculate hit directions and to prevent self harm.")]
    public Transform characterTransform;

    [Tooltip("The audio source of the attack dealing object for playing impact audio.")]
    public AudioSource audioSource;

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

    /// <summary>
    /// Indiactes whether an object has been hit by this attack trigger.
    /// </summary>
    private bool AnyObjectHit { get; set; }

    /// <summary>
    /// Indiactes whether a character has been hit by this attack trigger.
    /// </summary>
    public bool AnyCharacterHit => DamagedCharacters.Count > 0;

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
                AnyObjectHit = false;
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
        if (IsActive)
        {
            if (other.tag.Contains(Globals.HitBoxTag) && !other.gameObject.transform.IsChildOf(characterTransform))
            {
                TryDealDamage(other);
            }
            else if(!AnyObjectHit && !other.CompareTag(Globals.CharacterTag))
            {
                var sfx = other.tag switch
                {
                    Globals.WoodTag => SFX.HitOnWood,
                    Globals.StoneTag => SFX.HitOnStone,
                    _ => SFX.HitOnStone,
                };
                AudioManager.Instance.PlayOneShotSFX(audioSource, sfx);
                AnyObjectHit = true;
            }
        }
    }

    private bool TryDealDamage(Collider other)
    {
        var hitBox = other.GetComponent<HitBox>();
        if (!DamagedCharacters.Contains(hitBox.character))
        {
            if (hitBox.character.TryTakeDamage(Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(hitBox.character.transform.forward)))
            {
                DamagedCharacters.Add(hitBox.character);
                return true;
            }
        }
        return false;
    }

    #endregion
}
