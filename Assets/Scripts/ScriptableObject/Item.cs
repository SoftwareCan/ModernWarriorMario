using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemID; //Benzersiz Id (örnek key1)
    public string itemName; // Görünen isim (örnek kýrmýzý anahtar)
    public Sprite icon; // Itemin UI'dakli ikonu
    public bool isStackable; //Yýðýnlanabilir mi?
    public int maxStackSize; // Max yýðýn miktarý;
    public int price; //Market için 
    [TextArea] public string description;   //  Tooltip açýklamasý
    

}
