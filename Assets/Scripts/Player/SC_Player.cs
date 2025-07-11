using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class SC_Player : MonoBehaviour

{

    [Header("Pause Menu")]
    public GameObject pauseMenuUI;
    // public UnityEngine.UI.Image resumeButtonImage; // Reference to the button image for click effect
    private bool isPaused = false;
    
    [Header("Finish Menu")]
    public GameObject finishMenuUI;
    private bool isGameOver = false;

    [Header("Tutorial")]
    public GameObject tutorialMessage;
    public bool isTutorialActive = false;

    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public Camera playerCamera;

    [Header("Interaction Settings")]
    public float interactionDistance = 20f;
    public TMP_Text interactionTextUI;
    public GameObject interactionPanel;
    public GameObject toastPanel;

    private PlayerControls _actions;
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalLookRotation = 0f;
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    private IInteractable currentInteractable;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        _actions = new PlayerControls();

        _actions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        _actions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        _actions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        _actions.Player.Look.canceled += _ => lookInput = Vector2.zero;

        _actions.Player.Interact.performed += _ => HandleInteractionInput(); // 🔑 uses Input Action "Interact"

        _actions.Player.Pause.performed += _ => TogglePauseMenu();
        // _actions.Player.Pause.canceled += _ => TogglePauseMenu();
        
        _actions.Player.Look.Disable(); // Disable look input initially
        // Enable actions after setup
        _actions.Enable();
    }



    void OnEnable() 
    {
        if (_actions != null)
        {
            _actions.Enable();
        }
    }
    
    void OnDisable() 
    {
        if (_actions != null)
        {
            _actions.Disable();
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Don't handle player input if game is over
        if (isGameOver) return;
        
        HandleLook();
        HandleInteractionCheck();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= moveSpeed;

        // Apply gravity
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void ToggleOutline(IInteractable target, bool state)
    {
        if (target == null) return;

        var mono = target as MonoBehaviour;
        if (mono == null) return;

        var highlight = mono.GetComponent<SimpleMaterialHighlighter>();
        if (highlight != null)
        {
            highlight.SetHighlight(state);
        }
    }


    void HandleInteractionCheck()
    {
        if (currentInteractable != null)
            ToggleOutline(currentInteractable, false);
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        // Use extended interaction distance for better customer interaction
        float maxDistance = interactionDistance * 2f; // 2x multiplier for very comfortable interaction
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>()
                                       ?? hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                // Check if it's a customer and use their specific interaction distance
                CustomerController customer = interactable as CustomerController;
                float actualDistance = Vector3.Distance(playerCamera.transform.position, hit.point);
                float requiredDistance = customer != null ? customer.GetInteractionDistance() : interactionDistance;
                
                if (actualDistance <= requiredDistance)
            {
                currentInteractable = interactable;
                string interactionText = interactable.getInteractionText();

                    // Debug info for customer interaction
                    if (customer != null)
                    {
                        Debug.Log($"Customer interaction: Distance={actualDistance:F1}m, Required={requiredDistance:F1}m");
                    }

                ToggleOutline(currentInteractable, true);
                if (!string.IsNullOrEmpty(interactionText))
                {
                    interactionTextUI.text = interactionText;
                    interactionPanel.SetActive(true);
                }
                else
                {
                    interactionPanel.SetActive(false);
                }

                return;
                }
            }
        }

        if (currentInteractable != null)
        {
            ToggleOutline(currentInteractable, false);
            currentInteractable = null;
        }
        interactionPanel.SetActive(false);
    }

    void HandleInteractionInput()
    {
        if (currentInteractable != null && !isPaused && !isGameOver)
        {
            currentInteractable.Interact();
        }
    }
    
    public void ShowFinishMenu()
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pause the game
        
        // Hide other UI elements
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (toastPanel != null) toastPanel.SetActive(false);
        
        // Show finish menu
        if (finishMenuUI != null) finishMenuUI.SetActive(true);
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _actions.Player.Look.Disable(); // Freeze mouse movement
        
        Debug.Log("Game Over! Finish Menu displayed.");
    }
    
    public void RestartGame()
    {
        // Reset game state
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        // Hide finish menu
        if (finishMenuUI != null) finishMenuUI.SetActive(false);
        
        // Reset cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _actions.Player.Look.Enable();
        
        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void QuitToMainMenu()
    {
        // Reset game state
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        // Load main menu scene (you can change this to your main menu scene name)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }

    public void PlayClickEffect(RectTransform buttonTransform)
    {
        StartCoroutine(ClickEffect(buttonTransform));
    }

    private IEnumerator ClickEffect(RectTransform target)
    {
        Vector3 originalScale = target.localScale;
        Quaternion originalRotation = target.localRotation;
        Vector3 pressedScale = originalScale * 1.05f;
        Quaternion tiltRotation = Quaternion.Euler(0, 0, 10f); // Tilt 10 degrees sideways

        target.localScale = pressedScale;
        target.localRotation = tiltRotation;
        yield return new WaitForSecondsRealtime(0.1f);

        target.localScale = originalScale;
        target.localRotation = originalRotation;

        TogglePauseMenu(); // Call resume after the effect
    }

    void TogglePauseMenu()
    {
        // Don't allow pausing if game is over
        if (isGameOver) return;
        
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (isPaused)
        {
            // Show pause menu
            pauseMenuUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _actions.Player.Look.Disable(); // Freeze mouse movement
            toastPanel?.SetActive(false); // Hide toast panel if it exists
            if(isTutorialActive)
                tutorialMessage.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        }
        else
        {
            // Hide pause menu
            pauseMenuUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _actions.Player.Look.Enable();
            if(isTutorialActive)
                tutorialMessage.GetComponentInChildren<TextMeshProUGUI>().enabled = true;

        }
    }

    // Debug visualization of interaction distance
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerCamera.transform.position, interactionDistance);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCamera.transform.position, interactionDistance * 1.5f);
            
            // Draw interaction ray
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * (interactionDistance * 1.5f));
        }
    }

    public void ToggleTutorial(bool isActive)
    {
        this.isTutorialActive = isActive;
    }
}