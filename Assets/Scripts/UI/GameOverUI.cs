using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button quitButton;
    
    [Header("Game References")]
    [SerializeField] private SC_Player playerController;
    
    void Start()
    {
        // Auto-find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<SC_Player>();
        }
        
        // Setup button listeners
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitToMainMenu);
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("Try Again button pressed - Restarting game...");
        
        // Reset static variables in RestaurantGameManager
        if (RestaurantGameManager.Instance != null)
        {
            RestaurantGameManager.Instance.ResetGame();
        }
        
        // Reset customer manager
        if (CustomerManager.Instance != null)
        {
            CustomerManager.ResetGame();
        }
        
        // Reset time scale and cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Reload the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    public void QuitToMainMenu()
    {
        Debug.Log("Quit button pressed - Going to main menu...");
        
        // Reset time scale and cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Load main menu scene
        SceneManager.LoadScene("Menu");
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitToMainMenu);
        }
    }
} 