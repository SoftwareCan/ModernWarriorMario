using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventoryManager inventoryManager;
    private Image icon;
    private Item item;
    private int slotIndex;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("SlotUI: InventoryManager bulunamadý!");
        }
        icon = transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogError("SlotUI: Icon Image bulunamadý!");
        }
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(Item item, int quantity, int index)
    {
        this.item = item;
        slotIndex = index;
        if (icon != null)
        {
            if (item != null)
            {
                icon.sprite = item.icon;
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null)
        {
            Debug.Log("OnBeginDrag: Item null, sürükleme engellendi.");
            return;
        }

        originalParent = transform.parent;
        originalPosition = transform.position;
        transform.SetParent(transform.root); // Canvas’ýn köküne taþý
        canvasGroup.blocksRaycasts = false;
        Debug.Log($"Sürükleme baþladý: {item.itemName}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null) return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item == null) return;

        // Orijinal parent ve pozisyona geri dön
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(slotIndex); // Doðru sýrayý koru
        transform.position = originalPosition;
        canvasGroup.blocksRaycasts = true;

        // HintDoorManager’ý bul
        HintDoorManager hintDoorManager = Object.FindFirstObjectByType<HintDoorManager>();
        if (hintDoorManager == null)
        {
            Debug.LogError("OnEndDrag: HintDoorManager bulunamadý!");
            return;
        }

        RectTransform dropArea = hintDoorManager.GetKeyDropArea();
        if (dropArea == null)
        {
            Debug.LogError("OnEndDrag: keyDropArea null!");
            return;
        }

        // Canvas ve kamera ayarý
        Canvas canvas = dropArea.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("OnEndDrag: Canvas bulunamadý! dropArea’nýn bir Canvas parent’ý olmalý.");
            return;
        }

        Camera eventCamera = canvas.worldCamera; // WorldSpace için World Camera’yý kullan
        

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dropArea, eventData.position, eventCamera, out localPoint))
        {
            if (dropArea.rect.Contains(localPoint))
            {
                if (inventoryManager != null && inventoryManager.RemoveItem(item))
                {
                    hintDoorManager.ChangeColorAndOpenWay();
                    
                }
                else
                {
                    Debug.LogWarning("Anahtar kaldýrýlamadý!");
                }
            }
            else
            {
                Debug.Log("Anahtar keyDropArea’ya býrakýlmadý. LocalPoint: " + localPoint + ", Rect: " + dropArea.rect);
            }
        }
        else
        {
            Debug.LogWarning("ScreenPointToLocalPointInRectangle baþarýsýz! Position: " + eventData.position + ", DropArea: " + dropArea.rect + ", Camera: " + eventCamera.name);
        }
    }
}