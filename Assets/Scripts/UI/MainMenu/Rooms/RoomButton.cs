using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a room button of the <see cref="RoomsUI"/>.
/// </summary>
public class RoomButton : MonoBehaviour
{
    #region Properties and Fields

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
            roomNameText.text = _roomInfo.Name;
            roomPlayerCountText.text = $"{_roomInfo.PlayerCount}/{Globals.MaximumPlayerCountPerRoom}";
        }
    }
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI roomPlayerCountText;
    [SerializeField] private Button button;

    public event Action<RoomInfo> Clicked;
    public event Action Hovered;

    private Color buttonNormalColor;
    private Color buttonSelectedColor;

    #endregion

    #region Methods

    private void Start()
    {
        buttonNormalColor = button.colors.normalColor;
        buttonSelectedColor = button.colors.selectedColor;
    }

    public void OnHover()
    {
        Hovered?.Invoke();
    }

    public void OnClick()
    {
        Clicked.Invoke(RoomInfo);
    }

    public void ChangeSelectionColor(bool isSelected)
    {
        var colors = button.colors;
        colors.normalColor = isSelected ? buttonSelectedColor : buttonNormalColor;
        button.colors = colors;
    }

    #endregion
}