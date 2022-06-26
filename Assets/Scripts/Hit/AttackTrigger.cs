using Photon.Pun;
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

    private readonly string TryTakeDamageFunctionName = nameof(Character.TryTakeDamage);

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

    private new Collider collider;
    private bool IsColliderCapsule;
    private PhotonView photonView;

    #endregion

    #region Methods

    private void Start()
    {
        if (characterTransform == null)
        {
            Debug.LogWarning("An attack trigger's character field is null.");
        }
        collider = GetComponent<Collider>();
        IsColliderCapsule = collider is CapsuleCollider;
        photonView = GetComponent<PhotonView>();
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
                DealDamage(other);
            }
            else if (!AnyObjectHit && !other.CompareTag(Globals.CharacterTag))
            {
                if (other.tag == Globals.WoodTag)
                {
                    AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.HitOnWood);
                    AnyObjectHit = true;
                }
                else if (other.tag == Globals.StoneTag)
                {
                    AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.HitOnStone);
                    AnyObjectHit = true;
                }
            }
        }
    }

    private (Vector3 point0, Vector3 point1, float radius) CalculateColliderInfo(Collider col)
    {
        Vector3 point0 = Vector3.zero, point1 = Vector3.zero;
        float radius = 0f;
        if (IsColliderCapsule)
        {
            var capsule = col as CapsuleCollider;
            var direction = new Vector3 { [capsule.direction] = 1 };
            var offset = capsule.height / 2 - capsule.radius;
            var localPoint0 = capsule.center - direction * offset;
            var localPoint1 = capsule.center + direction * offset;
            point0 = transform.TransformPoint(localPoint0);
            point1 = transform.TransformPoint(localPoint1);
            radius = capsule.radius;
        }
        else 
        {
            var sphere = col as SphereCollider;
            var localPoint0 = sphere.center - Vector3.up * sphere.radius;
            var localPoint1 = sphere.center + Vector3.up * sphere.radius;
            point0 = transform.TransformPoint(localPoint0);
            point1 = transform.TransformPoint(localPoint1);
            radius = sphere.radius;
        }
        return (point0, point1, radius);
    }

    private void DealDamage(Collider other)
    {
        if (photonView.IsMine)
        {
            var hitBox = other.GetComponent<HitBox>();
            if (!DamagedCharacters.Contains(hitBox.character))
            {
                var info = CalculateColliderInfo(collider);
                hitBox.character.PhotonView.RPC(TryTakeDamageFunctionName, hitBox.character.PhotonView.Controller, Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(hitBox.character.transform.forward), info.point0, info.point1, info.radius);
                DamagedCharacters.Add(hitBox.character); // might refactor later
            }
        }
    }

    #endregion
}
