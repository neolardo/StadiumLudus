using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the animations and the behavior of a ranger trap.
/// </summary>
public class Trap : PoolableObject
{
    #region Fields and Properties

    [SerializeField]
    private Animator animator;

    public AttackTrigger trapTrigger;

    [HideInInspector]
    public TrapPoolManager trapPool;
    
    [HideInInspector]
    public float activeDuration;

    private bool isTrapOpened;

    public bool IsTrapActive => trapTrigger.IsActive;

    private const string AnimatorOpen = "Open";
    private const string AnimatorClose = "Close";
    private const float ActivationDelay = 0.3f;
    public const float DeactivationDelay = 0.7f;
    #endregion

    #region Methods

    #region Activate

    private void OnEnable()
    {
        ActivateTrap();
    }

    private void ActivateTrap()
    {
        transform.position = trapPool.container.position;
        transform.rotation = trapPool.container.rotation;
        animator.SetTrigger(AnimatorOpen);
        isTrapOpened = true;
        AudioManager.Instance.PlayOneShotSFX(trapTrigger.audioSource, SFX.TrapActivate);
        StartCoroutine(ActivateAttackTriggerAfterDelay());
    }

    private IEnumerator ActivateAttackTriggerAfterDelay()
    {
        yield return new WaitForSeconds(ActivationDelay);
        trapTrigger.IsActive = true;
        StartCoroutine(ExpireTrapAfterDurationEndsOrEnemyHit());
    }

    #endregion

    #region Deactivate

    [PunRPC]
    public override void DisableObject()
    {
        DeactivateTrap();
    }

    private IEnumerator ExpireTrapAfterDurationEndsOrEnemyHit()
    {
        float elapsedTime = 0;
        while(isTrapOpened && elapsedTime < activeDuration && !trapTrigger.AnyCharacterHit)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        DeactivateTrap();
    }

    public void DeactivateTrap()
    {
        if (isTrapOpened)
        {
            isTrapOpened = false;
            trapTrigger.IsActive = false;
            animator.SetTrigger(AnimatorClose);
            AudioManager.Instance.PlayOneShotSFX(trapTrigger.audioSource, SFX.TrapDeactivate);
            StartCoroutine(WaitForDeactivationAndDisable());
        }
    }

    private IEnumerator WaitForDeactivationAndDisable()
    {
        yield return new WaitForSeconds(DeactivationDelay);
        if (photonView.IsMine)
        {
            trapPool.OnObjectDisappeared(this);
        }
        gameObject.SetActive(false);
    }

    #endregion

    #endregion
}
