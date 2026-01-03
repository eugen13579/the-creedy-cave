using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an equipped shoes slot in the UI.
/// Displays the currently equipped shoes.
/// </summary>
public class ShoesCell : MonoBehaviour
{
    public Image shoesIcon;
    public ItemData equippedShoes;
    
    /// <summary>
    /// Sets the equipped shoes and updates the UI.
    /// </summary>
    public void SetShoes(ItemData shoes)
    {
        equippedShoes = shoes;
        
        if (shoesIcon != null)
        {
            if (shoes != null && shoes.icon != null)
            {
                shoesIcon.sprite = shoes.icon;
                shoesIcon.color = Color.white;
                shoesIcon.enabled = true;
            }
            else
            {
                shoesIcon.sprite = null;
                shoesIcon.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Clears the equipped shoes.
    /// </summary>
    public void ClearShoes()
    {
        equippedShoes = null;
        
        if (shoesIcon != null)
        {
            shoesIcon.sprite = null;
            shoesIcon.enabled = false;
        }
    }
}


