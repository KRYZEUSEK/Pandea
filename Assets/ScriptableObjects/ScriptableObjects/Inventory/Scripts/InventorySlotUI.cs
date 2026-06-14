
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int slotIndex; // indeks slotu w ekwipunku
    public DisplayInventory displayInventory;

    private CanvasGroup canvasGroup;
    public InventorySlot currentSlot;
    private bool isDragging = false;


    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot == null || currentSlot.item == null)
            return;

        if (displayInventory == null) return;

        isDragging = true; // przeciąganie rozpoczęte
        displayInventory.StartDrag(slotIndex);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
    }




    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return; // blokujemy ruch pustych slotów
        transform.position = eventData.position;
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return; // blokujemy reset pustych slotów
        isDragging = false;

        displayInventory.EndDrag();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }


    public void OnDrop(PointerEventData eventData)
    {

        if (displayInventory == null)
        {
            Debug.LogWarning("displayInventory jest null w OnBeginDrag");
            return;
        }

        displayInventory.SwapSlots(displayInventory.draggedSlotIndex, slotIndex);
    }
}
