using Photon.Pun;
using System.Collections;
using UnityEngine;
/// <summary>
/// An abstract class for any object that is interactable for the player.
/// </summary>
public abstract class Interactable : MonoBehaviour, IHighlightable
{
    #region Fields And Properties
    public PhotonView PhotonView { get; private set; }
    [SerializeField] private Highlight highlight;
    private float lastHighlightTriggerElapsedSeconds = Globals.HighlightDelay * 2;
    #endregion

    #region Methods

    public void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        StartCoroutine(HighlightOnTriggered());
    }

    #region Outline

    public void Highlight()
    {
        lastHighlightTriggerElapsedSeconds = 0;
    }

    private IEnumerator HighlightOnTriggered()
    {
        highlight.enabled = false;
        while (true)
        {
            yield return new WaitUntil(() => lastHighlightTriggerElapsedSeconds < Globals.HighlightDelay);
            while (lastHighlightTriggerElapsedSeconds < Globals.HighlightDelay)
            {
                highlight.enabled = true;
                lastHighlightTriggerElapsedSeconds += Time.deltaTime;
                yield return null;
            }
            highlight.enabled = false;
        }
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Gets the cloeset interaction point to a relative point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The cloeset interaction point to a relative point./returns>
    public abstract Vector3 GetClosestInteractionPoint(Vector3 point);


    [PunRPC]
    /// <summary>
    /// Tries to interract with an <see cref="Interactable"/> element.
    /// </summary>
    /// <param name="characterPhotonViewID">The <see cref="PhotonView"/> ID of the <see cref="Character"/> which tries to interract.</param>
    /// <returns>True, if the interaction was successful, otherwise false.</returns>
    public abstract bool TryInteract(int characterPhotonViewID);


    #endregion


    #endregion
}
