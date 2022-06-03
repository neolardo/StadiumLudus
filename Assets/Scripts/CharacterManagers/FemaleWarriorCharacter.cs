using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a female warrior character.
/// </summary>
public class FemaleWarriorCharacter : Character
{
    #region Properties and Fields

    private FemaleWarriorAnimationManager femaleWarriorAnimationManager;

    public AttackTrigger leftBattleAxeTrigger;
    public AttackTrigger rightBattleAxeTrigger;

    [Tooltip("Represents the minimum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMinimumDamage;

    [Tooltip("Represents the maximum damage of the battle axe weapon.")]
    [SerializeField]
    private float battleAxeMaximumDamage;

    [Tooltip("Represents audio source of the left battle axe.")]
    [SerializeField]
    private AudioSource leftBattleAxeAudioSource;

    [Tooltip("Represents audio source of the right battle axe.")]
    [SerializeField]
    private AudioSource rightBattleAxeAudioSource;

    #region Combo Attack

    private int currentComboCount = 0;
    private bool canComboContinue = false;
    private const float comboDelaySeconds = .5f;
    private const string AnimatorContinueAttack = "ContinueAttack";

    #endregion

    #region Skills

    #region Leap Attack

    [Tooltip("Represents jump force of the leap attack.")]
    [SerializeField]
    private float leapAttackJumpForce = 250;

    [Tooltip("Represents maximum distance of the leap attack.")]
    [SerializeField]
    private float leapAttackMaximumDistance = 3.5f;

    [Tooltip("Represents cooldown of the leap attack skill in seconds.")]
    [SerializeField]
    private float leapAttackCooldown = 5f;

    private const int LeapAttackSkillNumber = 1;

    private const float jumpingTime = 0.5f;

    private bool IsLeapAttackAvailable { get; set; } = true;

    private bool IsLeapAttackFirstFrame { get; set; }

    private Vector3 jumpTarget;

    private Vector3 currentJumpDelta;

    #endregion

    #region Ground Slam

    private const int GroundSlamSkillNumber = 2;

    #endregion  

    #endregion  

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();
        leftBattleAxeTrigger.MinimumDamage = battleAxeMinimumDamage;
        leftBattleAxeTrigger.MaximumDamage = battleAxeMaximumDamage;
        rightBattleAxeTrigger.MinimumDamage = battleAxeMinimumDamage;
        rightBattleAxeTrigger.MaximumDamage = battleAxeMaximumDamage;
        if (battleAxeMaximumDamage < Globals.CompareDelta)
        {
            Debug.LogWarning("Battle axe maximum damage for a female warrior character is set to a non-positive value.");
        }
        if (battleAxeMaximumDamage < battleAxeMinimumDamage)
        {
            Debug.LogWarning("Battle axe maximum damage for a female warrior character is set to a lesser value than the minimum.");
        }
        femaleWarriorAnimationManager = animationManager as FemaleWarriorAnimationManager;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateLeapAttackJumping();
    }

    #region Attack

    public override bool TryAttack(Vector3 attackTarget)
    {
        if (!femaleWarriorAnimationManager.IsInterrupted && !femaleWarriorAnimationManager.IsGuarding && !femaleWarriorAnimationManager.IsAttacking)
        {
            OnAttack(attackTarget);
            currentComboCount = 0;
            canComboContinue = false;
            femaleWarriorAnimationManager.SetContinueComboAttack(false);
            return true;
        }
        else if (!femaleWarriorAnimationManager.IsInterrupted && !femaleWarriorAnimationManager.IsGuarding && femaleWarriorAnimationManager.IsAttacking 
            && !femaleWarriorAnimationManager.IsContinueAttackRequested && currentComboCount < 2 && canComboContinue)
        {
            StartCoroutine(ComboDelay());
            femaleWarriorAnimationManager.SetContinueComboAttack(true);
            currentComboCount++;
            return true;
        }
        return false;
    }

    protected override void OnAttack(Vector3 attackTarget)
    {
        base.OnAttack(attackTarget);
        StartCoroutine(ManageAttackTrigger());
        StartCoroutine(ComboDelay());
    }


    private IEnumerator ManageAttackTrigger()
    {
        while (animationManager.IsAttacking)
        {
            yield return new WaitUntil(() => animationManager.CanDealDamage || !animationManager.IsAttacking);
            if (animationManager.CanDealDamage)
            {
                leftBattleAxeTrigger.IsActive = true;
                rightBattleAxeTrigger.IsActive = true;
            }
            yield return new WaitWhile(() => animationManager.CanDealDamage);
            leftBattleAxeTrigger.IsActive = false;
            rightBattleAxeTrigger.IsActive = false;
        }
    }


    private IEnumerator ComboDelay()
    {
        canComboContinue = false;
        yield return new WaitForSeconds(comboDelaySeconds);
        canComboContinue = true;
    }

    #endregion

    #region Skills

    public override void FireSkill(int skillNumber, Vector3 clickPosition)
    {
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                LeapAttack(clickPosition);
                break;
            case GroundSlamSkillNumber:
                GroundSlam();
                break;
            default:
                Debug.LogWarning("Invalid skill number for a female warrior character.");
                break;
        }
    }

