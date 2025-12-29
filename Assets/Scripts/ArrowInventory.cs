using UnityEngine;

/// <summary>
/// Singleton to track arrow count for ranged combat.
/// </summary>
public class ArrowInventory : MonoBehaviour
{
    public static ArrowInventory Instance { get; private set; }
    
    [SerializeField] private int arrowCount = 0;
    
    public int ArrowCount => arrowCount;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Adds arrows to the inventory.
    /// </summary>
    /// <param name="count">Number of arrows to add</param>
    public void AddArrows(int count)
    {
        arrowCount += count;
        Debug.Log($"Added {count} arrows. Total: {arrowCount}");
    }
    
    /// <summary>
    /// Uses one arrow. Returns true if arrow was used, false if no arrows available.
    /// </summary>
    /// <returns>True if arrow was used, false if no arrows available</returns>
    public bool UseArrow()
    {
        if (arrowCount > 0)
        {
            arrowCount--;
            Debug.Log($"Used 1 arrow. Remaining: {arrowCount}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Gets the current arrow count.
    /// </summary>
    /// <returns>Current number of arrows</returns>
    public int GetArrowCount()
    {
        return arrowCount;
    }
    
    /// <summary>
    /// Checks if player has arrows available.
    /// </summary>
    /// <returns>True if arrows are available</returns>
    public bool HasArrows()
    {
        return arrowCount > 0;
    }
}


