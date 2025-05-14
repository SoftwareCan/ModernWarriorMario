using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    [SerializeField] private List<Item> marketItems = new List<Item>(); // Marketin sunduðu öðeler
    [SerializeField] private GoldManager goldManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private MarketUI marketUI;

    private static MarketManager instance;

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

        if (goldManager == null)
        {
            Debug.LogError("MarketManager: GoldManager atanmamýþ!");
        }
        if (inventoryManager == null)
        {
            Debug.LogError("MarketManager: InventoryManager atanmamýþ!");
        }
        if (marketUI == null)
        {
            Debug.LogError("MarketManager: MarketUI atanmamýþ!");
        }
    }

    public static MarketManager Instance => instance;

    public void InitializeMarketUI()
    {
        marketUI.Setup(marketItems);
    }

    public bool PurchaseItem(Item item)
    {
        if (item == null)
        {
            Debug.LogError("PurchaseItem: Item null!");
            return false;
        }

        if (goldManager.GetGold() >= item.price)
        {
            if (inventoryManager.AddItem(item, 1))
            {
                goldManager.SpendGold(item.price);
                Debug.Log($"Öðe satýn alýndý: {item.itemName}, Fiyat: {item.price}");
                return true;
            }
            else
            {
                Debug.LogWarning("Envanter dolu, öðe satýn alýnamadý!");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Yetersiz altýn! Gerekli: {item.price}, Mevcut: {goldManager.GetGold()}");
            return false;
        }
    }

    public List<Item> GetMarketItems()
    {
        return marketItems;
    }

}