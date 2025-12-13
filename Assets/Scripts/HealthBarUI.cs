using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthText;
    [SerializeField] private Text healthTextShadow; // Optional shadow for better visibility
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private Color fullHealthColor = new Color(0f, 1f, 0f, 1f); // Bright green
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 1f); // Bright red
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30% health
    
    private PlayerHealth playerHealth;
    private Image healthBarImage;
    
    void Start()
    {
        // Find player health component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = player.AddComponent<PlayerHealth>();
            }
            
            playerHealth.OnHealthChanged += UpdateHealthBar;
            InitializeHealthBar();
        }
        else
        {
            Debug.LogWarning("Player not found. Make sure Player has 'Player' tag.");
        }
    }
    
    void InitializeHealthBar()
    {
        if (playerHealth == null) return;
        
        // Get health bar image component if not assigned
        if (healthBarFill == null)
        {
            healthBarFill = GetComponentInChildren<Image>();
            if (healthBarFill == null)
            {
                Debug.LogError("HealthBarUI: No Image component found. Please assign healthBarFill or add an Image component as a child.");
                return;
            }
        }
        
        healthBarImage = healthBarFill;
        healthBarImage.type = Image.Type.Filled;
        healthBarImage.fillMethod = Image.FillMethod.Horizontal;
        
        // Find health text component if not assigned
        if (showHealthText && healthText == null)
        {
            Text[] texts = GetComponentsInChildren<Text>();
            foreach (Text text in texts)
            {
                if (text.name == "HealthText" || text.name.Contains("HealthText"))
                {
                    healthText = text;
                }
                else if (text.name == "TextShadow" || text.name.Contains("Shadow"))
                {
                    healthTextShadow = text;
                }
            }
        }
        
        UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarImage == null) return;
        
        // Update fill amount (0 to 1)
        float fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        healthBarImage.fillAmount = fillAmount;
        
        // Update color based on health percentage
        if (fillAmount <= lowHealthThreshold)
        {
            healthBarImage.color = Color.Lerp(lowHealthColor, fullHealthColor, fillAmount / lowHealthThreshold);
        }
        else
        {
            healthBarImage.color = fullHealthColor;
        }
        
        // Update health text if available
        if (showHealthText && healthText != null)
        {
            string healthString = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            healthText.text = healthString;
            
            // Update shadow text if available
            if (healthTextShadow != null)
            {
                healthTextShadow.text = healthString;
            }
        }
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }
}

