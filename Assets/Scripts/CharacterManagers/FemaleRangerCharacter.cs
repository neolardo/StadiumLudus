using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female ranger character.
/// </summary>
public class FemaleRangerCharacter : Character
{
    #region Properties and Fields

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

    private bool hasInitialized;

    #endregion

    #region Methods


    protected void OnEnable()
    {
        // order is important
        if (!hasInitialized)
        {
            Initialize();
            hasInitialized = true;
        }
    }

    private void Initialize()
    {
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

    #region Skills

    public override void FireSkill(int skillNumber, Vector3 clickPosition)
    {
        throw new System.NotImplementedException();
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
        yield return new WaitUntil(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(false);
        arrowPool.Fire();
        yield return new WaitWhile(() => animationManager.CanDealDamage);
        animatedArrow.SetActive(true);
    }

    #endregion

    #endregion
}
