using UnityEngine;

public class MarketButton : MonoBehaviour
{
    [SerializeField] private MarketUI marketUI;

    private void Awake()
    {
        if (marketUI == null)
        {
            if (marketUI == null)
            {
                Debug.LogError("MarketButton: MarketUI bulunamadý!");
            }
        }
    }

    public void OnMarketButtonClicked()
    {
        marketUI.ToggleMarket();
    }
}