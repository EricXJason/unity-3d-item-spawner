using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     處理道具欄位 UI 元素的拖曳事件，將游標事件轉發給 PlacementManager。
/// </summary>
public class ItemDragSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Slot Settings")]
    [SerializeField] private PlacedItem placedItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
#if UNITY_EDITOR
        if (placedItem == null)
        {
            Debug.LogWarning("[ItemDragSlot] 尚未在 Inspector 指派 placedItem 預製物！", this);
        }
#endif
        if (placedItem == null) return;

        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.StartDragging(placedItem, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.UpdateDragging(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.EndDragging();
        }
    }
}
