using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female ranger character.
/// </summary>
public class FemaleRangerCharacter : Character
{
    #region Properties and Fields

    public Projectile arrowProjectile;

    public AttackTrigger arrowTrigger;

    [Tooltip("The arrow game object which is animated.")]
    [SerializeField]
    private GameObject animatedArrow;

    [Tooltip("The arrow game object which is fired.")]
    [SerializeField]
    private GameObject firedArrow;

    [Tooltip("The spawn zone of the arrows.")]
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
        arrowProjectile.Force = arrowForce;
        if (arrowMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a non-positive value.");
        }
        if (arrowMaximumDamage < arrowMinimumDamage)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a lesser value than the minimum.");
        }
    }

    #region Attack

    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAnimations());
        StartCoroutine(RotateToAttackDirection(attackTarget));
    }

    private IEnumerator ManageAnimations()
    {
        firedArrow.SetActive(false);
        yield return new WaitUntil(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(false);
        firedArrow.SetActive(true);
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(true);
    }

    #endregion

    #endregion
}
