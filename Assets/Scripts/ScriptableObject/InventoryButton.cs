using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InventoryUI inventoryUI;

    private void Awake()
    {
        if (inventoryUI == null)
        {
            inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null)
            {
                Debug.LogError("InventoryUI bulunamadý!");
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryUI.ToggleInventory();
    }
}
