using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a room button.
/// </summary>
public class RoomButton : MonoBehaviour
{
    private RoomInfo _roomInfo;
    public RoomInfo RoomInfo
    {
        get 
        {
            return _roomInfo;
        }
        set 
        {
            _roomInfo = value;
            UpdateRoomTexts();
        }
    }
    public MainMenuUI MainMenuUI { get; set; }
    public RoomsUI RoomsUI { get; set; }
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI roomPlayerCountText;
    public Button button;

    private Color buttonNormalColor;
    private Color buttonSelectedColor;

    private void Start()
    {
        buttonNormalColor = button.colors.normalColor;
        buttonSelectedColor = button.colors.selectedColor;
    }

    public void OnHover()
    {
        MainMenuUI.OnMenuButtonHover();
    }

    public void OnClick()
    {
        MainMenuUI.OnMenuButtonClick();
        RoomsUI.OnRoomSelected(RoomInfo);
    }

    private void UpdateRoomTexts()
    {
        roomNameText.text = RoomInfo.Name;
        roomPlayerCountText.text = $"{RoomInfo.PlayerCount}/{Globals.MaximumPlayerCountPerRoom}";
    }

    public void ChangeSelectionColor(bool isSelected)
    {
        var colors = button.colors;
        colors.normalColor = isSelected ? buttonSelectedColor : buttonNormalColor;
        button.colors = colors;
    }
}
