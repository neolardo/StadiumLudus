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
        set
        {
            _isTicked = value;
            tickGameObject.SetActive(_isTicked);
        }
    }

    public Action IsTickedChanged;

    #endregion

    #region Methods

    public void OnClick()
    {
        _isTicked = !_isTicked;
        IsTickedChanged?.Invoke();
    }
    public void SetIsTicked(bool value)
    {
        _isTicked = value;
    }

    #endregion
}
