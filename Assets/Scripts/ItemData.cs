using UnityEngine;

/// <summary>
/// ScriptableObject that defines item properties including type, stats, and economy values.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Hat,
        Gloves,
        Shoes,
        Consumable
    }
    
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public ItemType itemType = ItemType.Weapon;
    
    [Header("Weapon Stats")]
    [Tooltip("Damage dealt by this weapon (only used when ItemType is Weapon)")]
    public int damage;
    [Tooltip("Attack speed for this weapon (only used when ItemType is Weapon)")]
    public float attackSpeed;
    
    [Header("Defense Stats")]
    [Tooltip("Defense value provided by this item (used for Armor, Hat, Gloves, Shoes)")]
    public int defense;
    
    [Header("Economy")]
    [Tooltip("Sell price in coins when selling this item")]
    public int sellPrice = 1;
}

