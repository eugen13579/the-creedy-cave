using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages turn-based combat between player and enemies.
/// Handles combat state, turn order, and damage calculation.
/// </summary>
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    public bool IsInCombat => currentCombatState != CombatState.None;
    
    private enum CombatState
    {
        None,
        PlayerTurn,
        EnemyTurn
    }
    
    private CombatState currentCombatState = CombatState.None;
    private PlayerHealth currentPlayer;
    private EnemyHealth currentEnemy;
    private Coroutine combatCoroutine;
    
    /// <summary>
    /// Event fired when combat starts. Parameters: player, enemy.
    /// </summary>
    public event Action<PlayerHealth, EnemyHealth> OnCombatStarted;

    /// <summary>
    /// Event fired when combat ends.
    /// </summary>
    public event Action OnCombatEnded;

    /// <summary>
    /// Event fired when damage is dealt. Parameters: damage amount, isPlayerAttacking (true = player attacking enemy).
    /// </summary>
    public event Action<float, bool> OnDamageDealt;
    
    private void Awake()
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
    /// Starts a combat encounter between the player and an enemy.
    /// </summary>
    /// <param name="player">The player's health component</param>
    /// <param name="enemy">The enemy's health component</param>
    public void StartCombat(PlayerHealth player, EnemyHealth enemy)
    {
        // Prevent starting combat if already in combat
        if (IsInCombat)
        {
            return;
        }
        
        currentPlayer = player;
        currentEnemy = enemy;
        currentCombatState = CombatState.PlayerTurn;
        
        OnCombatStarted?.Invoke(player, enemy);
        
        // Start combat coroutine
        if (combatCoroutine != null)
        {
            StopCoroutine(combatCoroutine);
        }
        combatCoroutine = StartCoroutine(CombatLoop());
    }
    
    /// <summary>
    /// Ends the current combat encounter.
    /// </summary>
    public void EndCombat()
    {
        if (!IsInCombat)
        {
            return;
        }
        
        if (combatCoroutine != null)
        {
            StopCoroutine(combatCoroutine);
            combatCoroutine = null;
        }
        
        currentCombatState = CombatState.None;
        currentPlayer = null;
        currentEnemy = null;
        
        OnCombatEnded?.Invoke();
    }
    
    private IEnumerator CombatLoop()
    {
        while (IsInCombat && currentPlayer != null && currentEnemy != null)
        {
            // Check if combat should end
            if (currentPlayer.CurrentHealth <= 0 || currentEnemy.CurrentHealth <= 0)
            {
                EndCombat();
                yield break;
            }
            
            // Player turn
            if (currentCombatState == CombatState.PlayerTurn)
            {
                yield return StartCoroutine(ExecutePlayerTurn());
                yield return new WaitForSeconds(2f); // Wait for damage display
                
                // Check if enemy died
                if (currentEnemy.CurrentHealth <= 0)
                {
                    EndCombat();
                    yield break;
                }
                
                currentCombatState = CombatState.EnemyTurn;
            }
            // Enemy turn
            else if (currentCombatState == CombatState.EnemyTurn)
            {
                yield return StartCoroutine(ExecuteEnemyTurn());
                yield return new WaitForSeconds(2f); // Wait for damage display
                
                // Check if player died
                if (currentPlayer.CurrentHealth <= 0)
                {
                    EndCombat();
                    yield break;
                }
                
                currentCombatState = CombatState.PlayerTurn;
            }
        }
    }
    
    private IEnumerator ExecutePlayerTurn()
    {
        if (currentPlayer == null || currentEnemy == null) yield break;
        
        float damage = currentPlayer.AttackDamage;
        currentEnemy.TakeDamage(damage);
        
        // Notify UI to show damage
        OnDamageDealt?.Invoke(damage, true);
        
        yield return null;
    }
    
    private IEnumerator ExecuteEnemyTurn()
    {
        if (currentPlayer == null || currentEnemy == null) yield break;
        
        float damage = currentEnemy.AttackDamage;
        currentPlayer.TakeDamage(damage);
        
        // Notify UI to show damage
        OnDamageDealt?.Invoke(damage, false);
        
        yield return null;
    }
}
