using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private int slotsPerRow = 4;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        Debug.Log("InventoryUI Awake baþladý.");
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.Log("InventoryManager.Instance null, sahnede aranýyor...");
            inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
            if (inventoryManager == null)
            {
                Debug.LogError("InventoryManager bulunamadý! Lütfen sahneye InventoryManager objesi ekleyin.");
            }
            else
            {
                Debug.Log($"InventoryManager sahnede bulundu: {inventoryManager.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"InventoryManager bulundu: {inventoryManager.gameObject.name}");
        }

        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryPanel atanmamýþ!");
            inventoryPanel = gameObject;
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
        inventoryPanel.SetActive(false);
        CreateSlots();
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += UpdateUI;
        }
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= UpdateUI;
        }
    }

    private void CreateSlots()
    {
        if (slotPrefab == null || slotContainer == null)
        {
            Debug.LogError("Slot oluþturulamadý: slotPrefab veya slotContainer null!");
            return;
        }
        if (inventoryManager == null)
        {
            Debug.LogError("Slot oluþturulamadý: inventoryManager null!");
            return;
        }

        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        var slots = inventoryManager.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotContainer);
            var slotImage = slotObj.GetComponent<Image>();
            var slotUI = slotObj.GetComponent<SlotUI>();
            var icon = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            var quantityText = slotObj.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();

            if (slotImage) slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            if (slotUI) slotUI.Setup(slots[i].item, slots[i].quantity, i);
            if (icon) icon.gameObject.SetActive(false);
            if (quantityText) quantityText.text = "";
        }

        var gridLayout = slotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = slotsPerRow;
    }

    private void UpdateUI()
    {
        if (slotContainer == null || inventoryManager == null)
        {
            Debug.LogError("UpdateUI: slotContainer veya inventoryManager null!");
            return;
        }

        var slots = inventoryManager.GetSlots();
        for (int i = 0; i < slots.Count && i < slotContainer.childCount; i++)
        {
            var slotObj = slotContainer.GetChild(i).gameObject;
            var slotUI = slotObj.GetComponent<SlotUI>();
            var icon = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            var quantityText = slotObj.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();

            if (slots[i].item != null)
            {
                if (icon)
                {
                    icon.sprite = slots[i].item.icon;
                    icon.gameObject.SetActive(true);
                }
                if (quantityText)
                {
                    quantityText.text = slots[i].quantity > 1 ? slots[i].quantity.ToString() : "";
                }
                if (slotUI) slotUI.Setup(slots[i].item, slots[i].quantity, i);
            }
            else
            {
                if (icon) icon.gameObject.SetActive(false);
                if (quantityText) quantityText.text = "";
                if (slotUI) slotUI.Setup(null, 0, i);
            }
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            Debug.Log($"Envanter {(inventoryPanel.activeSelf ? "açýldý" : "kapandý")}");
        }
        else
        {
            Debug.LogError("ToggleInventory: inventoryPanel null!");
        }
    }
}
