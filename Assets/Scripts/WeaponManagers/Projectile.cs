using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a projectile weapon.
/// To fire a projectile set it's gameobject to active.
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Properties and Fields

    [Tooltip("The attack trigger of the projectile.")]
    public AttackTrigger projectileTrigger;

    [Tooltip("The projectile trail renderer.")]
    public TrailRenderer projectileTrailRenderer;

    [Tooltip("The sound effect of the projectile.")]
    public SFX projectileSFX;

    /// <summary>
    /// The projectile pool.
    /// </summary>
    public ProjectilePoolManager ProjectilePool { get; set; }

    /// <summary>
    /// Indicates the maximum distance until an arrow can travel.
    /// After that the arrow will be deactivated.
    /// </summary>
    private const float distanceMaximum = 20;

    public PhotonView photonView;
    private Rigidbody rb;
    private new Transform transform;
    private Character target;

    private Vector3 originPoint;
    private Vector3 targetPoint;
    private Vector3 targetOffset = Vector3.up * 1f;
    private const float velocity = 100;

    private const float triggerDelaySecondsAfterHit = 0.2f;

    private const float particleSystemStoppingDelaySeconds = 0.5f;
    private bool IsStopped => rb.isKinematic;

    private bool hasInitialized = false;

    #endregion

    #region Methods

    #region Initialize

    [PunRPC]
    public void EnableProjectile(int targetCharacterPhotonViewID = -1)
    {
        SetTarget(targetCharacterPhotonViewID);
        gameObject.SetActive(true);
    }

    [PunRPC]
    public void DisableProjectile()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
        Fire();
    }

    private void Initialize()
    {
        transform = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        hasInitialized = true;
    }

    #endregion

    #region Fire

    private void Fire()
    {
        AudioManager.Instance.PlayOneShotSFX(projectileTrigger.audioSource, projectileSFX, doNotRepeat: true);
        gameObject.transform.parent = null;
        projectileTrigger.IsActive = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.position = ProjectilePool.spawnZone.position;
        rb.rotation = ProjectilePool.spawnZone.rotation;
        transform.position = ProjectilePool.spawnZone.position;
        transform.rotation = ProjectilePool.spawnZone.rotation;
        originPoint = transform.position;
        targetPoint = transform.position + new Vector3(ProjectilePool.spawnZone.forward.x, 0, ProjectilePool.spawnZone.forward.z) * (distanceMaximum+1);
        rb.isKinematic = false;
        projectileTrailRenderer.emitting = true;
        projectileTrigger.IsActive = true;
    }

    #endregion

    #region Update

    private void FixedUpdate()
    {
        if (!IsStopped)
        {
            if (target != null)
            {
                targetPoint = target.transform.position + targetOffset;
            }
            var nextPosition = rb.position + velocity * Time.fixedDeltaTime * (targetPoint - rb.position).normalized;
            if (photonView.IsMine && target != null && (targetPoint - nextPosition).magnitude > (targetPoint - rb.position).magnitude)
            {
                ForceHitTarget();
            }
            else if (photonView.IsMine && (rb.position - originPoint).magnitude > distanceMaximum)
            {
                OnProjectileWentTooFar();
            }
            else
            {
                rb.MovePosition(nextPosition);
            }
        }
    }

    #endregion

    #region Target
    private void SetTarget(int characterPhotonViewID)
    {
        target = characterPhotonViewID == -1 ? null : GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID];
    }

    #endregion

    #region Hit

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || other.gameObject.transform.IsChildOf(ProjectilePool.characterTransform) || other.CompareTag(Globals.CharacterTag) || other.CompareTag(Globals.AttackTriggerTag) || other.CompareTag(Globals.IgnoreBoxTag))
        {
            return;
        }
        else if (!IsStopped)
        {
            if (other.CompareTag(Globals.HitBoxTag))
            {
                var hitCharacter = other.GetComponent<HitBox>().character;
                if (hitCharacter == target)
                {
                    projectileTrigger.ForceAttackAfterDelay(target, 0);
                }
                OnHit(rb.position, hitCharacter.PhotonView.ViewID, hitCharacter.GetClosestRigColliderTransformIndex(rb.position));
            }
            else
            {
                OnHit(rb.position);
            }
        }
    }

    private void ForceHitTarget()
    {
        if (target != null)
        {
            projectileTrigger.ForceAttackAfterDelay(target, 0);
            OnHit(rb.position, target.PhotonView.ViewID, target.GetClosestRigColliderTransformIndex(rb.position));
        }
    }

    [PunRPC]
    public void OnHit(Vector3 point, int characterPhotonViewID = -1, int colliderIndex = -1)
    {
        if (!IsStopped)
        {
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(OnHit), RpcTarget.Others, point, characterPhotonViewID, colliderIndex);
            }
            if (characterPhotonViewID != -1)
            {
                var colliderTransform = GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID].GetRigColliderTransform(colliderIndex);
                transform.parent = colliderTransform;
                transform.localPosition = Vector3.zero;
            }
            else
            {
                rb.position = point;
            }
            Stop();
            StartCoroutine(DisableAttackTriggerAfterDelay());
            StopTrailParticleSystem();
        }
    }

    [PunRPC]
    public void OnDamagingSucceeded(int characterPhotonViewID)
    {
        projectileTrigger.OnDamagingSucceeded(characterPhotonViewID);
    }

    [PunRPC]
    public void OnDamagingFailed(int characterPhotonViewID)
    {
        projectileTrigger.OnDamagingFailed(characterPhotonViewID);
    }

    #endregion

    #region Stop

    [PunRPC]
    public void OnProjectileWentTooFar()
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(OnProjectileWentTooFar), RpcTarget.Others);
            ProjectilePool.OnProjectileDisappeared(this);
        }
        StopTrailParticleSystem();
        Stop();
        projectileTrigger.IsActive = false;
        gameObject.SetActive(false);
        Debug.Log("Projectile went too far, it's been deactivated.");
    }

    private void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
    }

    private IEnumerator DisableAttackTriggerAfterDelay()
    {
        yield return new WaitForSeconds(triggerDelaySecondsAfterHit);
        projectileTrigger.IsActive = false;
    }

    private void StopTrailParticleSystem()
    {
        projectileTrailRenderer.emitting = false;
    }

    #endregion

    #endregion

}
