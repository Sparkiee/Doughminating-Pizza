using UnityEngine;
using TMPro;
using System.Collections;

public class RestaurantGameManager : MonoBehaviour
{
    [Header("Day System")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private float dayDuration = 60f; // Seconds per day (60 seconds = 1 minute)
    
    [Header("Difficulty Levels")]
    [SerializeField] private int daysPerLevel = 7;  // Days before difficulty increases
    [SerializeField] private int maxLevel = 5;      // Maximum difficulty level
    
    // Game state
    private static int currentDay = 0;
    private static int currentLevel = 1;
    private float dayTimer = 0f;
    private bool gameStarted = false;
    
    // Singleton pattern
    public static RestaurantGameManager Instance { get; private set; }
    
    // Events for other systems to subscribe to
    public static System.Action<int> OnDayChanged;
    public static System.Action<int, int> OnLevelChanged; // (oldLevel, newLevel)
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find day text if not assigned
        if (dayText == null)
        {
            FindDayText();
        }
    }
    
    void Start()
    {
        StartGame();
    }
    
    void Update()
    {
        if (!gameStarted) return;
        
        // Update day timer
        dayTimer += Time.deltaTime;
        
        // Check if a day has passed
        if (dayTimer >= dayDuration)
        {
            dayTimer = 0f;
            AdvanceDay();
        }
    }
    
    public void StartGame()
    {
        if (gameStarted) return;
        
        gameStarted = true;
        currentDay = 0;  // Start from day 0
        currentLevel = 1;
        dayTimer = 0f;
        
        UpdateDayDisplay();
        
        Debug.Log("Restaurant Game Started! Day 0, Level 1");
    }
    
    private void AdvanceDay()
    {
        currentDay++;
        
        // Check if we need to increase difficulty level
        int newLevel = Mathf.Min(((currentDay - 1) / daysPerLevel) + 1, maxLevel);
        
        if (newLevel != currentLevel)
        {
            int oldLevel = currentLevel;
            currentLevel = newLevel;
            OnLevelChanged?.Invoke(oldLevel, currentLevel);
            Debug.Log($"Level increased from {oldLevel} to {currentLevel}!");
        }
        
        // Update UI
        UpdateDayDisplay();
        
        // Notify other systems
        OnDayChanged?.Invoke(currentDay);
        
        Debug.Log($"Day {currentDay} started (Level {currentLevel})");
    }
    
    private void UpdateDayDisplay()
    {
        if (dayText != null)
        {
            dayText.text = currentDay.ToString();
        }
    }
    
    private void FindDayText()
    {
        // Look for GameObject named "Day" with a child "Text (TMP)"
        GameObject dayObject = GameObject.Find("Day");
        if (dayObject != null)
        {
            // Look for Text (TMP) component in children
            TextMeshProUGUI[] textComponents = dayObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponents.Length > 0)
            {
                dayText = textComponents[0];
                Debug.Log("Auto-found Day Text component");
            }
            else
            {
                Debug.LogWarning("Found Day object but no TextMeshProUGUI component in children");
            }
        }
        else
        {
            Debug.LogWarning("Could not find Day object in scene. Please assign dayText manually.");
        }
    }
    
    public static int GetCurrentDay() => currentDay;
    
    public static int GetCurrentLevel() => currentLevel;
    
    public float GetDayProgress()
    {
        return gameStarted ? (dayTimer / dayDuration) : 0f;
    }
    
    public static bool IsGameStarted() => Instance != null && Instance.gameStarted;
    
    public void ResetGame()
    {
        gameStarted = false;
        currentDay = 0;
        currentLevel = 1;
        dayTimer = 0f;
        UpdateDayDisplay();
        Debug.Log("Restaurant Game Reset");
    }
    
    public static float GetDifficultyMultiplier()
    {
        return 1f + (currentLevel - 1) * 0.2f; // Each level increases difficulty by 20%
    }
    
    public static string GetLevelName()
    {
        return currentLevel switch
        {
            1 => "Beginner",
            2 => "Easy",
            3 => "Normal",
            4 => "Hard",
            5 => "Expert",
            _ => "Unknown"
        };
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
} 