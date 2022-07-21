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

    [SerializeField]
    private AudioSource audioSource;

    #region Orb Effect

    [SerializeField]
    private VisualEffect orbVFX;

    [SerializeField]
    private Transform orbTransform;

    [SerializeField]
    private Light orbLight;

    [Tooltip("The speed of the orb light fading out.")]
    [SerializeField]
    private float orbFadingSpeed = 1;

    private const string orbVFXSpawnRateName = "SpawnRate";
    private int orbVFXSpawnRateValue;

    private const string orbVFXSizeValueName = "Size";
    private const float orbVFXSizeTargetValue = .03f;
    private float orbVFXSizeInitialValue;


    private const string orbVFXAttractionSpeedName = "AttractionSpeed";
    private const float orbVFXAttractionSpeedTargetValue = 30;
    private float orbVFXAttractionSpeedInitialValue;

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
                    StartCoroutine(FadeOutOrbLight());
                    StartCoroutine(DeactivateAfterDurationElapsed());
                    StartCoroutine(TryAddBuffToTargetAfterDelay());
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
    [Range(20, 3 * 60)]
    private int duration;

    /// <summary>
    /// The target <see cref="Character"/> of this effect.
    /// </summary>
    private Character target;

    private const float delayBeforeActivation = 2f;
    private const float delayBeforeMove = 0.75f;
    private const float moveDuration = 1.5f;
    private Vector3 moveTargetDelta = 1.3f * Vector3.up;
    private const float characterVFXSpawnRateDelay = 3;
    private const string characterVFXSpawnRateName = "SpawnRate";
    private int characterVFXSpawnRateValue;

    private bool hasInitialized;

    /// <summary>
    /// A helper boolean which indicates whether the last orb light fade was out or in.
    /// </summary>
    private bool wasLastFadeOut;

    #endregion

    #region Methods

    #region Initialize

    private void OnEnable()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
        audioSource.loop = true;
        AudioManager.Instance.PlaySFX(audioSource, SFX.BuffIdle);
        orbVFX.SetInt(orbVFXSpawnRateName, orbVFXSpawnRateValue);
        StartCoroutine(FadeInOrbLight());
    }

    private void Initialize()
    {
        orbInitialIntesity = orbLight.intensity;
        orbLight.intensity = 0;
        orbInitialPosition = orbTransform.position;
        characterVFXSpawnRateValue = characterEffectVFX.GetInt(characterVFXSpawnRateName);
        characterEffectVFX.SetInt(characterVFXSpawnRateName, 0);
        orbVFXSizeInitialValue = orbVFX.GetFloat(orbVFXSizeValueName);
        orbVFXAttractionSpeedInitialValue = orbVFX.GetFloat(orbVFXAttractionSpeedName);
        orbVFXSpawnRateValue = orbVFX.GetInt(orbVFXSpawnRateName);
        hasInitialized = true;
    }

    #endregion

    #region Apply

    /// <summary>
    /// Starts to use this buff on the given <see cref="Character"/>.
    /// </summary>
    /// <param name="character">The <see cref="Character"/> to apply the buff to.</param>
    public void UseOn(Character character)
    {
        target = character;
        IsActive = true;
    }

    private IEnumerator TryAddBuffToTargetAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeActivation);
        target.AddBuff(this);
    }

    #endregion

    #region Orb

    private IEnumerator MoveOrbToTarget()
    {
        orbVFX.SetInt(orbVFXSpawnRateName, 0);
        orbVFX.SetFloat(orbVFXSizeValueName, orbVFXSizeTargetValue);
        orbVFX.SetFloat(orbVFXAttractionSpeedName, orbVFXAttractionSpeedTargetValue);
        yield return new WaitForSeconds(delayBeforeMove);
        float elapsedSeconds = 0;
        while (elapsedSeconds < moveDuration)
        {
            orbTransform.position = Vector3.Lerp(orbInitialPosition, (target.transform.position + moveTargetDelta), elapsedSeconds / moveDuration);
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        orbTransform.gameObject.SetActive(false);
        characterEffectTransform.gameObject.SetActive(true);
        characterEffectVFX.SetInt(characterVFXSpawnRateName, characterVFXSpawnRateValue);
        AudioManager.Instance.Stop(audioSource);
        audioSource.loop = false;
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.BuffUse);
        StartCoroutine(StickCharacterEffectToTarget());
    }

    private IEnumerator FadeOutOrbLight()
    {
        wasLastFadeOut = true;
        while (orbLight.intensity > 0 && wasLastFadeOut)
        {
            orbLight.intensity -= orbFadingSpeed * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeInOrbLight()
    {
        wasLastFadeOut = false;
        while (orbLight.intensity < orbInitialIntesity && !wasLastFadeOut)
        {
            orbLight.intensity += orbFadingSpeed * Time.deltaTime;
            yield return null;
        }
        orbLight.intensity = orbInitialIntesity;
    }

    #endregion

    #region Character Effect

    private IEnumerator StickCharacterEffectToTarget()
    {
        while (IsActive)
        {
            characterEffectTransform.position = target.transform.position;
            yield return null;
        }
    }

    #endregion

    #region Deactivate

    private IEnumerator DeactivateAfterDurationElapsed()
    {
        if (IsActive)
        {
            yield return new WaitForSeconds(duration - characterVFXSpawnRateDelay);
        }
        if (IsActive)
        {
            characterEffectVFX.SetInt(characterVFXSpawnRateName, 0);
        }
        if (IsActive)
        {
            yield return new WaitForSeconds(characterVFXSpawnRateDelay);
            IsActive = false;
        }
    }

    public void ForceDeactivate()
    {
        if (IsActive)
        {
            StartCoroutine(ForceDeactivateAfterDelay());
        }
    }

    private IEnumerator ForceDeactivateAfterDelay()
    {
        if (IsActive)
        {
            characterEffectVFX.SetInt(characterVFXSpawnRateName, 0);
            yield return new WaitForSeconds(characterVFXSpawnRateDelay);
        }
        IsActive = false;
    }

    #endregion

    #region Reset

    private void ResetToInitialState()
    {
        gameObject.SetActive(false);
        orbTransform.position = orbInitialPosition;
        orbTransform.gameObject.SetActive(true);
        characterEffectTransform.gameObject.SetActive(false);
        orbVFX.SetFloat(orbVFXSizeValueName, orbVFXSizeInitialValue);
        orbVFX.SetFloat(orbVFXAttractionSpeedName, orbVFXAttractionSpeedInitialValue);
        target.RemoveBuffs();
    }

    #endregion

    #endregion
}
