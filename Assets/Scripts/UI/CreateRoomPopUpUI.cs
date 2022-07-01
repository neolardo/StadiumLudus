using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private EventSystem system;

    #endregion

    #region Methods


    private void Start()
    {
        system = EventSystem.current;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                TryCreateRoom();
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                if (next != null)
                {
                    InputField inputfield = next.GetComponent<InputField>();
                    if (inputfield != null)
                    {
                        inputfield.OnPointerClick(new PointerEventData(system));
                    }
                    system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                }
            }
        }

    }
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
