using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private Item item; // ScriptableObject item referansý
    [SerializeField] private int quantity = 1; // Toplandýðýnda eklenecek miktar

    public void Pickup()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance null!");
            return;
        }
        if (item == null)
        {
            Debug.LogError("PickupItem: Item null!");
            return;
        }
        if (InventoryManager.Instance.AddItem(item, quantity))
        {
            gameObject.SetActive(false); // Obje sahneden kaybolur
            Debug.Log($"Item alýndý: {item.itemName}, Miktar: {quantity}");
        }
    }

    public Item GetItem()
    {
        return item;
    }
}