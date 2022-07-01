using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the animations and the behaviour of a ranger trap.
/// </summary>
public class Trap : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField]
    private Animator animator;

    public AttackTrigger trapTrigger;

    [HideInInspector]
    public TrapPoolManager trapPool;
    
    [HideInInspector]
    public float activeDuration;

    public bool IsTrapActive => trapTrigger.IsActive;
    private bool CanExpire => trapTrigger.IsActive;

    private const string AnimatorOpen = "Open";
    private const string AnimatorClose = "Close";
    private const float ActivationDelay = 0.5f;
    private const float DeactivationDelay = 0.7f;
    #endregion

    #region Methods

    private void OnEnable()
    {
        ActivateTrap();
    }

    private void ActivateTrap()
    {
        animator.SetTrigger(AnimatorOpen);
        AudioManager.Instance.PlayOneShotSFX(trapTrigger.audioSource, SFX.TrapActivate);
        StartCoroutine(ActivateAttackTriggerAfterDelay());
    }

    public void DeactivateTrap(bool notifyTrapPool = true)
    {
        trapTrigger.IsActive = false;
        animator.SetTrigger(AnimatorClose);
        AudioManager.Instance.PlayOneShotSFX(trapTrigger.audioSource, SFX.TrapDeactivate);
        StartCoroutine(WaitForDeactivationAndHide(notifyTrapPool));
    }

    private IEnumerator ActivateAttackTriggerAfterDelay()
    {
        yield return new WaitForSeconds(ActivationDelay);
        trapTrigger.IsActive = true;
        StartCoroutine(ExpireTrapAfterDurationEndsOrEnemyHit());
    }

    private IEnumerator ExpireTrapAfterDurationEndsOrEnemyHit()
    {
        float elapsedTime = 0;
        while(gameObject.activeSelf && CanExpire &&  elapsedTime < activeDuration && !trapTrigger.AnyCharacterHit)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (gameObject.activeSelf && CanExpire)
        {
            DeactivateTrap();
        }
    }

    private IEnumerator WaitForDeactivationAndHide(bool notifyTrapPool)
    {
        yield return new WaitForSeconds(DeactivationDelay);
        if (notifyTrapPool)
        {
            trapPool.OnTrapDisappeared(this);
        }
        gameObject.SetActive(false);
    }

    #endregion
}
