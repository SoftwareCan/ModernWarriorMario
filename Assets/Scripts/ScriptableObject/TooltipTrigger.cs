using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string description = "Description";

    public void OnPointerEnter(PointerEventData eventData)
    {
        string tooltipText = $"<b>{itemName}</b>\n<size=80%>{description}</size>";
        TooltipUI.Instance.ShowTooltip(tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.HideTooltip();
    }
}