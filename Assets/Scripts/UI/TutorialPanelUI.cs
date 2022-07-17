using UnityEngine;

/// <summary>
/// Manages the tutorial panel UI.
/// </summary>
public class TutorialPanelUI : MonoBehaviour
{
    [SerializeField] CheckBox settingsTutorialOverlayCheckbox;

    private void Awake()
    {
        settingsTutorialOverlayCheckbox.SetIsTicked(gameObject.activeSelf);
    }
    void Start()
    {
        settingsTutorialOverlayCheckbox.IsTickedChanged += OnTutorialOverlayTickedChanged;
    }

    private void OnTutorialOverlayTickedChanged()
    {
        gameObject.SetActive(settingsTutorialOverlayCheckbox.IsTicked);
    }
}
