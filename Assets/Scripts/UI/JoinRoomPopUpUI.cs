using TMPro;
using UnityEngine;

/// <summary>
/// Manages the join room pop up UI of the rooms UI page.
/// </summary>
public class JoinRoomPopUpUI : MonoBehaviour
{
    #region Properties and Fields

    public RoomsUI roomsUI;
    public TMP_InputField usernameInput;
    public TMP_InputField roomPasswordInput;

    #endregion

    #region Methods

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }

    public void TryJoinRoom()
    {
        roomsUI.TryJoinRoom(roomPasswordInput.text,usernameInput.text);
    }

    #endregion
}
