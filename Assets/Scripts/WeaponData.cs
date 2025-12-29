using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public int damage;
    public float attackSpeed;
    [Tooltip("Sell price in coins when selling this weapon")]
    public int sellPrice = 1;
}



