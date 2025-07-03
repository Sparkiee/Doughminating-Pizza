using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CustomerAppearanceData
{
    public string name;
    public GameObject modelPrefab;  // Prefab with model (e.g., Cop, Doctor, etc.)
    public bool isEnabled = true;   // Can disable specific models
}

public class CustomerAppearanceManager : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController customerAnimatorController;

    [Header("Customer Models")]
    [Tooltip("Drag all customer prefabs from the Assets/Perfabs/Customers/ folder here")]
    [SerializeField] private CustomerAppearanceData[] availableAppearances = new CustomerAppearanceData[]
    {
        new CustomerAppearanceData { name = "Cop", isEnabled = true },
        new CustomerAppearanceData { name = "Doctor", isEnabled = true },
        new CustomerAppearanceData { name = "Nurse", isEnabled = true },
        new CustomerAppearanceData { name = "Knight", isEnabled = true },
        new CustomerAppearanceData { name = "Ghost", isEnabled = true },
        new CustomerAppearanceData { name = "Devil", isEnabled = true },
        new CustomerAppearanceData { name = "Witch", isEnabled = true },
        new CustomerAppearanceData { name = "Zombie", isEnabled = true },
        new CustomerAppearanceData { name = "Santa", isEnabled = true },
        new CustomerAppearanceData { name = "Pilgrim", isEnabled = true },
        new CustomerAppearanceData { name = "Robber", isEnabled = true },
        new CustomerAppearanceData { name = "Villager", isEnabled = true },
        new CustomerAppearanceData { name = "Patient", isEnabled = true },
        new CustomerAppearanceData { name = "Streaker", isEnabled = true },
        new CustomerAppearanceData { name = "SoccerPlayerBlue", isEnabled = true },
        new CustomerAppearanceData { name = "SoccerPlayerRed", isEnabled = true },
        new CustomerAppearanceData { name = "CricketBatsman", isEnabled = true },
        new CustomerAppearanceData { name = "CricketFielder", isEnabled = true }
    };
    
    private static CustomerAppearanceManager instance;
    public static CustomerAppearanceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CustomerAppearanceManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log($"CustomerAppearanceManager initialized with {availableAppearances.Length} appearance options");
    }

    // Get random appearance
    public CustomerAppearanceData GetRandomAppearance()
    {
        if (availableAppearances == null || availableAppearances.Length == 0)
        {
            Debug.LogError("No customer appearances available!");
            return null;
        }

        // Filter only enabled appearances with assigned prefabs
        List<CustomerAppearanceData> enabledAppearances = new List<CustomerAppearanceData>();
        foreach (var appearance in availableAppearances)
        {
            if (appearance.isEnabled && appearance.modelPrefab != null)
            {
                enabledAppearances.Add(appearance);
            }
        }

        if (enabledAppearances.Count == 0)
        {
            Debug.LogWarning("No enabled customer appearances with assigned prefabs available! Using default appearance.");
            return null;
        }

        int randomIndex = Random.Range(0, enabledAppearances.Count);
        CustomerAppearanceData selected = enabledAppearances[randomIndex];
        Debug.Log($"Selected random appearance: {selected.name}");
        return selected;
    }

    // Apply appearance to customer
    public GameObject ApplyAppearanceToCustomer(GameObject customerObject, CustomerAppearanceData appearance)
    {
        if (appearance == null || appearance.modelPrefab == null)
        {
            Debug.LogWarning("Invalid appearance data, using default customer model");
            return null;
        }

        // Find or create container for model
        Transform modelParent = customerObject.transform.Find("ModelContainer");
        if (modelParent == null)
        {
            GameObject modelContainer = new GameObject("ModelContainer");
            modelContainer.transform.SetParent(customerObject.transform);
            modelContainer.transform.localPosition = Vector3.zero;
            modelContainer.transform.localRotation = Quaternion.identity;
            modelContainer.transform.localScale = Vector3.one;
            modelParent = modelContainer.transform;
        }

        // Remove old model if exists
        for (int i = modelParent.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(modelParent.GetChild(i).gameObject);
            else
                DestroyImmediate(modelParent.GetChild(i).gameObject);
        }

        // Find visual part in prefab
        GameObject visualPart = FindVisualPart(appearance.modelPrefab);
        
        if (visualPart != null)
        {
            // Create copy of visual part
            GameObject newModel = Instantiate(visualPart, modelParent);
            newModel.name = appearance.name + "_Model";
            
            // Configure position and scale
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
            

            newModel.AddComponent<Animator>();
            if (customerAnimatorController != null)
            {
                Animator newAnimator = newModel.GetComponent<Animator>();
                newAnimator.runtimeAnimatorController = customerAnimatorController;
            }

            // Remove components not needed for visuals (if any)
            RemoveUnnecessaryComponents(newModel);
            
            Debug.Log($"Successfully applied appearance '{appearance.name}' to customer");
            return newModel;
        }
        else
        {
            Debug.LogError($"Could not find visual part in prefab: {appearance.name}");
            return null;
        }
    }

    // Find visual part in prefab
    GameObject FindVisualPart(GameObject prefab)
    {
        // Look for object with name matching prefab name (e.g., "Cop" in Cop.prefab)
        string prefabName = prefab.name;
        
        // First check root object
        if (prefab.name == prefabName && HasVisualComponents(prefab))
        {
            return prefab;
        }

        // Look for child object with needed name
        Transform found = prefab.transform.Find(prefabName);
        if (found != null && HasVisualComponents(found.gameObject))
        {
            return found.gameObject;
        }

        // If not found by name, look for any object with visual components
        return FindFirstVisualObject(prefab);
    }

            // Find first object with visual components
    GameObject FindFirstVisualObject(GameObject obj)
    {
        if (HasVisualComponents(obj))
        {
            return obj;
        }

        foreach (Transform child in obj.transform)
        {
            GameObject found = FindFirstVisualObject(child.gameObject);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    // Check if object has visual components
    bool HasVisualComponents(GameObject obj)
    {
        return obj.GetComponent<MeshRenderer>() != null || 
               obj.GetComponent<SkinnedMeshRenderer>() != null ||
               obj.GetComponentInChildren<MeshRenderer>() != null ||
               obj.GetComponentInChildren<SkinnedMeshRenderer>() != null;
    }

    // Remove unnecessary components from visual model
    void RemoveUnnecessaryComponents(GameObject obj)
    {
        // Remove components that might conflict with main Customer object
        
        // Remove colliders (we already have main collider on Customer)
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            if (Application.isPlaying)
                Destroy(collider);
            else
                DestroyImmediate(collider);
        }

        // Remove Rigidbody if exists
        Rigidbody[] rigidbodies = obj.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            if (Application.isPlaying)
                Destroy(rb);
            else
                DestroyImmediate(rb);
        }

        // Remove AudioSource if exists (we already have main one on Customer)
        AudioSource[] audioSources = obj.GetComponentsInChildren<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            if (Application.isPlaying)
                Destroy(audioSource);
            else
                DestroyImmediate(audioSource);
        }

        // Remove control scripts (e.g., CustomerController if there's a duplicate)
        MonoBehaviour[] scripts = obj.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script is CustomerController)
            {
                if (Application.isPlaying)
                    Destroy(script);
                else
                    DestroyImmediate(script);
            }
        }
    }

    // For debugging - get list of available appearances
    public string[] GetAvailableAppearanceNames()
    {
        if (availableAppearances == null) return new string[0];
        
        List<string> names = new List<string>();
        foreach (var appearance in availableAppearances)
        {
            if (appearance.isEnabled && appearance.modelPrefab != null)
            {
                names.Add(appearance.name);
            }
        }
        return names.ToArray();
    }

    // Get number of available appearances
    public int GetEnabledAppearanceCount()
    {
        int count = 0;
        foreach (var appearance in availableAppearances)
        {
            if (appearance.isEnabled && appearance.modelPrefab != null)
            {
                count++;
            }
        }
        return count;
    }
} 