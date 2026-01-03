using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an equipped gloves slot in the UI.
/// Displays the currently equipped gloves.
/// </summary>
public class GlovesCell : MonoBehaviour
{
    public Image glovesIcon;
    public ItemData equippedGloves;
    
    /// <summary>
    /// Sets the equipped gloves and updates the UI.
    /// </summary>
    public void SetGloves(ItemData gloves)
    {
        equippedGloves = gloves;
        
        if (glovesIcon != null)
        {
            if (gloves != null && gloves.icon != null)
            {
                glovesIcon.sprite = gloves.icon;
                glovesIcon.color = Color.white;
                glovesIcon.enabled = true;
            }
            else
            {
                glovesIcon.sprite = null;
                glovesIcon.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Clears the equipped gloves.
    /// </summary>
    public void ClearGloves()
    {
        equippedGloves = null;
        
        if (glovesIcon != null)
        {
            glovesIcon.sprite = null;
            glovesIcon.enabled = false;
        }
    }
}


