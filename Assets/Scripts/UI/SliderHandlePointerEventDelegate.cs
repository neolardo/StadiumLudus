using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A delegate for fireing pointer events from a slider handle.
/// </summary>
public class SliderHandlePointerEventDelegate : MonoBehaviour, IPointerEnterHandler, IDropHandler
{
    [SerializeField]  private MainMenuUI eventReceiver;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventReceiver != null)
        {
            eventReceiver.OnDrop();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventReceiver != null)
        {
            eventReceiver.OnPointerEnter();
        }
    }
}
