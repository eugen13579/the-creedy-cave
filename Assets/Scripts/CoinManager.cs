using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public int coinCount = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
