using TMPro;
using UnityEngine;

/// <summary>
/// Manages a player name UI of the <see cref="CharacterSelectionUI"/> page.
/// </summary>
public class PlayerNameUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private TextMeshProUGUI playerText; 
    [SerializeField] private GameObject tick;
    public string PlayerName => playerText.text;

    #endregion

    #region Methods

    public void SetPlayerText(string playerName)
    {
        playerText.text = playerName;
    }

    public void SetIsCharacterConfirmed(bool isCharacterConfirmed)
    {
        tick.SetActive(isCharacterConfirmed);
    }

    #endregion
}
