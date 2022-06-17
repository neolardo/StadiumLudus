using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the cracking animation of a warrior character.
/// </summary>
public class CrackManager : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private Transform startTransform;
    [SerializeField] private Crack crackPrefab;
    [SerializeField] private float spreadSpeed;
    [SerializeField] private float closeSpeed;
    [SerializeField] private float duration;
    [SerializeField] private float openThreshold;
    [SerializeField] private float maximumRange;
    [SerializeField] private int maximumSideCrackCount;
    [SerializeField] private float maximumSideCrackRange; 
    [SerializeField] private float currentRange;
    private List<Crack> cracks;
    private List<Crack> sideCracks;
    private bool ShouldCloseCracks { get; set; } = false;
    private float UnitPerBlendShape { get; set; }


    #endregion

    #region Methods

    #region Initialize

    private void Start()
    {
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
            var crack = Instantiate(crackPrefab, transform);
            crack.gameObject.SetActive(false);
            cracks.Add(crack);
        }
        for (int i = 0; i < maximumSideCrackCount; i++)
        {
            var crack = Instantiate(crackPrefab, transform);
            crack.gameObject.SetActive(false);
            sideCracks.Add(crack);
        }
    }

    #endregion


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OpenCrack(currentRange, startTransform);
        }
    }

    public void OpenCrack(float range, Transform start, bool isSideCrack = false, int sideCrackIndex = 0)
    {
        range = Mathf.Min(range, maximumRange);
        StartCoroutine(AnimateOpenCrack(range, start, isSideCrack, sideCrackIndex));
        if (!isSideCrack)
        {
            StartCoroutine(SignalWhenCracksShouldClose());
        }
    }

    private IEnumerator SignalWhenCracksShouldClose()
    {
        ShouldCloseCracks = false;
        yield return new WaitForSeconds(duration);
        ShouldCloseCracks = true;
    }

    private List<int> GenerateRandomCornerIndexes(float range)
    {
        int count = Mathf.RoundToInt(maximumSideCrackCount * (range / maximumRange));
        int maxIndex = Mathf.FloorToInt(range / UnitPerBlendShape);
        return Globals.GenerateRandomIndexes(0, maxIndex, count);
    }

    private IEnumerator AnimateOpenCrack(float range, Transform start, bool isSideCrack, int sideCrackIndex)
    {
        Vector3 startPosition = start.position;
        int crackCount = Mathf.CeilToInt(range / crackPrefab.length);
        var localCracks = cracks;
        if (isSideCrack)
        {
            localCracks = new List<Crack>();
            localCracks.Add(sideCracks[sideCrackIndex]);
        }
        for (int i = 0; i < crackCount; i++)
        {
            var crack = localCracks[i];
            crack.transform.position = startPosition;
            crack.transform.forward = start.forward;
            startPosition += start.forward * crackPrefab.length;
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
                    localCracks[i].SetBlendShape(j, 0);
                }
                else if (!(i == 0 && j == 0 && !isSideCrack))
                {
                    if (!isSideCrack && cornerIndexes.Contains(i * unitPerCrack + j))
                    {
                        int ind = cornerIndexes.IndexOf(i * unitPerCrack + j);
                        OpenCrack(Random.Range(UnitPerBlendShape, maximumSideCrackRange), localCracks[i].cornerPoints[j], true, ind);
                    }
                    float lerp = 0;
                    while (lerp < 1)
                    {
                        localCracks[i].SetBlendShape(j, openThreshold * lerp);
                        lerp += Time.deltaTime * spreadSpeed;
                        yield return null;
                    }
                    localCracks[i].SetBlendShape(j, openThreshold);
                }
            }
        }

        yield return new WaitUntil(() => ShouldCloseCracks);
        StartCoroutine(AnimateCloseCrack(localCracks, range, isSideCrack));
    }

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
            lerp -= Time.deltaTime * closeSpeed;
        }
        foreach (var c in localCracks)
        {
            c.gameObject.SetActive(false);
        }
    }

    #endregion
}
