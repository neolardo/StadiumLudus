using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the end game UI.
/// </summary>
public class EndGameUI : MonoBehaviour
{
    #region Properties and Fields

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject rematchRequestedTextGameObject;
    [SerializeField] private GameObject rematchTextGameObject;
    [SerializeField] private Button rematchButton;
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;

    private const string WinMainText = "YOU WIN";
    private const string LoseMainText = "YOU LOSE";
    private const float titleFadeDelay = 0.5f;
    private const float titleFadeDuration = 1f;
    private const float buttonsFadeDelay = .5f;
    private const float buttonsFadeDuration = 1f;

    #endregion

    #region Methods

    #region Show 

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        StartCoroutine(FadeInTitleAndButtons());
    }

    private IEnumerator FadeInTitleAndButtons()
    {
        yield return new WaitForSeconds(titleFadeDelay);
        while (titleCanvasGroup.alpha < 1)
        {
            titleCanvasGroup.alpha += Time.deltaTime / titleFadeDuration;
            yield return null;
        }
        titleCanvasGroup.alpha = 1;
        yield return new WaitForSeconds(buttonsFadeDelay);
        buttonsCanvasGroup.gameObject.SetActive(true);
        while (buttonsCanvasGroup.alpha < 1)
        {
            buttonsCanvasGroup.alpha += Time.deltaTime / buttonsFadeDuration;
            yield return null;
        }
        buttonsCanvasGroup.alpha = 1;
    }

    public void SetTitleText(bool win)
    {
        titleText.text = win ? WinMainText : LoseMainText;
    }

    #endregion

    #region Handle Button Clicks

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

    #endregion

    #endregion
}
