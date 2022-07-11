using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female ranger character.
/// </summary>
public class FemaleRangerCharacter : RangerCharacter
{
    #region Properties and Fields
    public override CharacterFightingStyle FightingStyle => CharacterFightingStyle.Light;

    [Header("Arrow")]
    [Tooltip("The arrow pool manager.")]
    [SerializeField]
    private ProjectilePoolManager arrowPool;

    [Tooltip("The arrow game object which is animated.")]
    [SerializeField]
    private GameObject animatedArrow;

    [Tooltip("Represents the minimum damage of a fired arrow.")]
    [SerializeField]
    private float arrowMinimumDamage;

    [Tooltip("Represents the maximum damage of a fired arrow.")]
    [SerializeField]
    private float arrowMaximumDamage;

    #region Skills

    #region Trap

    protected override float TrapPlacementDelay => 0.7f;

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Initialize

    protected override void Awake()
    {
        base.Awake();
        if (arrowMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a non-positive value.");
        }
        if (arrowMaximumDamage < arrowMinimumDamage)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a lesser value than the minimum.");
        }
        arrowPool.MinimumDamage = arrowMinimumDamage;
        arrowPool.MaximumDamage = arrowMaximumDamage;
    }

    #endregion

    #region Attack

    #region Without Target

    protected override void OnAttackWithoutTarget(Vector3 attackTarget)
    {
        chaseTarget = null;
        interactionTarget = null;
        ClearDestination();
        rangerAnimationManager.SetIsDrawing(true);
        Debug.Log("OnAttackWithoutTarget");
        animationManager.Attack();
        StartCoroutine(ManageAnimations());
    }

    private IEnumerator ManageAnimations()
    {
        yield return new WaitUntil(() => (!rangerAnimationManager.IsDrawing && animationManager.CanDealDamage) || !animationManager.IsAttacking);
        if (animationManager.IsAttacking)
        {
            stamina -= attackStaminaCost;
            animatedArrow.SetActive(false);
            arrowPool.Fire(attackTarget);
        }
        yield return new WaitWhile(() => animationManager.IsAttacking);
        animatedArrow.SetActive(true);
    }

    #endregion

    #region With Target

    protected override void OnAttackChaseTarget()
    {
        ClearDestination();
        rangerAnimationManager.SetIsDrawing(true);
        Debug.Log("OnAttackChaseTarget");
        animationManager.Attack();
        StartCoroutine(ManageAnimations());
    }

    #endregion

    #endregion

    #endregion
}
