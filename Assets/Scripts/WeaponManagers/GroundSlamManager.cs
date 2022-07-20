using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the ground slam skill of a <see cref="WarriorCharacter"/>
/// </summary>
public class GroundSlamManager: MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private AudioSource startAudioSource;
    [SerializeField] private AudioSource endAudioSource;
    [SerializeField] private Transform crackContainer;
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform rockTransform;
    [SerializeField] private Animator rockAnimator;
    [SerializeField] private ParticleSystem startCrack;
    [SerializeField] private ParticleSystem startSmoke;
    [SerializeField] private ParticleSystem endSmoke;
    [SerializeField] private Crack crackPrefab;
    [SerializeField] private float spreadDuration;
    [SerializeField] private float closeDuration;
    [SerializeField] private float openDuration;
    [SerializeField] private float openThreshold;
    [SerializeField] private float maximumRange;
    [SerializeField] private int maximumSideCrackCount;
    [SerializeField] private float maximumSideCrackRange; 
    private List<Crack> cracks;
    private List<Crack> sideCracks;
    private Vector3 startSmokePositionDelta = 0.1f * Vector3.up;
    private Vector3 startCrackPositionDelta = 0.2f * Vector3.up;
    private Vector3 endSmokePositionDelta = 0.3f * Vector3.up;
    private Vector3 endRockPositionDelta = 0.1f * Vector3.down;
    private const string RockAnimatorAppear = "Appear";
    private const string RockAnimatorDisappear = "Disappear";
    private const float RockDisableDelay = 1f;
    private const float crackSoundFadeOutDuration = .5f;

    private bool ShouldCloseCracks { get; set; } = false;
    private bool CrackDestinationReached { get; set; } = false;
    private float UnitPerBlendShape { get; set; }
    public bool IsRockVisible { get; private set; } = false;


    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
        transform.parent = null;
        UnitPerBlendShape = crackPrefab.length / (crackPrefab.blendShapeCount - 1);
        maximumSideCrackRange = Mathf.Min(maximumSideCrackRange, crackPrefab.length); // maximum one cracks per sidecrack
        InitializeCracks();
    }

    private void InitializeCracks()
    {
        cracks = new List<Crack>();
        sideCracks = new List<Crack>();
        int maximumCracks = Mathf.CeilToInt(maximumRange / crackPrefab.length);
        for (int i = 0; i < maximumCracks; i++)
        {
            var crack = Instantiate(crackPrefab, crackContainer);
            crack.gameObject.SetActive(false);
            cracks.Add(crack);
        }
        for (int i = 0; i < maximumSideCrackCount; i++)
        {
            var crack = Instantiate(crackPrefab, crackContainer);
            crack.gameObject.SetActive(false);
            sideCracks.Add(crack);
        }
    }

    #endregion

    #region Fire

    public void Fire(Vector3 target, float delaySeconds = 0f)
    {
        StartCoroutine(FireAfterDelay(target, delaySeconds));
    }

    private IEnumerator FireAfterDelay(Vector3 target, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        StartCoroutine(SignalWhenCracksShouldClose());
        AudioManager.Instance.PlayOneShotSFX(startAudioSource, SFX.GroundSlamStart);
        AudioManager.Instance.PlaySFX(startAudioSource, SFX.GroundSlamCracking);
        startCrack.transform.position = startTransform.position + startCrackPositionDelta;
        startSmoke.transform.position = startTransform.position + startSmokePositionDelta;
        startCrack.Play();
        startSmoke.Play();
        endSmoke.transform.position = target + endSmokePositionDelta;
        rockTransform.position = target + endRockPositionDelta;
        var direction = (target - startTransform.position).normalized;
        float range = (target - startTransform.position).magnitude;
        OpenCrack(startTransform.position, direction, range);
        StartCoroutine(AnimateRock());
    }

    private IEnumerator SignalWhenCracksShouldClose()
    {
        ShouldCloseCracks = false;
        yield return new WaitForSeconds(openDuration);
        ShouldCloseCracks = true;
    }

    #endregion

    #region Rock

    private IEnumerator AnimateRock()
    {
        yield return new WaitUntil(() => CrackDestinationReached);
        AudioManager.Instance.PlayOneShotSFX(endAudioSource, SFX.GroundSlamEnd);
        IsRockVisible = true;
        rockTransform.gameObject.SetActive(true);
        rockAnimator.SetTrigger(RockAnimatorAppear);
        endSmoke.Play();
        yield return new WaitUntil(() => ShouldCloseCracks);
        rockAnimator.SetTrigger(RockAnimatorDisappear);
        AudioManager.Instance.FadeOut(startAudioSource, crackSoundFadeOutDuration);
        yield return new WaitForSeconds(RockDisableDelay);
        rockTransform.gameObject.SetActive(false);
        CrackDestinationReached = false;
        IsRockVisible = false;
    }

    #endregion

    #region Crack

    #region Open

    private void OpenCrack(Vector3 start, Vector3 direction, float range, bool isSideCrack = false, int sideCrackIndex = 0)
    {
        range = Mathf.Min(range, maximumRange);
        StartCoroutine(AnimateOpenCrack(start, direction, range, isSideCrack, sideCrackIndex));
    }

    private IEnumerator AnimateOpenCrack(Vector3 start, Vector3 direction, float range, bool isSideCrack, int sideCrackIndex)
    {
        int crackCount = Mathf.CeilToInt(range / crackPrefab.length);
        var localCracks = cracks;
        if (isSideCrack)
        {
            localCracks = new List<Crack>();
            localCracks.Add(sideCracks[sideCrackIndex]);
        }
        var startPosition = start;
        for (int i = 0; i < crackCount; i++)
        {
            var crack = localCracks[i];
            crack.transform.position = startPosition;
            crack.transform.forward = direction;
            startPosition += direction * crackPrefab.length;
            for (int j = 0; j < crack.blendShapeCount; j++)
            {
                crack.SetBlendShape(j, 0);
            }
            crack.gameObject.SetActive(true);
        }
        var cornerIndexes = isSideCrack? null : GenerateRandomCornerIndexes(range);
        int unitPerCrack = Mathf.RoundToInt(crackPrefab.length / UnitPerBlendShape);
        for (int i = 0; i < crackCount; i++)
        {
            for (int j = 0; j < crackPrefab.blendShapeCount; j++)
            {
                if (i * crackPrefab.length + UnitPerBlendShape * j >= range)
                {
                    if (!isSideCrack)
                    {
                        CrackDestinationReached = true;
                    }
                    localCracks[i].SetBlendShape(j, 0);
                }
                else if (!(i == 0 && j == 0 && !isSideCrack))
                {
                    if (!isSideCrack && cornerIndexes.Contains(i * unitPerCrack + j))
                    {
                        int ind = cornerIndexes.IndexOf(i * unitPerCrack + j);
                        OpenCrack(localCracks[i].cornerPoints[j].position, localCracks[i].cornerPoints[j].forward, Random.Range(UnitPerBlendShape, maximumSideCrackRange),true, ind);
                    }
                    float lerp = 0;
                    while (lerp < 1)
                    {
                        localCracks[i].SetBlendShape(j, openThreshold * lerp);
                        lerp += Time.deltaTime / (spreadDuration / (unitPerCrack * crackCount));
                        yield return null;
                    }
                    localCracks[i].SetBlendShape(j, openThreshold);
                }
            }
        }
        yield return new WaitUntil(() => ShouldCloseCracks);
        StartCoroutine(AnimateCloseCrack(localCracks, range, isSideCrack));
    }

    private List<int> GenerateRandomCornerIndexes(float range)
    {
        int count = Mathf.RoundToInt(maximumSideCrackCount * (range / maximumRange));
        int maxIndex = Mathf.FloorToInt(range / UnitPerBlendShape);
        return Globals.GenerateRandomIndexes(0, maxIndex, count);
    }

    #endregion

    #region Close

    private IEnumerator AnimateCloseCrack(List<Crack> localCracks, float range, bool isSideCrack)
    {
        float lerp = 1;
        while (lerp > 0)
        {
            for (int i = 0; i < localCracks.Count; i++)
            {
                for (int j = 0; j < crackPrefab.blendShapeCount; j++)
                {
                    if ( !(i == 0 && j == 0 && !isSideCrack) && (i * crackPrefab.length + UnitPerBlendShape * j < range))
                    {
                        localCracks[i].SetBlendShape(j, openThreshold * lerp);
                    }
                }
            }
            yield return null;
            lerp -= Time.deltaTime / closeDuration;
        }
        foreach (var c in localCracks)
        {
            c.gameObject.SetActive(false);
        }
    }

    #endregion

    #endregion

    #endregion
}
