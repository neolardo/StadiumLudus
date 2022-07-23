using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a projectile weapon.
/// To fire a projectile set it's gameobject to active.
/// </summary>
public class Projectile : PoolableObject
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

    private Rigidbody rb;
    private new Transform transform;
    private Character target;
    private bool hasInitialized = false;
    private bool doesPotentialObjectHitPointExist;
    private bool remoteStopRequested;
    private Vector3 potentialObjectHitPoint;
    private Vector3 originPoint;
    private Vector3 targetPoint;
    private Vector3 verticalTargetOffset = Vector3.up * 1f;
    private Vector3 currentOffset;
    private float verticalRandomOffsetRange = .5f;
    private const float velocity = 70;
    private const float triggerDelaySecondsAfterHit = 0.2f;
    private const float particleSystemStoppingDelaySeconds = 0.5f;
    private const float maximumDirectionalChangePerFixedUpdateFrame = 90f;
    private bool IsStopped => rb.isKinematic;


    #endregion

    #region Methods

    #region Initialize

    [PunRPC]
    public void EnableProjectile(int targetCharacterPhotonViewID = -1)
    {
        target = targetCharacterPhotonViewID == -1 ? null : GameRoundManager.Instance.LocalCharacterReferenceDictionary[targetCharacterPhotonViewID];
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
        StartCoroutine(Fire());
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

    private IEnumerator Fire()
    {
        AudioManager.Instance.PlayOneShotSFX(projectileTrigger.audioSource, projectileSFX, doNotRepeat: true);
        transform.parent = null;
        projectileTrigger.IsActive = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.position = ProjectilePool.spawnZone.position;
        rb.rotation = ProjectilePool.spawnZone.rotation;
        transform.position = ProjectilePool.spawnZone.position;
        transform.rotation = ProjectilePool.spawnZone.rotation;
        originPoint = transform.position;
        currentOffset = verticalTargetOffset + (Random.Range(0, verticalRandomOffsetRange * 2f) - verticalRandomOffsetRange) * Vector3.up;
        targetPoint = transform.position + new Vector3(ProjectilePool.spawnZone.forward.x, 0, ProjectilePool.spawnZone.forward.z) * (distanceMaximum+1);
        CheckPotentialObjectHit();
        yield return new WaitForEndOfFrame(); // to avoid showing the projectile vfx before fireing
        projectileTrailRenderer.emitting = true;
        yield return new WaitForEndOfFrame(); // show the vfx
        rb.isKinematic = false;
        projectileTrigger.IsActive = true;
    }

    private void CheckPotentialObjectHit()
    {
        Ray ray = new Ray(originPoint, (targetPoint - originPoint).normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distanceMaximum, (1 << Globals.DefaultLayer) | (1 << Globals.GroundLayer), QueryTriggerInteraction.Collide))
        {
            potentialObjectHitPoint = hit.point;
            doesPotentialObjectHitPointExist = true;
        }
        else
        {
            doesPotentialObjectHitPointExist = false;
        }
    }

    #endregion

    #region Update

    private void FixedUpdate()
    {
        if (!IsStopped)
        {
            if (photonView.IsMine)
            {
                LocalUpdate();
            }
            else
            {
                RemoteUpdate();
            }
        }
    }

    private void LocalUpdate()
    {
        if (target != null)
        {
            targetPoint = target.transform.position + currentOffset;
        }
        var nextVelocity = (targetPoint - rb.position).normalized * velocity;
        if (target != null && Vector3.Angle(nextVelocity, rb.velocity) > maximumDirectionalChangePerFixedUpdateFrame)
        {
            ForceHitTarget();
        }
        else if ((rb.position - originPoint).magnitude > distanceMaximum)
        {
            OnProjectileWentTooFar();
        }
        else if ((projectileTrigger.AnyObjectHit || (doesPotentialObjectHitPointExist && (potentialObjectHitPoint - originPoint).magnitude < (rb.position - originPoint).magnitude)))
        {
            OnLocalHit(rb.position);
        }
        else
        {
            rb.velocity = nextVelocity;
        }
    }

    private void RemoteUpdate()
    {
        if (target != null)
        {
            targetPoint = target.transform.position + currentOffset;
        }
        var nextVelocity = (targetPoint - rb.position).normalized * velocity;
        if (target != null && Vector3.Angle(nextVelocity, rb.velocity) > maximumDirectionalChangePerFixedUpdateFrame)
        {
            OnRemoteHit(rb.position, target.PhotonView.ViewID, target.GetClosestRigColliderTransformIndex(rb.position));
        }
        else if ((rb.position - originPoint).magnitude > distanceMaximum)
        {
            OnProjectileWentTooFar();
        }
        else if (doesPotentialObjectHitPointExist && (potentialObjectHitPoint - originPoint).magnitude < (rb.position - originPoint).magnitude)
        {
            OnRemoteHit(rb.position);
        }
        else
        {
            rb.velocity = nextVelocity;
        }
    }

    #endregion

    #region Hit


    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            OnLocalTriggerEnter(other);
        }
        else
        {
            OnRemoteTriggerEnter(other);
        }
    }


    private void StickToTarget(Vector3 point, int characterPhotonViewID, int colliderIndex)
    {
        if (characterPhotonViewID != -1)
        {
            var colliderTransform = GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID].GetRigColliderTransform(colliderIndex);
            transform.parent = colliderTransform;
            transform.localPosition = Vector3.zero;
        }
        else
        {
            rb.position = transform.position = point;
        }
    }

    #region Local

    private void OnLocalTriggerEnter(Collider other)
    {
        if (!IsStopped && (other.gameObject.layer == Globals.DefaultLayer || other.gameObject.layer == Globals.HitBoxLayer || other.gameObject.layer == Globals.GroundLayer))
        {
            if (other.CompareTag(Globals.HitBoxTag))
            {
                var hitCharacter = other.GetComponent<HitBox>().character;
                if (hitCharacter == target)
                {
                    projectileTrigger.ForceAttackAfterDelay(target, 0);
                }
                OnLocalHit(rb.position, hitCharacter.PhotonView.ViewID, hitCharacter.GetClosestRigColliderTransformIndex(rb.position));
            }
            else
            {
                OnLocalHit(rb.position);
            }
        }
    }

    private void OnLocalHit(Vector3 point, int characterPhotonViewID = -1, int colliderIndex = -1)
    {
        if (!IsStopped)
        {
            point = doesPotentialObjectHitPointExist ? potentialObjectHitPoint : point;
            photonView.RPC(nameof(RequestRemoteStop), RpcTarget.Others, point, characterPhotonViewID, colliderIndex);
            StickToTarget(point, characterPhotonViewID, colliderIndex);
            Stop();
            StartCoroutine(DisableAttackTriggerAfterDelay());
        }
    }

    private void ForceHitTarget()
    {
        if (target != null)
        {
            projectileTrigger.ForceAttackAfterDelay(target, 0);
            OnLocalHit(rb.position, target.PhotonView.ViewID, target.GetClosestRigColliderTransformIndex(rb.position));
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

    #region Remote

    private void OnRemoteTriggerEnter(Collider other)
    {
        if (!IsStopped && (other.gameObject.layer == Globals.DefaultLayer || other.gameObject.layer == Globals.HitBoxLayer || other.gameObject.layer == Globals.GroundLayer))
        {
            if (other.CompareTag(Globals.HitBoxTag))
            {
                var hitCharacter = other.GetComponent<HitBox>().character;
                if (hitCharacter.PhotonView.Owner == photonView.Owner) // prevent remote self hit
                {
                    Physics.IgnoreCollision(other, GetComponent<Collider>());
                }
                else
                {
                    OnRemoteHit(rb.position, hitCharacter.PhotonView.ViewID, hitCharacter.GetClosestRigColliderTransformIndex(rb.position));
                }
            }
            else
            {
                OnRemoteHit(rb.position);
            }
        }
    }

    [PunRPC]
    public void RequestRemoteStop(Vector3 point, int characterPhotonViewID = -1, int colliderIndex = -1)
    {
        StartCoroutine(WaitUntilStopedThenStickToTarget(point, characterPhotonViewID, colliderIndex));
    }

    private IEnumerator WaitUntilStopedThenStickToTarget(Vector3 point, int characterPhotonViewID, int colliderIndex)
    {
        if (!IsStopped)
        {
            remoteStopRequested = true;
            yield return new WaitUntil(() => IsStopped);
            StickToTarget(point, characterPhotonViewID, colliderIndex);
        }
    }


    private void OnRemoteHit(Vector3 point, int characterPhotonViewID = -1, int colliderIndex = -1)
    {
        if (!IsStopped)
        {
            Stop();
            if (!remoteStopRequested)
            {
                StickToTarget(point, characterPhotonViewID, colliderIndex);
            }
            remoteStopRequested = false;
        }
    }

    #endregion

    #endregion

    #region Stop

    private void OnProjectileWentTooFar()
    {
        if (photonView.IsMine)
        {
            ProjectilePool.OnObjectDisappeared(this);
        }
        Stop();
        projectileTrigger.IsActive = false;
        gameObject.SetActive(false);
        Debug.Log("Projectile went too far, it's been deactivated.");
    }

    private void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        projectileTrailRenderer.emitting = false;
    }

    private IEnumerator DisableAttackTriggerAfterDelay()
    {
        yield return new WaitForSeconds(triggerDelaySecondsAfterHit);
        projectileTrigger.IsActive = false;
    }


    #endregion

    #endregion

}
