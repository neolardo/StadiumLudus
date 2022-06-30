using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the end game UI.
/// </summary>
public class EndGameUI : MonoBehaviour
{
    #region Properties and Fields

    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private GameObject rematchRequestedTextGameObject;
    [SerializeField] private GameObject rematchTextGameObject;
    [SerializeField] private Button rematchButton;
    [SerializeField] private AudioSource audioSource;

    private const string WinMainText = "YOU WIN";
    private const string LoseMainText = "YOU LOSE";

    #endregion

    #region Methods

    public void SetMainText(bool win)
    {
        mainText.text = win ? WinMainText : LoseMainText;
    }

    public void OnRematchRequested()
    {
        rematchTextGameObject.SetActive(false);
        rematchButton.enabled = false;
        rematchRequestedTextGameObject.SetActive(true);
        GameRoundManager.Instance.OnLocalPlayerRequestedRematch();
    }
    public void OnExit()
    {
        PhotonNetwork.LeaveRoom();
    }

    #region Button Sounds

    public void OnButtonHover()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonHover);
    }
    public void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX(audioSource, SFX.MenuButtonClick);
    }

    #endregion

    #endregion
}
