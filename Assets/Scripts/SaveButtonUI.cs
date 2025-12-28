using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Runtime UI component for the save button.
/// Handles save button click and displays save slot selection.
/// </summary>
public class SaveButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private GameObject saveSlotPanel;
    [SerializeField] private Transform slotButtonContainer;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private TextMeshProUGUI panelTitleText;
    [SerializeField] private GameObject slotButtonPrefab;
    
    private const int MAX_SAVE_SLOTS = 5;
    private enum PanelMode { Save, Load }
    private PanelMode currentMode = PanelMode.Save;
    
    void Start()
    {
        // Setup save button
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }
        
        // Setup load button
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
        
        // Setup close button
        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(CloseSaveSlotPanel);
        }
        
        // Initially hide the panel
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when the save button is clicked.
    /// </summary>
    void OnSaveButtonClicked()
    {
        ShowSaveSlotPanel();
    }
    
    /// <summary>
    /// Shows the save slot selection panel in Save mode.
    /// </summary>
    void ShowSaveSlotPanel()
    {
        currentMode = PanelMode.Save;
        ShowSlotPanel();
    }
    
    /// <summary>
    /// Shows the load slot selection panel in Load mode.
    /// </summary>
    void ShowLoadSlotPanel()
    {
        currentMode = PanelMode.Load;
        ShowSlotPanel();
    }
    
    /// <summary>
    /// Shows the slot panel with appropriate mode.
    /// </summary>
    void ShowSlotPanel()
    {
        if (saveSlotPanel == null) return;
        
        // Update title based on mode
        if (panelTitleText != null)
        {
            panelTitleText.text = currentMode == PanelMode.Save ? "Select Save Slot" : "Select Load Slot";
        }
        
        saveSlotPanel.SetActive(true);
        
        // Clear existing slot buttons
        if (slotButtonContainer != null)
        {
            foreach (Transform child in slotButtonContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create slot buttons based on mode
            for (int i = 1; i <= MAX_SAVE_SLOTS; i++)
            {
                if (currentMode == PanelMode.Save)
                {
                    CreateSaveSlotButton(i);
                }
                else
                {
                    CreateLoadSlotButton(i);
                }
            }
        }
    }
    
    /// <summary>
    /// Closes the save slot selection panel.
    /// </summary>
    void CloseSaveSlotPanel()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when the load button is clicked.
    /// </summary>
    void OnLoadButtonClicked()
    {
        ShowLoadSlotPanel();
    }
    
    /// <summary>
    /// Creates a button for saving to a specific slot.
    /// </summary>
    /// <param name="slot">Slot number (1-5)</param>
    void CreateSaveSlotButton(int slot)
    {
        if (slotButtonContainer == null) return;
        
        GameObject buttonObj = new GameObject($"SaveSlot{slot}Button");
        buttonObj.transform.SetParent(slotButtonContainer, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400, 50);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.5f, 0.8f, 1f); // Blue button
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.6f, 0.9f, 1f);
        colors.pressedColor = new Color(0.2f, 0.4f, 0.7f, 1f);
        button.colors = colors;
        
        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        
        // Get save slot info
        bool exists = SaveSystem.SaveExists(slot);
        string statusText = exists ? " (Exists)" : " (Empty)";
        
        if (exists)
        {
            try
            {
                string filePath = SaveSystem.GetSaveFilePath(slot);
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                    if (saveData != null && !string.IsNullOrEmpty(saveData.saveTimestamp))
                    {
                        statusText = $" (Saved: {saveData.saveTimestamp})";
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
        
        buttonText.text = $"Save Slot {slot}{statusText}";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Add click listener for save
        int slotNumber = slot; // Capture for closure
        button.onClick.AddListener(() => OnSaveSlotButtonClicked(slotNumber, exists));
    }
    
    /// <summary>
    /// Creates a button for loading from a specific slot.
    /// </summary>
    /// <param name="slot">Slot number (1-5)</param>
    void CreateLoadSlotButton(int slot)
    {
        if (slotButtonContainer == null) return;
        
        bool exists = SaveSystem.SaveExists(slot);
        
        // Only show slots that have saves
        if (!exists) return;
        
        GameObject buttonObj = new GameObject($"LoadSlot{slot}Button");
        buttonObj.transform.SetParent(slotButtonContainer, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400, 50);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.5f, 0.8f, 0.3f, 1f); // Green button for load
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.5f, 0.8f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.6f, 0.9f, 0.4f, 1f);
        colors.pressedColor = new Color(0.4f, 0.7f, 0.2f, 1f);
        button.colors = colors;
        
        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        
        // Get save slot info
        string statusText = "";
        string sceneName = "";
        
        try
        {
            string filePath = SaveSystem.GetSaveFilePath(slot);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData != null)
                {
                    if (!string.IsNullOrEmpty(saveData.saveTimestamp))
                    {
                        statusText = $" - {saveData.saveTimestamp}";
                    }
                    if (!string.IsNullOrEmpty(saveData.currentSceneName))
                    {
                        sceneName = $" ({saveData.currentSceneName})";
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        
        buttonText.text = $"Load Slot {slot}{statusText}{sceneName}";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Add click listener for load
        int slotNumber = slot; // Capture for closure
        button.onClick.AddListener(() => OnLoadSlotButtonClicked(slotNumber));
    }
    
    /// <summary>
    /// Called when a save slot button is clicked.
    /// </summary>
    /// <param name="slot">Slot number</param>
    /// <param name="exists">Whether save file exists</param>
    void OnSaveSlotButtonClicked(int slot, bool exists)
    {
        // If save exists, we'll just overwrite (confirmation can be added later)
        if (exists)
        {
            Debug.Log($"Save slot {slot} already exists. Overwriting...");
        }
        
        // Perform the save
        bool success = SaveSystem.SaveGame(slot);
        
        if (success)
        {
            Debug.Log($"Game saved successfully to slot {slot}!");
            CloseSaveSlotPanel();
            
            // Refresh the panel to show updated info
            ShowSaveSlotPanel();
        }
        else
        {
            Debug.LogError($"Failed to save game to slot {slot}!");
        }
    }
    
    /// <summary>
    /// Called when a load slot button is clicked.
    /// </summary>
    /// <param name="slot">Slot number</param>
    void OnLoadSlotButtonClicked(int slot)
    {
        // Show confirmation (using a simple approach - in production you'd use a proper dialog)
        // For now, we'll just log and load
        Debug.Log($"Loading game from slot {slot}... Current progress will be lost.");
        
        // Perform the load
        bool success = SaveSystem.LoadGame(slot);
        
        if (success)
        {
            Debug.Log($"Game loaded successfully from slot {slot}!");
            CloseSaveSlotPanel();
        }
        else
        {
            Debug.LogError($"Failed to load game from slot {slot}!");
        }
    }
}

