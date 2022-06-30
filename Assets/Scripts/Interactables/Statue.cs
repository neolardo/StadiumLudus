using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages an interactable statue.
/// </summary>
public class Statue : Interactable
{
    #region Properties and Fields

    [Tooltip("The distance of the range where a player can interact with the statue.")]
    [SerializeField]
    private float interactionRangeDistance = 1f;

    [Tooltip("The delay before the first spawn buff.")]
    [SerializeField]
    [Range(5, 5 * 60)]
    private int firstBuffSpawnDelay;

    [Tooltip("The delay between the deactivation of the last buff and a new one being spawned.")]
    [SerializeField]
    [Range(5, 2*60)]
    private int buffSpawnDelay;

    [Tooltip("The list of buffs that can be spawned by this statue.")]
    [SerializeField]
    private List<Buff> buffs;

    /// <summary>
    /// The current buff which is available or active.
    /// </summary>
    private Buff currentBuff;

    /// <summary>
    /// True if a buff is available to use.
    /// </summary>
    private bool IsBuffAvailable { get; set; }

    #endregion

    #region Methods

    private void Start()
    {
        StartCoroutine(SpawnFirstBuff());
    }

    private IEnumerator SpawnFirstBuff()
    {
        yield return new WaitForSeconds(firstBuffSpawnDelay);
        StartBuffSpawning();
    }

    private void StartBuffSpawning()
    {
        if (PhotonView.IsMine)
        {
            var index = Random.Range(0, buffs.Count);
            PhotonView.RPC(nameof(SpawnBuff), RpcTarget.All, index);
        }
    }

    [PunRPC]
    public void SpawnBuff(int buffIndex)
    {
        IsBuffAvailable = true;
        currentBuff = buffs[buffIndex];
        currentBuff.gameObject.SetActive(true);
    }

    private IEnumerator WaitUntilCurrentBuffDeactivatedAndSpawnNewOne()
    {
        yield return new WaitUntil(() => !currentBuff.IsActive);
        yield return new WaitForSeconds(buffSpawnDelay);
        StartBuffSpawning();
    }

    /// <inheritdoc/>
    public override Vector3 GetClosestInteractionPoint(Vector3 point)
    {
        var dir = (point - transform.position).normalized;
        var distance = (point - transform.position).magnitude;
        return distance > interactionRangeDistance ? transform.position + dir * interactionRangeDistance : point;
    }

    [PunRPC]
    /// <inheritdoc/>
    public override bool TryInteract(int characterPhotonViewID)
    {
        if (IsBuffAvailable)
        {
            IsBuffAvailable = false;
            var character = GameRoundManager.Instance.LocalCharacterReferenceDictionary[characterPhotonViewID];
            character.KneelBeforeStatue(transform.position);
            currentBuff.UseOn(character);
            StartCoroutine(WaitUntilCurrentBuffDeactivatedAndSpawnNewOne());
            return true;
        }
        return false;
    }

    #endregion
}
