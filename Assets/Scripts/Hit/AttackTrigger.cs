using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack trigger for any kind object that deals damage.
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("The audio source of the attack dealing object for playing impact audio.")]
    public AudioSource audioSource;

    [Tooltip("The photon view of the object with this attack trigger.")]
    public PhotonView photonView;

    [Tooltip("The transform of the owner character used to determine the hit direction.")]
    public Transform ownerTransform;

    [Tooltip("True if the attack trigger transform's forward should be used as the attack direction, otherwise the direction between owner character and the target's center will be used.")]
    [SerializeField]
    private bool useForwardAsAttackDirection = false;

    [Tooltip("Indicates whether this attack trigger can be guarded or not.")]
    [SerializeField]
    private bool canBeGuarded = true;

    /// <summary>
    /// The minimum possible damage of this attack trigger.
    /// </summary>
    public float MinimumDamage { get; set; }

    /// <summary>
    /// The maximum possible damage of this attack trigger.
    /// </summary>
    public float MaximumDamage { get; set; }

    /// <summary>
    /// A list of gameobjects representing the previously attacked characters.
    /// </summary>
    private List<Character> AttackedCharacters { get; } = new List<Character>();

    /// <summary>
    /// A list of gameobjects representing the previously damaged characters.
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
                forceAttackTarget = null;
                AnyObjectHit = false;
                DamagedCharacters.Clear();
                AttackedCharacters.Clear();
            }
        }
    }

    /// <summary>
    /// The temporary force attack target of this attack trigger.
    /// </summary>
    private Character forceAttackTarget;

    private new Transform transform;
    private new Collider collider;
    private bool IsColliderCapsule;

    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
        collider = GetComponent<Collider>();
        transform = GetComponent<Transform>();
        IsColliderCapsule = collider is CapsuleCollider;
    }

    #endregion

    #region Regular Attack

    private void OnTriggerEnter(Collider other)
    {
        if (IsActive)
        {
            if (other.tag.Contains(Globals.HitBoxTag))
            {
                DealDamage(other);
            }
            else if (!AnyObjectHit)
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

    private void DealDamage(Collider other)
    {
        if (photonView.IsMine)
        {
            var hitBox = other.GetComponent<HitBox>();
            if (!AttackedCharacters.Contains(hitBox.character))
            {
                var info = CalculateColliderInfo(collider);
                hitBox.character.PhotonView.RPC(TryTakeDamageFunctionName, hitBox.character.PhotonView.Controller, Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(hitBox.character.transform.forward, hitBox.character.transform.position), info.point0, info.point1, info.radius, photonView.ViewID, forceAttackTarget == hitBox.character, canBeGuarded);
                AttackedCharacters.Add(hitBox.character);
                Debug.LogWarning($"Trying regular attack...");
            }
        }
    }

    #endregion

    #region Force Attack

    public void ForceAttackAfterDelay(Character target, float delaySeconds)
    {
        StartCoroutine(WaitUntilDelayThenForceAttack(target, delaySeconds));
    }

    private IEnumerator WaitUntilDelayThenForceAttack(Character target, float delaySeconds)
    {
        forceAttackTarget = target;
        yield return new WaitForSeconds(delaySeconds);
        if (IsActive && !AttackedCharacters.Contains(target))
        {
            target.PhotonView.RPC(TryTakeDamageFunctionName, target.PhotonView.Controller, Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(target.transform.forward, target.transform.position), Vector3.zero, Vector3.zero, 0f, photonView.ViewID, true, canBeGuarded);
            AttackedCharacters.Add(target);
            Debug.LogWarning($"Force attack successful.");
        }
        else
        {
            Debug.LogWarning($"Could not force attack: isActive: {IsActive}, already attacked:{AttackedCharacters.Contains(target)}");
        }
    }

    #endregion

    #region Calculate Infos

    private (Vector3 point0, Vector3 point1, float radius) CalculateColliderInfo(Collider col)
    {
        Vector3 point0, point1;
        float radius;
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

    private HitDirection CalculateHitDirection(Vector3 otherForward, Vector3 otherPosition)
    {
        var attackDirection = useForwardAsAttackDirection ? transform.forward : (otherPosition - ownerTransform.position).normalized;
        float angle = Vector3.SignedAngle(attackDirection, otherForward, Vector3.up);
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

    #endregion

    #region Character Damaged

    [PunRPC]
    public void OnCharacterDamaged(int characterPhotonViewID)
    {
        DamagedCharacters.Add(GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID]);
    }

    #endregion

    #endregion
}
