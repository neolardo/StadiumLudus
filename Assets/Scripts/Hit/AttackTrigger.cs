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

    [Tooltip("The transform of the owner character used to determine the hit direction. Optional, if the hit direction is calculated using the attack trigger's foward vector.")]
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
    /// A list of <see cref="Character"/>s which are currently in the trigger zone.
    /// </summary>
    private List<Character> TriggeredCharacters { get; } = new List<Character>();

    /// <summary>
    /// A list of previously attacked <see cref="Character"/>s.
    /// </summary>
    private List<Character> AttackedCharacters { get; } = new List<Character>();

    /// <summary>
    /// A list of previously damaged <see cref="Character"/>s.
    /// </summary>
    private List<Character> DamagedCharacters { get; } = new List<Character>();

    /// <summary>
    /// Indiactes whether an object has been hit by this attack trigger.
    /// </summary>
    public bool AnyObjectHit { get; private set; }

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
            if (value)
            {
                OnActivated();
            }
            else
            {
                OnDeactivated();
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

    #region Activation

    private void OnActivated()
    {
        foreach (var c in TriggeredCharacters)
        {
            DealDamage(c);
        }
    }

    private void OnDeactivated()
    {
        forceAttackTarget = null;
        AnyObjectHit = false;
        DamagedCharacters.Clear();
        AttackedCharacters.Clear();
    }

    #endregion

    #region Trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains(Globals.HitBoxTag))
        {
            var character = other.GetComponent<HitBox>().character;
            TriggeredCharacters.Add(character);
            if (IsActive)
            {
                DealDamage(character);
            }
        }
        else if (IsActive && !AnyObjectHit)
        {
            if (other.tag.Contains(Globals.WoodTag))
            {
                AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.HitOnWood);
                AnyObjectHit = true;
            }
            else if (other.tag.Contains(Globals.StoneTag))
            {
                AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.HitOnStone);
                AnyObjectHit = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains(Globals.HitBoxTag))
        {
            var character = other.GetComponent<HitBox>().character;
            TriggeredCharacters.Remove(character);
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

    #region Regular Attack

    private void DealDamage(Character target)
    {
        if (photonView.IsMine)
        {
            if (!AttackedCharacters.Contains(target))
            {
                var info = CalculateColliderInfo(collider);
                target.PhotonView.RPC(TryTakeDamageFunctionName, target.PhotonView.Controller, Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(target.transform.forward, target.transform.position), info.point0, info.point1, info.radius, photonView.ViewID, forceAttackTarget == target, canBeGuarded);
                AttackedCharacters.Add(target);
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
        if (photonView.IsMine)
        {
            forceAttackTarget = target;
            yield return new WaitForSeconds(delaySeconds);
            if (IsActive && !AttackedCharacters.Contains(target))
            {
                target.PhotonView.RPC(TryTakeDamageFunctionName, target.PhotonView.Controller, Random.Range(MinimumDamage, MaximumDamage), CalculateHitDirection(target.transform.forward, target.transform.position), Vector3.zero, Vector3.zero, 0f, photonView.ViewID, true, canBeGuarded);
                AttackedCharacters.Add(target);
            }
        }
    }

    #endregion

    #region Character Damaged

    [PunRPC]
    public void OnDamagingSucceeded(int characterPhotonViewID)
    {
        if (IsActive)
        {
            DamagedCharacters.Add(GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID]);
        }
    }

    [PunRPC]
    public void OnDamagingFailed(int characterPhotonViewID)
    {
        if (IsActive)
        {
            var target = GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID];
            AttackedCharacters.Remove(target);
            if (TriggeredCharacters.Contains(target))
            {
                DealDamage(target);
            }
        }
    }

    #endregion

    #endregion
}
