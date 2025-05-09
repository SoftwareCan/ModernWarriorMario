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
        // Ýleride sahneler arasý kalýcýlýk için (isteðe baðlý)
        // DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return; // negatif veya sýfýr ekleme
        gold += amount;
        Debug.Log($"Altýn Eklendi: {amount}, Toplam: {gold}");
        OnGoldChanged?.Invoke(gold); //UI'yi güncellemek için event tetikle
    }

    public bool SpendGold(int amount)
    {
        if(amount<=0 || gold<amount)
        {
            Debug.LogWarning($"Yetersiz altýn! Gerekli: {amount}, Mevcut: {gold}");
            return false;
        }

        gold -= amount;
        Debug.Log($"Alhtýn harcandý: {amount}, Kalan: {gold}");
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public int GetGold()
    {
        return gold;
    }

}