    #region Leap Attack

    private void LeapAttack(Vector3 attackTarget)
    {
        if (IsAlive && IsLeapAttackAvailable && !femaleWarriorAnimationManager.IsInterrupted && !femaleWarriorAnimationManager.IsAttacking && !femaleWarriorAnimationManager.IsGuarding)
        {
            if ((attackTarget - rb.position).magnitude > leapAttackMaximumDistance)
            {
                var edgePoint = rb.position + (attackTarget - rb.position).normalized * leapAttackMaximumDistance;
                var raycastPoint = edgePoint + Vector3.up * 5;
                Ray ray = new Ray(raycastPoint, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
                {
                    edgePoint = hit.point;
                }
                attackTarget = edgePoint;
            }
            IsLeapAttackFirstFrame = true;
            IsLeapAttackAvailable = false;
            jumpTarget = attackTarget;
            SetRotationTarget(attackTarget);
            MoveTo(jumpTarget);
            femaleWarriorAnimationManager.LeapAttack();
            StartCoroutine(ManageAttackTrigger());
            StartCoroutine(ManageLeapAttackCooldown());
            StartCoroutine(ResetDestinationAfterLeap());
        }
    }

    private void UpdateLeapAttackJumping()
    {
        if (femaleWarriorAnimationManager.IsJumping)
        {
            if (IsLeapAttackFirstFrame)
            {
                rb.AddForce(Vector3.up * leapAttackJumpForce, ForceMode.Impulse);
                currentJumpDelta = (new Vector3(jumpTarget.x, rb.position.y, jumpTarget.z) - rb.position) / (jumpingTime * 50);
                IsLeapAttackFirstFrame = false;
            }
            else
            {
                rb.MovePosition(rb.position + currentJumpDelta);
            }
        }
    }

    private IEnumerator ManageLeapAttackCooldown()
    {
        float remainingTime = leapAttackCooldown;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        IsLeapAttackAvailable = true;
    }

    private IEnumerator ResetDestinationAfterLeap()
    {
        yield return new WaitUntil(() => femaleWarriorAnimationManager.IsJumping);
        yield return new WaitWhile(() => femaleWarriorAnimationManager.IsJumping);
        ClearDestination();
    }

    #endregion

    #region Ground Slam

    private void GroundSlam()
    {
        if (IsAlive && !femaleWarriorAnimationManager.IsInterrupted && !femaleWarriorAnimationManager.IsAttacking && !femaleWarriorAnimationManager.IsGuarding)
        {

        }
    }

    #endregion

    #endregion

    #endregion
}
