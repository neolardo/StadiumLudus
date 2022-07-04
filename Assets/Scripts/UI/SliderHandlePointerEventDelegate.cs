using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A delegate for fireing pointer events from a slider handle.
/// </summary>
public class SliderHandlePointerEventDelegate : MonoBehaviour, IPointerEnterHandler, IDropHandler
{
    [SerializeField]  private GameObject eventReceiver;
    private IPointerEnterReceiver pointerEnterReceiver;
    private IDropReceiver dropHandler;

    private void Start()
    {
        pointerEnterReceiver = eventReceiver.GetComponent<IPointerEnterReceiver>();
        if(pointerEnterReceiver == null)
        {
            Debug.LogWarning($"{eventReceiver.name} does not implement the {nameof(IPointerEnterReceiver)} interface. {nameof(IPointerEnterHandler)} event won't be handled.");
        }
        dropHandler = eventReceiver.GetComponent<IDropReceiver>();
        if (dropHandler == null)
        {
            Debug.LogWarning($"{eventReceiver.name} does not implement the {nameof(IDropReceiver)} interface. {nameof(IDropHandler)} event won't be handled.");
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (eventReceiver != null)
        {
            dropHandler.OnDrop();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventReceiver != null)
        {
            pointerEnterReceiver.OnPointerEnter();
        }
    }
}
