using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 2000f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float attackDamage = 150f;
    
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action<PlayerHealth, float> OnDamageTaken; // player, damage amount
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float AttackDamage => attackDamage;
    
    private bool isDead = false;
    private AudioSource audioSource;
    private AudioClip damageSound;
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Auto-subscribe to DamageNumberManager if it exists
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SubscribeToPlayer(this);
        }

        // Get or add AudioSource component for damage sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Load damage sound clip
        #if UNITY_EDITOR
        damageSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets/Sound/11_human_damage_1.wav");
        #else
        // At runtime, load from Resources folder (file needs to be moved to Resources/Assets/Sound/ for this to work)
        // Alternatively, assign clip in inspector via SerializeField
        damageSound = Resources.Load<AudioClip>("Assets/Sound/11_human_damage_1");
        #endif
    }
    
    void OnEnable()
    {
        // Subscribe when enabled (in case manager wasn't ready at Start)
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SubscribeToPlayer(this);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.UnsubscribeFromPlayer(this);
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return; // Không nhận damage nếu đã chết
        
        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnDamageTaken?.Invoke(this, damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void TakeHalfDamage()
    {
        TakeDamage(0.5f);
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void HealHalf()
    {
        Heal(0.5f);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetAttackDamage(float newAttackDamage)
    {
        attackDamage = newAttackDamage;
    }
    
    private void Die()
    {
        if (isDead) return; // Tránh gọi nhiều lần
        
        isDead = true;
        Debug.Log("Player died!");
        
        // Gọi PlayerDeath để xử lý death sequence (animation + death screen)
        PlayerDeath playerDeath = GetComponent<PlayerDeath>();
        if (playerDeath != null)
        {
            playerDeath.HandleDeath();
        }
        else
        {
            Debug.LogWarning("PlayerHealth: Không tìm thấy PlayerDeath component! Vui lòng thêm PlayerDeath vào player GameObject.");
            // Fallback: gọi trực tiếp DeathManager nếu không có PlayerDeath
            if (DeathManager.Instance != null)
            {
                DeathManager.Instance.ShowDeathScreen();
            }
        }
    }
    
    /// <summary>
    /// Reset death state (dùng khi restart)
    /// </summary>
    public void ResetDeath()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

