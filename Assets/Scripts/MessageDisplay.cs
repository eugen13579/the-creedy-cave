using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Displays temporary messages on screen (e.g., "Player has used a healthflask (HP +500)").
/// Messages appear in the top-left corner and disappear after a set duration.
/// Supports multiple messages stacked vertically.
/// </summary>
public class MessageDisplay : MonoBehaviour
{
    public static MessageDisplay Instance;
    
    [SerializeField] private float messageDuration = 5f;
    [SerializeField] private float messageSpacing = 5f; // Space between messages
    [SerializeField] private int maxMessages = 5; // Maximum number of messages to show at once
    
    private Canvas messageCanvas;
    private Transform messageContainer;
    private List<MessageItem> activeMessages = new List<MessageItem>();
    
    private class MessageItem
    {
        public GameObject gameObject;
        public TextMeshProUGUI text;
        public Coroutine coroutine;
        public float startTime;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        SetupMessageDisplay();
    }
    
    void SetupMessageDisplay()
    {
        // Find or create HUD Canvas
        Canvas hudCanvas = null;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "HUDCanvas" || c.name == "HUD")
            {
                hudCanvas = c;
                break;
            }
        }
        
        if (hudCanvas == null)
        {
            // Create HUD Canvas if it doesn't exist
            GameObject canvasObj = new GameObject("HUDCanvas");
            hudCanvas = canvasObj.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            hudCanvas.sortingOrder = 100;
        }
        
        messageCanvas = hudCanvas;
        
        // Find or create message container
        Transform containerTransform = hudCanvas.transform.Find("MessageDisplayContainer");
        if (containerTransform != null)
        {
            messageContainer = containerTransform;
        }
        else
        {
            GameObject containerObj = new GameObject("MessageDisplayContainer");
            containerObj.transform.SetParent(hudCanvas.transform, false);
            
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.pivot = new Vector2(0, 1);
            containerRect.anchoredPosition = new Vector2(20, -20); // Top-left, 20px from edges
            containerRect.sizeDelta = new Vector2(800, 400); // Container for multiple messages
            
            messageContainer = containerObj.transform;
        }
    }
    
    /// <summary>
    /// Shows a temporary message that disappears after the set duration.
    /// Supports multiple messages stacked vertically.
    /// </summary>
    public void ShowMessage(string message, float duration = -1f)
    {
        if (messageContainer == null)
        {
            SetupMessageDisplay();
        }
        
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        
        // Remove oldest messages if we're at max capacity
        while (activeMessages.Count >= maxMessages)
        {
            RemoveMessage(activeMessages[0]);
        }
        
        // Create new message GameObject
        GameObject messageObj = new GameObject($"Message_{activeMessages.Count}");
        messageObj.transform.SetParent(messageContainer, false);
        
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(0, 1);
        messageRect.pivot = new Vector2(0, 1);
        messageRect.sizeDelta = new Vector2(800, 40); // Wider to fit text on one line
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = message;
        messageText.fontSize = 24;
        messageText.fontStyle = FontStyles.Bold;
        messageText.color = new Color(1f, 1f, 1f, 1f); // White color
        messageText.alignment = TextAlignmentOptions.Left;
        messageText.verticalAlignment = VerticalAlignmentOptions.Top;
        messageText.outlineWidth = 0.2f;
        messageText.outlineColor = new Color(0f, 0f, 0f, 1f); // Black outline
        messageText.enableWordWrapping = false; // Prevent text wrapping
        messageText.overflowMode = TextOverflowModes.Overflow; // Allow text to overflow if needed
        
        // Create message item
        MessageItem messageItem = new MessageItem
        {
            gameObject = messageObj,
            text = messageText,
            startTime = Time.time
        };
        
        // Position message based on existing messages
        UpdateMessagePositions();
        
        // Start coroutine to remove message after duration
        messageItem.coroutine = StartCoroutine(ShowMessageCoroutine(messageItem, duration > 0 ? duration : messageDuration));
        
        activeMessages.Add(messageItem);
    }
    
    private void UpdateMessagePositions()
    {
        float currentY = 0f;
        foreach (MessageItem item in activeMessages)
        {
            if (item.gameObject != null)
            {
                RectTransform rect = item.gameObject.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -currentY);
                currentY += rect.sizeDelta.y + messageSpacing;
            }
        }
    }
    
    private IEnumerator ShowMessageCoroutine(MessageItem messageItem, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        RemoveMessage(messageItem);
    }
    
    private void RemoveMessage(MessageItem messageItem)
    {
        if (messageItem == null) return;
        
        if (messageItem.coroutine != null)
        {
            StopCoroutine(messageItem.coroutine);
        }
        
        if (messageItem.gameObject != null)
        {
            Destroy(messageItem.gameObject);
        }
        
        activeMessages.Remove(messageItem);
        UpdateMessagePositions();
    }
    
    /// <summary>
    /// Shows an error message (red text).
    /// </summary>
    public void ShowError(string message, float duration = -1f)
    {
        if (messageContainer == null)
        {
            SetupMessageDisplay();
        }
        
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        
        // Remove oldest messages if we're at max capacity
        while (activeMessages.Count >= maxMessages)
        {
            RemoveMessage(activeMessages[0]);
        }
        
        // Create new message GameObject
        GameObject messageObj = new GameObject($"Error_{activeMessages.Count}");
        messageObj.transform.SetParent(messageContainer, false);
        
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0, 1);
        messageRect.anchorMax = new Vector2(0, 1);
        messageRect.pivot = new Vector2(0, 1);
        messageRect.sizeDelta = new Vector2(800, 40);
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = message;
        messageText.fontSize = 24;
        messageText.fontStyle = FontStyles.Bold;
        messageText.color = new Color(1f, 0f, 0f, 1f); // Red color for errors
        messageText.alignment = TextAlignmentOptions.Left;
        messageText.verticalAlignment = VerticalAlignmentOptions.Top;
        messageText.outlineWidth = 0.2f;
        messageText.outlineColor = new Color(0f, 0f, 0f, 1f); // Black outline
        messageText.enableWordWrapping = false;
        messageText.overflowMode = TextOverflowModes.Overflow;
        
        MessageItem messageItem = new MessageItem
        {
            gameObject = messageObj,
            text = messageText,
            startTime = Time.time
        };
        
        UpdateMessagePositions();
        messageItem.coroutine = StartCoroutine(ShowMessageCoroutine(messageItem, duration > 0 ? duration : messageDuration));
        
        activeMessages.Add(messageItem);
    }
}

