using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MarketUI : MonoBehaviour
{
    [SerializeField] private GameObject marketPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private int slotsPerRow = 4;
    [SerializeField] private MarketManager marketManager; // SerializeField ile atama

    private void Awake()
    {
        // marketManager'ý Inspector'dan atanan referansla kullan  
        if (marketManager == null)
        {
            // Fallback: Eðer Inspector'da atanmamýþsa, sahnede ara  
            marketManager = Object.FindFirstObjectByType<MarketManager>();
            if (marketManager == null)
            {
                Debug.LogError("MarketUI: MarketManager bulunamadý! Lütfen sahneye bir MarketManager objesi ekleyin veya Inspector'da atayýn.");
            }
        }

        if (marketPanel == null)
        {
            Debug.LogError("MarketPanel atanmamýþ!");
            marketPanel = gameObject;
        }
        if (slotPrefab == null)
        {
            Debug.LogError("SlotPrefab atanmamýþ!");
        }
        if (slotContainer == null)
        {
            Debug.LogError("SlotContainer atanmamýþ!");
            slotContainer = transform;
        }
    }

    private void Start()
    {
        marketPanel.SetActive(false);
    }

    public void Setup(List<Item> items)
    {
        if (slotContainer == null || slotPrefab == null)
        {
            Debug.LogError("Setup: slotContainer veya slotPrefab null!");
            return;
        }

        // Mevcut slotlarý temizle
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // Yeni slotlar oluþtur
        for (int i = 0; i < items.Count; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotContainer);
            var slotImage = slotObj.GetComponent<Image>();
            var icon = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            var nameText = slotObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            var priceText = slotObj.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
            var buyButton = slotObj.transform.Find("BuyButton")?.GetComponent<Button>();

            if (slotImage) slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            if (icon) icon.sprite = items[i].icon;
            if (nameText) nameText.text = items[i].itemName;
            if (priceText) priceText.text = $"{items[i].price} Gold";
            if (buyButton)
            {
                int index = i;
                buyButton.onClick.AddListener(() => OnBuyButtonClicked(items[index]));
            }
        }

        var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = slotsPerRow;
    }

    private void OnBuyButtonClicked(Item item)
    {
        if (marketManager != null && marketManager.PurchaseItem(item))
        {
            Debug.Log($"Satýn alma baþarýlý: {item.itemName}");
        }
        else
        {
            Debug.LogWarning($"Satýn alma baþarýsýz: {item.itemName}");
        }
    }

    public void ToggleMarket()
    {
        if (marketPanel != null)
        {
            bool isActive = !marketPanel.activeSelf;
            marketPanel.SetActive(isActive);
            if (isActive && marketManager != null)
            {
                marketManager.InitializeMarketUI();
            }
            Debug.Log($"Market {(isActive ? "açýldý" : "kapandý")}");
        }
        else
        {
            Debug.LogError("ToggleMarket: marketPanel null!");
        }
    }
}