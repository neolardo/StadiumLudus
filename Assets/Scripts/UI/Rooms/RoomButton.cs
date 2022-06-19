using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a room button.
/// </summary>
public class RoomButton : MonoBehaviour
{
    private Room _room;
    public Room Room
    {
        get 
        {
            return _room;
        }
        set 
        {
            _room = value;
            UpdateRoomTexts();
        }
    }
    public MainMenuUI MainMenuUI { get; set; }
    public RoomsUI RoomsUI { get; set; }
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI roomStatusText;
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
        RoomsUI.OnRoomSelected(Room);
    }

    private void UpdateRoomTexts()
    {
        roomNameText.text = Room.Name;
        roomStatusText.text = Room.Status == RoomStatus.CharacterSelection ? "character selection" : "in-game" ;
        roomPlayerCountText.text = $"{Room.Players.Count}/{Room.MaximumPlayerCount}";
    }
    public void ChangeSelectionColor(bool isSelected)
    {
        var colors = button.colors;
        colors.normalColor = isSelected ? buttonSelectedColor : buttonNormalColor;
        button.colors = colors;
    }
}
