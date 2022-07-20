using UnityEngine;

/// <summary>
/// Manages the tutorial panel UI.
/// </summary>
public class TutorialPanelUI : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField] private CheckBox settingsTutorialOverlayCheckbox;
    public bool IsEnabled { get; private set; }

    #endregion

    #region Methods

    private void Awake()
    {
        settingsTutorialOverlayCheckbox.IsTickedChanged += OnTutorialOverlayTickedChanged;
        IsEnabled = settingsTutorialOverlayCheckbox.IsTicked;
        if (!IsEnabled)
        {
            Hide();
        }
    }

    private void OnTutorialOverlayTickedChanged()
    {
        IsEnabled = settingsTutorialOverlayCheckbox.IsTicked;
    }

    #region Show Hide

    public void ShowIfEnabled()
    {
        if (IsEnabled)
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false); 
    }

    #endregion

    #endregion
}
