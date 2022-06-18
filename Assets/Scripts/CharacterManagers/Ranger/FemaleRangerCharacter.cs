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

    [Tooltip("Represents the force of a fired arrow.")]
    [SerializeField]
    private float arrowForce = 3;

    #region Skills

    #region Dash
    protected override float DashJumpingTime => 0.29f;

    #endregion

    #region Trap

    protected override float TrapPlacementDelay => 0.7f;

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Initialize

    protected override void Initialize()
    {
        base.Initialize();
        if (arrowMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a non-positive value.");
        }
        if (arrowMaximumDamage < arrowMinimumDamage)
        {
            Debug.LogWarning("Arrow maximum damage for a female ranger character is set to a lesser value than the minimum.");
        }
        if (arrowForce <= 0)
        {
            Debug.LogWarning("Arrow force for a female ranger character is set to non-positive value.");
        }
        arrowPool.MinimumDamage = arrowMinimumDamage;
        arrowPool.MaximumDamage = arrowMaximumDamage;
        arrowPool.Force = arrowForce;
    }

    #endregion

    #region Attack

    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAnimations());
    }

    private IEnumerator ManageAnimations()
    {
        yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsAttacking);
        if (animationManager.CanDealDamage)
        {
            animatedArrow.SetActive(false);
            arrowPool.Fire();
        }
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(true);
    }

    #endregion

    #endregion
}
