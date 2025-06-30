using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Player Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField, Tooltip("Current health of the player (cannot exceed maxHealth)")] private int health = 3;  // Current health of the player
    [SerializeField] private GameObject healthUI; // Prefab for the health bar UI
    [SerializeField] private GameObject healthy;
    [SerializeField] private GameObject ruined;
    [SerializeField] private bool isGodMode = false;
    [SerializeField] private Toggle godModeToggle; // Toggle for enabling/disabling god mode

    private List<GameObject> healthBar = new List<GameObject>();
    private SC_Player playerController;

    public void updateHealth()
    {
        foreach (var icon in healthBar)
        {
            Destroy(icon);
        }
        healthBar.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject icon;
            if (i < health)
                icon = Instantiate(healthy, healthUI.transform);
            else
                icon = Instantiate(ruined, healthUI.transform);
            healthBar.Add(icon);
        }

    }
    void Start()
    {
        playerController = FindObjectOfType<SC_Player>();
        if (playerController == null)
        {
            Debug.LogWarning("SC_Player not found! Game Over functionality will not work.");
        }
        updateHealth();

        if (godModeToggle != null)
        {
            godModeToggle.onValueChanged.AddListener(delegate { ToggleGodMode(godModeToggle.isOn); });
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        if (isGodMode)
        {
            Debug.Log("God mode is active, no damage taken.");
            return; // Skip damage if in god mode
        }
        health -= damage;
        if (health <= 0)
        {
            health = 0; // Ensure health doesn't go below 0
            updateHealth();
            
            // Trigger Game Over
            if (playerController != null)
            {
                playerController.ShowFinishMenu();
                Debug.Log("Health reached 0! Game Over triggered.");
            }
            return;
        }
        updateHealth();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        health += amount; // Increase current health as well
        updateHealth();
    }

    public void Heal(int amount)
    {
        if (isGodMode)
        {
            Debug.Log("God mode is active, no healing applied.");
            return; // Skip healing if in god mode
        }
        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth; // Ensure health doesn't exceed maxHealth
        }
        updateHealth();
    }

    private void ToggleGodMode(bool isActive)
    {
        isGodMode = isActive;
        if (isGodMode)
        {
            Debug.Log("God mode activated.");
        }
        else
        {
            Debug.Log("God mode deactivated.");
        }
    }
}
