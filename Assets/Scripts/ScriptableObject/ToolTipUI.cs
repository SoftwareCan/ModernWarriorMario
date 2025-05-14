using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform panelRectTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Canvas'ı parent'tan al
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Tooltip'un parent'ında Canvas bulunamadı!");
            enabled = false;
            return;
        }

        canvasRect = canvas.GetComponent<RectTransform>();
        panelRectTransform = tooltipPanel.GetComponent<RectTransform>();

        // Pivot ve anchor'ı ayarla
        panelRectTransform.pivot = new Vector2(0, 1);
        panelRectTransform.anchorMin = new Vector2(0, 1);
        panelRectTransform.anchorMax = new Vector2(0, 1);

        HideTooltip();
    }

    private void Update()
    {
        if (!tooltipPanel.activeSelf || canvas == null) return;

        // Fare pozisyonunu canvas koordinatlarına çevir
        Vector2 localPoint;
        bool isValid = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        if (!isValid) return;

        // Canvas ölçeklendirmesini dikkate al
        localPoint /= canvas.scaleFactor;

        // Fareye göre offset
        Vector2 offset = new Vector2(15f, -15f); // Sağ üstte
        Vector2 anchoredPos = localPoint + offset;

        // Sınır kontrolleri
        Vector2 panelSize = panelRectTransform.sizeDelta;
        Vector2 canvasSize = canvasRect.sizeDelta;

        anchoredPos.x = Mathf.Clamp(anchoredPos.x, -canvasSize.x / 2, canvasSize.x / 2 - panelSize.x);
        anchoredPos.y = Mathf.Clamp(anchoredPos.y - panelSize.y, -canvasSize.y / 2, canvasSize.y / 2 - panelSize.y);

        panelRectTransform.anchoredPosition = anchoredPos;
    }

    public void ShowTooltip(string content)
    {
        tooltipText.text = content;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}