using UnityEngine;

/// <summary>
/// Manages player currency (coins) - collection, spending, and balance tracking.
/// </summary>
public class CoinManager : MonoBehaviour
{
    [SerializeField] private int coinCount = 0;

    /// <summary>
    /// Gets or sets the current coin count.
    /// </summary>
    public int CoinCount
    {
        get => coinCount;
        set => coinCount = Mathf.Max(0, value); // Ensure coin count is never negative
    }

    /// <summary>
    /// Called when a coin is collected. Increments the coin count.
    /// </summary>
    public void CollectCoin()
    {
        coinCount++;
    }
    
    /// <summary>
    /// Attempts to spend coins. Returns true if successful, false if insufficient coins.
    /// </summary>
    /// <param name="amount">Amount of coins to spend</param>
    /// <returns>True if coins were spent successfully, false if insufficient coins</returns>
    public bool SpendCoins(int amount)
    {
        if (coinCount >= amount)
        {
            coinCount -= amount;
            return true;
        }
        return false;
    }
}
