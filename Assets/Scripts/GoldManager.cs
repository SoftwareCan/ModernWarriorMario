using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    private int gold = 0;
    public static event Action<int>OnGoldChanged;

    private void Awake()
    {
        // �leride sahneler aras� kal�c�l�k i�in (iste�e ba�l�)
        // DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return; // negatif veya s�f�r ekleme
        gold += amount;
        Debug.Log($"Alt�n Eklendi: {amount}, Toplam: {gold}");
        OnGoldChanged?.Invoke(gold); //UI'yi g�ncellemek i�in event tetikle
    }

    public bool SpendGold(int amount)
    {
        if(amount<=0 || gold<amount)
        {
            Debug.LogWarning($"Yetersiz alt�n! Gerekli: {amount}, Mevcut: {gold}");
            return false;
        }

        gold -= amount;
        Debug.Log($"Alht�n harcand�: {amount}, Kalan: {gold}");
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public int GetGold()
    {
        return gold;
    }

}
