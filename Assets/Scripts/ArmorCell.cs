using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an equipped armor slot in the UI.
/// Displays the currently equipped armor.
/// </summary>
public class ArmorCell : MonoBehaviour
{
    public Image armorIcon;
    public ItemData equippedArmor;
    
    /// <summary>
    /// Sets the equipped armor and updates the UI.
    /// </summary>
    public void SetArmor(ItemData armor)
    {
        equippedArmor = armor;
        
        if (armorIcon != null)
        {
            if (armor != null && armor.icon != null)
            {
                armorIcon.sprite = armor.icon;
                armorIcon.color = Color.white;
                armorIcon.enabled = true;
            }
            else
            {
                armorIcon.sprite = null;
                armorIcon.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Clears the equipped armor.
    /// </summary>
    public void ClearArmor()
    {
        equippedArmor = null;
        
        if (armorIcon != null)
        {
            armorIcon.sprite = null;
            armorIcon.enabled = false;
        }
    }
}


