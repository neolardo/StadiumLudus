using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Represents a temporary effect which enhances a character's properties in a certain way.
/// </summary>
public class Buff : MonoBehaviour
{
    #region Properties and Fields

    #region Orb Effect

    [SerializeField]
    private VisualEffect orbVFX;

    [SerializeField]
    private Transform orbTransform;

    [SerializeField]
    private Light orbLight;

    [Tooltip("The speed of the orb moving towards the target character once activated.")]
    [SerializeField]
    private float orbMovingSpeed = 3;

    [Tooltip("The speed of the orb light fading out.")]
    [SerializeField]
    private float orbFadingSpeed = 1;

    private const string orbVFXSpawnRateName = "SpawnRate";
    private int orbVFXSpawnRateValue;
    private float orbInitialIntesity;
    private Vector3 orbInitialPosition;

    #endregion

    #region Character Effect

    [SerializeField]
    private VisualEffect characterEffectVFX;

    [SerializeField]
    private Transform characterEffectTransform;

    #endregion

    private bool _isActive;

    /// <summary>
    /// Inidicates whether this buff is enabled on the target <see cref="Character"/>.
    /// </summary>
    public bool IsActive
    {
        get { return _isActive; }
        private set 
        {
            if (value != _isActive)
            {
                _isActive = value;
                if (_isActive)
                {
                    StartCoroutine(MoveOrbToTarget());
                    StartCoroutine(DeactivateAfterDurationElapsed());
                    target.AddBuff(this);
                }
                else
                {
                    ResetToInitialState();
                }
            }
        }
    }

    [Tooltip("The type of the buff effect.")]
    public BuffType type;

    [Tooltip("The mode that determines how to apply this buff on the character.")]
    public BuffApplimentMode applimentMode;

    [Tooltip("The numberic effect value of this buff which changes the property of the character.")]
    [SerializeField]
    public float effectValue;

    [Tooltip("The duration of this effect in seconds.")]
    [SerializeField]
    [Range(20, 3*60)]
    private int duration;

    /// <summary>
    /// The target <see cref="Character"/> of this effect.
    /// </summary>
    private Character target;

    private const float delayBeforeMove = 0.75f;
    private const float moveTargetHeight = 1.3f;
    private const float characterVFXSpawnRateDelay = 3;
    private const string characterVFXSpawnRateName = "SpawnRate";
    private int characterVFXSpawnRateValue;

    private bool hasInitialized;

    #endregion

    #region Methods

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
        orbVFX.SetInt(orbVFXSpawnRateName, orbVFXSpawnRateValue);
    }

    private void Initialize()
    {
        orbInitialIntesity = orbLight.intensity;
        orbInitialPosition = orbTransform.position;
        characterVFXSpawnRateValue = characterEffectVFX.GetInt(characterVFXSpawnRateName);
        characterEffectVFX.SetInt(characterVFXSpawnRateName, 0);
        orbVFXSpawnRateValue = orbVFX.GetInt(orbVFXSpawnRateName);
        hasInitialized = true;
    }

    /// <summary>
    /// Starts to use this buff on the given <see cref="Character"/>.
    /// </summary>
    /// <param name="character">The <see cref="Character"/> to apply the buff to.</param>
    public void UseOn(Character character)
    {
        target = character;
        IsActive = true;
    }

    private IEnumerator MoveOrbToTarget()
    {
        orbVFX.SetInt(orbVFXSpawnRateName, 0);
        yield return new WaitForSeconds(delayBeforeMove);
        while (((target.transform.position + moveTargetHeight * Vector3.up) - orbTransform.position).magnitude > Globals.PositionThreshold)
        {
            var dir = ((target.transform.position + moveTargetHeight * Vector3.up) - orbTransform.position).normalized;
            orbTransform.position += dir * orbMovingSpeed * Time.deltaTime;
            if (orbLight.intensity > 0)
            {
                orbLight.intensity -= orbFadingSpeed * Time.deltaTime;
            }
            yield return null;
        }
        orbTransform.gameObject.SetActive(false);
        characterEffectTransform.gameObject.SetActive(true);
        characterEffectVFX.SetInt(characterVFXSpawnRateName, characterVFXSpawnRateValue);
        StartCoroutine(StickCharacterEffectToTarget());
    }

    private IEnumerator StickCharacterEffectToTarget()
    {
        while(IsActive)
        {
            characterEffectTransform.position = target.transform.position;
            yield return null;
        }
    }

    private IEnumerator DeactivateAfterDurationElapsed()
    {
        yield return new WaitForSeconds(duration - characterVFXSpawnRateDelay);
        characterEffectVFX.SetInt(characterVFXSpawnRateName, 0);
        yield return new WaitForSeconds(characterVFXSpawnRateDelay);
        IsActive = false;
    }

    private void ResetToInitialState()
    {
        gameObject.SetActive(false);
        orbLight.intensity = orbInitialIntesity;
        orbTransform.position = orbInitialPosition;
        orbTransform.gameObject.SetActive(true);
        characterEffectTransform.gameObject.SetActive(false);
        target.RemoveBuffs();
    }

    #endregion
}
