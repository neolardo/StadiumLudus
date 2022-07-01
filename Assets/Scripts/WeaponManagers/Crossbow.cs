using UnityEngine;

/// <summary>
/// Manages a crossbow's animations and it's <see cref="Projectile"/>s.
/// </summary>
public class Crossbow : MonoBehaviour
{
    #region Properties and Fields
    private Animator animator;

    [Tooltip("The audio source of the character.")]
    [SerializeField]
    private AudioSource characterAudioSource;

    [Tooltip("The bolt projectile pool.")]
    [SerializeField]
    private ProjectilePoolManager boltPool;

    [Tooltip("The bolt game object which is animated.")]
    [SerializeField]
    private GameObject quiverBolt;

    [Tooltip("The bolt game object which is attached to the crossbow.")]
    [SerializeField]
    private GameObject crossbowBolt;
    public bool IsReloading { get; private set; }

    #region Animator Constants

    private const string AnimatorReload = "Reload";
    private const string AnimatorAttack = "Attack";
    private const string AnimatorDieFront = "DieFront";
    private const string AnimatorDieBack = "DieBack";

    #endregion

    #endregion

    #region Methods

    void Start()
    {
        animator = GetComponent<Animator>();
        quiverBolt.SetActive(false);
        crossbowBolt.SetActive(false);
    }

    #region Reload

    /// <summary>
    /// Plays the reload animation.
    /// </summary>
    public void Reload()
    {
        IsReloading = true;
        quiverBolt.SetActive(true);
        animator.SetTrigger(AnimatorReload);
    }

    /// <summary>
    /// Called when the reload animation reached that state when the arrow is placed in it's place, but the animation is not yet finished.
    /// </summary>
    public void OnReloaded()
    {
        quiverBolt.SetActive(false);
        crossbowBolt.SetActive(true);
        AudioManager.Instance.PlayOneShotSFX(characterAudioSource, SFX.CrossbowReload, doNotRepeat : true);
    }

    /// <summary>
    /// Called when the reload animation has finished.
    /// </summary>
    public void OnReloadFinished()
    {
        IsReloading = false;
    }

    #endregion

    #region Attack
    public void Attack()
    {
        animator.SetTrigger(AnimatorAttack);
    }

    public void OnArrowFired()
    {
        crossbowBolt.SetActive(false);
        boltPool.Fire();
    }

    #endregion

    #region Die

    public void Die(HitDirection direction)
    {
        animator.SetTrigger(direction == HitDirection.Back ? AnimatorDieBack : AnimatorDieFront);
    }

    #endregion

    #endregion
}
