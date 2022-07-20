using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a male ranger character.
/// </summary>
public class MaleRangerCharacter : RangerCharacter
{
    #region Properties and Fields

    private MaleRangerAnimationManager maleRangerAnimationManager;

    public override CharacterFightingStyle FightingStyle => CharacterFightingStyle.Heavy;

    [Header("Bolt")]
    public Crossbow crossbow;

    [Tooltip("The bolt pool manager.")]
    [SerializeField]
    public ProjectilePoolManager boltPool;

    [Tooltip("Represents the minimum damage of a fired bolt.")]
    [SerializeField]
    private float boltMinimumDamage;

    [Tooltip("Represents the maximum damage of a fired bolt.")]
    [SerializeField]
    private float boltMaximumDamage;

    private bool IsBoltLoaded { get; set; }

    protected override bool IsInAction => base.IsInAction || crossbow.IsReloading;

    protected override bool CanAttack => base.CanAttack && IsBoltLoaded;
    
    private bool CanReload => IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill && !animationManager.IsInteracting && !crossbow.IsReloading && !IsBoltLoaded;

    #region Skills

    #region Trap

    protected override float TrapPlacementDelay => Trap.DeactivationDelay + .1f;

    #endregion

    #endregion

    #endregion

    #region Methods

    #region Init

    protected override void Awake()
    {
        base.Awake();
        if (boltMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Bolt maximum damage for a male ranger character is set to a non-positive value.");
        }
        if (boltMaximumDamage < boltMinimumDamage)
        {
            Debug.LogWarning("Bolt maximum damage for a male ranger character is set to a lesser value than the minimum.");
        }
        boltPool.MinimumDamage = boltMinimumDamage;
        boltPool.MaximumDamage = boltMaximumDamage;
        maleRangerAnimationManager = animationManager as MaleRangerAnimationManager;
    }

    #endregion

    #region Attack

    #region Attack

    public override void StartAttack(Vector3 attackPoint, Character target = null)
    {
        if (CanReload)
        {
            Reload();
        }
        else if (IsBoltLoaded)
        {
            base.StartAttack(attackPoint, target);
        }
    }

    [PunRPC]
    public void Reload()
    {
        if (PhotonView.IsMine)
        {
            PhotonView.RPC(nameof(Reload), RpcTarget.Others);
        }
        maleRangerAnimationManager.Reload();
        crossbow.Reload();
        IsBoltLoaded = true;
    }

    #region Without Target

    protected override void OnAttackWithoutTarget(Vector3 attackTarget)
    {
        chaseTarget = null;
        interactionTarget = null;
        ClearDestination();
        rangerAnimationManager.SetIsDrawing(true);
        animationManager.Attack();
        crossbow.Draw();
        StartCoroutine(ManageAnimations());
    }

    private IEnumerator ManageAnimations()
    {
        yield return new WaitUntil(() => !rangerAnimationManager.IsDrawing || !animationManager.IsAttacking);
        if (animationManager.IsAttacking)
        {
            stamina -= attackStaminaCost;
            crossbow.Fire(attackTarget);
            IsBoltLoaded = false;
        }
    }

    #endregion

    #region With Target

    protected override void OnAttackChaseTarget()
    {
        ClearDestination();
        rangerAnimationManager.SetIsDrawing(true);
        animationManager.Attack();
        crossbow.Draw();
        StartCoroutine(ManageAnimations());
    }

    #endregion

    #endregion

    #endregion

    #region Take Damage

    protected override void OnTakeDamage()
    {
        base.OnTakeDamage();
        crossbow.OnTakeDamage();
    }

    #endregion

    #region Die

    [PunRPC]
    protected override void OnDie(HitDirection direction)
    {
        base.OnDie(direction);
        crossbow.Die(direction);
    }

    #endregion

    #endregion
}
