using TMPro;
using UnityEngine;

/// <summary>
/// Manages the create room pop up UI of the rooms UI page.
/// </summary>
public class CreateRoomPopUpUI : MonoBehaviour
{
    #region Properties and Fields

    public RoomsUI roomsUI;
    public TMP_InputField usernameInput;
    public TMP_InputField roomNameInput;
    public TMP_InputField roomPasswordInput;

    #endregion

    #region Methods

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }

    public void TryCreateRoom()
    {
        roomsUI.TryCreateRoom(roomNameInput.text, roomPasswordInput.text, usernameInput.text);
    }

    #endregion
}
