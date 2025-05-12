using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public Item item; // Slot’taki item
        public int quantity; // Item miktarý
    }

    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int maxSlots = 16;

    private static InventoryManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Slotlarý baþlat
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot { item = null, quantity = 0 });
        }
    }

    public static InventoryManager Instance => instance;

    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogError("AddItem: Item null!");
            return false;
        }

        // Önce yýðýnlanabilir item’ý kontrol et
        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.quantity < item.maxStackSize)
                {
                    slot.quantity += quantity;
                    if (slot.quantity > item.maxStackSize)
                    {
                        int excess = slot.quantity - item.maxStackSize;
                        slot.quantity = item.maxStackSize;
                        AddItem(item, excess);
                    }
                    Debug.Log($"Item eklendi: {slot.item.itemName}, Miktar: {slot.quantity}");
                    OnInventoryChanged?.Invoke(); // Null-safe event çaðrýsý
                    return true;
                }
            }
        }

        // Boþ slot ara
        foreach (var slot in slots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.quantity = quantity;
                Debug.Log($"Item eklendi: {item.itemName}, Miktar: {slot.quantity}");
                OnInventoryChanged?.Invoke(); // Null-safe event çaðrýsý
                return true;
            }
        }

        Debug.LogWarning($"Envanter dolu! Item eklenemedi: {item.itemName}");
        return false;
    }

    public bool RemoveItem(Item item, int quantity = 1)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.quantity >= quantity)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                {
                    slot.item = null;
                    slot.quantity = 0;
                }
                Debug.Log($"Item çýkarýldý: {item.itemName}, Kalan: {slot.quantity}");
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"Item bulunamadý veya yeterli miktarda deðil: {item.itemName}");
        return false;
    }

    public List<InventorySlot> GetSlots()
    {
        return slots;
    }

    public event System.Action OnInventoryChanged;

    public bool HasItem(Item item, int quantity = 1)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.quantity >= quantity)
            {
                return true;
            }
        }
        return false;
    }
}