using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 1000f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float attackDamage = 100f;
    
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action<EnemyHealth, float> OnDamageTaken; // enemy, damage amount
    public event Action OnDeath;
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float AttackDamage => attackDamage;
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Auto-subscribe to DamageNumberManager if it exists
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SubscribeToEnemy(this);
        }
    }
    
    void OnEnable()
    {
        // Subscribe when enabled (in case manager wasn't ready at Start)
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SubscribeToEnemy(this);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.UnsubscribeFromEnemy(this);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnDamageTaken?.Invoke(this, damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Enemy died!");
    }
}



