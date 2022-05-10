using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a female ranger character.
/// </summary>
public class MaleRangerCharacter : Character
{
    #region Properties and Fields

    public Crossbow crossbow;

    public Arrow arrow;

    public AttackTrigger arrowTrigger;

    [Tooltip("The arrow game object which is animated.")]
    [SerializeField]
    private GameObject animatedArrow;

    [Tooltip("The arrow game object which is fired.")]
    [SerializeField]
    private GameObject firedArrow;

    [Tooltip("The spanw zone of the arrows.")]
    [SerializeField]
    private GameObject arrowSpawnZone;

    [Tooltip("Represents the minimum damage of a fired arrow.")]
    [SerializeField]
    private float arrowMinimumDamage;

    [Tooltip("Represents the maximum damage of a fired arrow.")]
    [SerializeField]
    private float arrowMaximumDamage;

    [Tooltip("Represents the force of a fired arrow.")]
    [SerializeField]
    private float arrowForce = 3;


    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        arrowTrigger.MinimumDamage = arrowMinimumDamage;
        arrowTrigger.MaximumDamage = arrowMaximumDamage;
        if (arrowMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a non-positive value.");
        }
        if (arrowMaximumDamage < arrowMinimumDamage)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a lesser value than the minimum.");
        }
        arrow.Force = arrowForce;
    }

    #region Attack

    protected override void OnAttack(Vector3 attackTarget)
    {
        StartCoroutine(ManageAttackTrigger());
        StartCoroutine(RotateToAttackDirection(attackTarget));
    }

    private IEnumerator ManageAttackTrigger()
    {
        firedArrow.SetActive(false);
        yield return new WaitUntil(() => animationManager.CanDealDamage);
        arrowTrigger.IsActive = true;
        animatedArrow.SetActive(false);
        firedArrow.SetActive(true);
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        arrowTrigger.IsActive = false;
        animatedArrow.SetActive(true);
    }

    #endregion

    #region Die

    protected override void OnDie(HitDirection direction)
    {
        base.OnDie(direction);
        crossbow.AnimateDeath(direction);
    }

    #endregion

    #endregion
}
