using System;
using UnityEngine;

/// <summary>
/// Manages a checkbox UI element.
/// </summary>
public class CheckBox : MonoBehaviour
{
    #region Properties and Fields

    [SerializeField] private GameObject tickGameObject;

    private bool _isTicked;
    public bool IsTicked
    {
        get { return _isTicked; }
        private set
        {
            _isTicked = value;
            tickGameObject.SetActive(_isTicked);
            IsTickedChanged?.Invoke();
        }
    }

    public Action IsTickedChanged;

    #endregion

    #region Methods

    public void OnClick()
    {
        IsTicked = !IsTicked;
    }
    public void SetIsTicked(bool value)
    {
        IsTicked = value;
    }

    #endregion
}
