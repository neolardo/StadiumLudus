using UnityEngine;

/// <summary>
/// Manages the tutorial panel UI.
/// </summary>
public class TutorialPanelUI : MonoBehaviour
{
    [SerializeField] private CheckBox settingsTutorialOverlayCheckbox;
    [SerializeField] private CharacterUI characterUI;
    public bool IsVisible { get; private set; }

    public void Initialize()
    {
        settingsTutorialOverlayCheckbox.IsTickedChanged += OnTutorialOverlayTickedChanged;
        IsVisible = settingsTutorialOverlayCheckbox.IsTicked;
    }

    private void OnTutorialOverlayTickedChanged()
    {
        IsVisible = settingsTutorialOverlayCheckbox.IsTicked;
        characterUI.RefreshTutorialPanel();
    }
}
