using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a female warrior character.
/// </summary>
public class FemaleWarriorCharacter : Character
{
    #region Properties and Fields

    private FemaleWarriorAnimationManager femaleWarriorAnimationManager;

    [Header("Battle Axe")]
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

    [Header("Leap Attack")]
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

    private const float leapAttackJumpingTime = 0.5f;

    private bool IsLeapAttackAvailable { get; set; } = true;

    private bool IsLeapAttackFirstFrame { get; set; }

    private Vector3 leapAttackTarget;

    private Vector3 leapAttackJumpDelta;
    private bool CanLeapAttack => IsAlive && IsLeapAttackAvailable && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill;


    #endregion

    #region Whirlwind

    [Header("Whirlwind")]
    [Tooltip("Represents the rotation speed of the whirlwind in degrees/second.")]
    [SerializeField]
    private float whirlwindRotationSpeed = 920f;

    [Tooltip("Represents the amount of stamina is drained while using the whirlwind per seconds.")]
    [SerializeField]
    private float whirlwindStaminaCost = 35f;

    private const float whirlwindStartAnimationDelay = 0.5f;
    private const float whirlwindEndAnimationDelay = 0.3f;
    private const int WhirlwindSkillNumber = 2;
    private bool CanWhirlwind => IsAlive && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill;

    #endregion

    #region Ground Slam

    private const int GroundSlamSkillNumber = 3;

    [Header("Ground Slam")]
    [SerializeField]
    private GroundSlamManager groundSlamManager;

    [Tooltip("Represents the stamina cost of the ground slam skill.")]
    [SerializeField]
    private float groundSlamStaminaCost = 20f;

    [Tooltip("Represents maximum distance of the ground slam.")]
    [SerializeField]
    private float groundSlamMaximumDistance = 3.5f;

    [Tooltip("Represents cooldown of the ground slam skill in seconds.")]
    [SerializeField]
    private float groundSlamCooldown = 5f;
    private bool IsGroundSlamAvailable { get; set; } = true;
    private bool CanGroundSlam => IsAlive && IsGroundSlamAvailable && !animationManager.IsInterrupted && !animationManager.IsAttacking && !animationManager.IsGuarding && !animationManager.IsUsingSkill;

    private const float GroundSlamStartDelay = 1f;

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

    public override void StartSkill(int skillNumber, Vector3 clickPosition)
    {
        switch (skillNumber)
        {
            case LeapAttackSkillNumber:
                LeapAttack(clickPosition);
                break;
            case WhirlwindSkillNumber:
                StartWhirlwind();
                break;
            case GroundSlamSkillNumber:
                GroundSlam(clickPosition);
                break;
            default:
                Debug.LogWarning("Invalid skill number for a female warrior character.");
                break;
        }
    }

    public override void EndSkill(int skillNumber)
    {
        if (skillNumber == WhirlwindSkillNumber)
        {
            EndWhirlwind();
        }
    }

    #region Leap Attack
    private void LeapAttack(Vector3 attackTarget)
    {
        if (CanLeapAttack)
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
            leapAttackTarget = attackTarget;
            SetRotationTarget(attackTarget);
            MoveTo(leapAttackTarget);
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
                leapAttackJumpDelta = (new Vector3(leapAttackTarget.x, rb.position.y, leapAttackTarget.z) - rb.position) / (leapAttackJumpingTime * 50);
                IsLeapAttackFirstFrame = false;
            }
            else
            {
                rb.MovePosition(rb.position + leapAttackJumpDelta);
            }
        }
    }

    private IEnumerator ManageLeapAttackCooldown()
    {
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(LeapAttackSkillNumber, leapAttackCooldown);
        }
        yield return new WaitForSeconds(leapAttackCooldown);
        IsLeapAttackAvailable = true;
    }

    private IEnumerator ResetDestinationAfterLeap()
    {
        yield return new WaitUntil(() => femaleWarriorAnimationManager.IsJumping);
        yield return new WaitWhile(() => femaleWarriorAnimationManager.IsJumping);
        ClearDestination();
    }

    #endregion

    #region Whirlwind

    private void StartWhirlwind()
    {
        if (CanWhirlwind)
        {
            femaleWarriorAnimationManager.StartWhirlwind();
            StartCoroutine(ManageWhirlwindStaminaDrain());
            StartCoroutine(ManageWhirlwindRotation());
        }
    }

    private void EndWhirlwind()
    {
        if (IsAlive && femaleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            femaleWarriorAnimationManager.EndWhirlwind();
        }
    }

    private IEnumerator ManageWhirlwindRotation()
    {
        yield return new WaitForSeconds(whirlwindStartAnimationDelay);
        while (femaleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(whirlwindRotationSpeed * Time.deltaTime, Vector3.up));
            yield return null;
        }
        float elapsedTime = 0;
        while (elapsedTime < whirlwindEndAnimationDelay)
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(Mathf.Lerp(whirlwindRotationSpeed, 0, elapsedTime / whirlwindEndAnimationDelay) * Time.deltaTime, Vector3.up));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ManageWhirlwindStaminaDrain()
    {
        var staminaDelta = Time.deltaTime * whirlwindStaminaCost;
        while (stamina > staminaDelta && femaleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            stamina -= staminaDelta;
            yield return null;
            staminaDelta = Time.deltaTime * whirlwindStaminaCost;
        }
        if (femaleWarriorAnimationManager.IsWhirlwindOnGoing)
        {
            EndWhirlwind();
        }
    }

    #endregion


    #region Ground Slam

    private void GroundSlam(Vector3 attackTarget)
    {
        if (CanGroundSlam)
        {
            if ((attackTarget - rb.position).magnitude > groundSlamMaximumDistance)
            {
                var edgePoint = rb.position + (attackTarget - rb.position).normalized * groundSlamMaximumDistance;
                var raycastPoint = edgePoint + Vector3.up * 5;
                Ray ray = new Ray(raycastPoint, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
                {
                    edgePoint = hit.point;
                }
                attackTarget = edgePoint;
            }
            IsGroundSlamAvailable = false;
            SetRotationTarget(attackTarget);
            forceRotation = true;
            femaleWarriorAnimationManager.GroundSlam();
            //StartCoroutine(ManageAttackTrigger());
            StartCoroutine(ManageGroundSlamCooldown());
            groundSlamManager.Fire(attackTarget, GroundSlamStartDelay);
        }
    }

    private IEnumerator ManageGroundSlamCooldown()
    {
        if (characterUI != null)
        {
            characterUI.StartSkillCooldown(GroundSlamSkillNumber, groundSlamCooldown);
        }
        yield return new WaitForSeconds(groundSlamCooldown);
        forceRotation = false;
        IsGroundSlamAvailable = true;
    }

    #endregion

    #endregion

    #endregion
}
