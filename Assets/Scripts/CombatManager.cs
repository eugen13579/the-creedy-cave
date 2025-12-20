using UnityEngine;
using System.Collections;
using System;

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
    
    public event Action<PlayerHealth, EnemyHealth> OnCombatStarted;
    public event Action OnCombatEnded;
    public event Action<float, bool> OnDamageDealt; // damage, isPlayerAttacking (true = player attacking enemy)
    
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
    
    public void EndCombat()
    {
        if (!IsInCombat) return;
        
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


