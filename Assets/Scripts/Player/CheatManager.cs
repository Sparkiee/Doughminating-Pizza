using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour
{

    [Header("Cheat Settings")]
    [SerializeField] private GameObject cheatPanel; // Panel to hold the cheats UI
    [SerializeField] private Cheat[] cheats; // Array of cheats to manage

    [System.Serializable]
    public class Cheat {
        public enum cheatName {
            GodMode,
            InstantPizza,
            NoBurn,
        }
        public cheatName name;
        public Toggle toggle;
        public bool isActive = false;
    }

    public static CheatManager Instance { get; private set; }

    void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        #if PRODUCTION
            cheatPanel.SetActive(false);
            Debug.Log("Cheat panel is disabled in production mode.");
        #endif
    }
    
    void Start() {
        foreach (var cheat in cheats) {
            if (cheat.toggle != null) {
                cheat.toggle.onValueChanged.AddListener(delegate { ToggleCheat(cheat); });
            } else {
                Debug.LogWarning($"Toggle for cheat {cheat.name} is not assigned!");
            }
        }
    }

    private void ToggleCheat(Cheat cheat) {
        cheat.isActive = cheat.toggle.isOn;
        Debug.Log($"Cheat {cheat.name} is now {(cheat.isActive ? "enabled" : "disabled")}");
    }

    public bool IsCheatActive(Cheat.cheatName name) {
        foreach (var cheat in cheats) {
            if (cheat.name == name) {
                return cheat.isActive;
            }
        }
        return false; // Cheat not found, return false
    }
}
