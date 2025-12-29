using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDrop", menuName = "Items/Item Drop Data")]
public class ItemDropData : ScriptableObject
{
    public enum ItemType
    {
        Weapon,
        Coin
    }
    
    [Header("Item Configuration")]
    public ItemType itemType = ItemType.Weapon;
    
    [Range(0f, 1f)]
    [Tooltip("Probability of this item spawning when enemy dies (0 = never, 1 = always)")]
    public float spawnRate = 0.5f;
    
    [Header("Weapon Settings")]
    [Tooltip("Weapon data to spawn (only used when Item Type is Weapon)")]
    public WeaponData weaponData;
    
    [Header("Coin Settings")]
    [Tooltip("Coin value (optional, only used when Item Type is Coin)")]
    public int coinValue = 1;
}

