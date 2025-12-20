using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an equipped weapon slot in the UI.
/// Displays the currently equipped weapon.
/// </summary>
public class WeaponCell : MonoBehaviour
{
    public Image weaponIcon;
    public WeaponData equippedWeapon;
    
    /// <summary>
    /// Sets the equipped weapon and updates the UI.
    /// </summary>
    public void SetWeapon(WeaponData weapon)
    {
        equippedWeapon = weapon;
        
        if (weaponIcon != null)
        {
            if (weapon != null && weapon.icon != null)
            {
                weaponIcon.sprite = weapon.icon;
                weaponIcon.color = Color.white;
                weaponIcon.enabled = true;
            }
            else
            {
                weaponIcon.sprite = null;
                weaponIcon.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Clears the equipped weapon.
    /// </summary>
    public void ClearWeapon()
    {
        equippedWeapon = null;
        
        if (weaponIcon != null)
        {
            weaponIcon.sprite = null;
            weaponIcon.enabled = false;
        }
    }
}
