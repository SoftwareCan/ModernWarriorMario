using System;
using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    private int gold = 0;
    [SerializeField] private UIManager uiManager; // UIManager referansý
    public static event Action<int> OnGoldChanged;

    private static GoldManager instance;

    private void Awake()
    {
        // Singleton pattern  
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arasý kalýcýlýk  
        }
        else
        {
            Destroy(gameObject);
        }

        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("GoldManager: UIManager bulunamadý!");
            }
        }
    }

    public static GoldManager Instance => instance;

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Geçersiz altýn miktarý: {amount}. Sýfýr veya negatif eklenemez!");
            return;
        }

        gold += amount;
        Debug.Log($"Altýn eklendi: {amount}, Toplam: {gold}");
        OnGoldChanged?.Invoke(gold);
        UpdateUIGold(); // UI'yi güncelle
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Geçersiz harcama miktarý: {amount}. Sýfýr veya negatif harcanamaz!");
            return false;
        }

        if (gold < amount)
        {
            Debug.LogWarning($"Yetersiz altýn! Gerekli: {amount}, Mevcut: {gold}");
            return false;
        }

        gold -= amount;
        Debug.Log($"Altýn harcandý: {amount}, Kalan: {gold}");
        OnGoldChanged?.Invoke(gold);
        UpdateUIGold(); // UI'yi güncelle
        return true;
    }

    public int GetGold()
    {
        return gold;
    }

    private void UpdateUIGold()
    {
        if (uiManager != null)
        {
            uiManager.UpdateGoldText(gold);
        }
        else
        {
            Debug.LogWarning("GoldManager: UIManager null, altýn UI güncellenemedi!");
        }
    }
}