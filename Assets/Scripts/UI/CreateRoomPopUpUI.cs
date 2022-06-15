using TMPro;
using UnityEngine;

/// <summary>
/// Manages the create room pop up UI of the rooms UI page.
/// </summary>
public class CreateRoomPopUpUI : MonoBehaviour
{
    #region Properties and Fields

    public TMP_InputField roomNameInput;
    public TMP_InputField roomPasswordInput;

    #endregion

    #region Methods

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }

    public void OnCreateRoom()
    {
        Debug.Log($"Room with Name: {roomNameInput.text}, Pass:{roomPasswordInput.text}");
    }

    #endregion
}
