using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDrop", menuName = "Items/Item Drop Data")]
public class ItemDropData : ScriptableObject
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Hat,
        Gloves,
        Shoes,
        Consumable,
        Coin
    }
    
    [Header("Item Configuration")]
    public ItemType itemType = ItemType.Weapon;
    
    [Range(0f, 1f)]
    [Tooltip("Probability of this item spawning when enemy dies (0 = never, 1 = always)")]
    public float spawnRate = 0.5f;
    
    [Header("Item Settings")]
    [Tooltip("Item data to spawn (used when Item Type is Weapon, Armor, Hat, Gloves, Shoes, or Consumable)")]
    public ItemData itemData;
    
    [Header("Coin Settings")]
    [Tooltip("Coin value (optional, only used when Item Type is Coin)")]
    public int coinValue = 1;
}

