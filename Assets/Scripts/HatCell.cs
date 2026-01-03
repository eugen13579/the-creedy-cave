using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an equipped hat slot in the UI.
/// Displays the currently equipped hat.
/// </summary>
public class HatCell : MonoBehaviour
{
    public Image hatIcon;
    public ItemData equippedHat;
    
    /// <summary>
    /// Sets the equipped hat and updates the UI.
    /// </summary>
    public void SetHat(ItemData hat)
    {
        equippedHat = hat;
        
        if (hatIcon != null)
        {
            if (hat != null && hat.icon != null)
            {
                hatIcon.sprite = hat.icon;
                hatIcon.color = Color.white;
                hatIcon.enabled = true;
            }
            else
            {
                hatIcon.sprite = null;
                hatIcon.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Clears the equipped hat.
    /// </summary>
    public void ClearHat()
    {
        equippedHat = null;
        
        if (hatIcon != null)
        {
            hatIcon.sprite = null;
            hatIcon.enabled = false;
        }
    }
}


