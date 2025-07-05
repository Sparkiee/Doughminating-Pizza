using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour   // class and file with the same name
{
    [Header("Doors")]
    [SerializeField] public Transform entryDoor;    // people come here
    [SerializeField] public Transform exitDoor;     // leave from here

    [Header("Seats & Spawning")]
    [SerializeField] private Transform[] seatTransforms;         // "chairs" points - will be auto-found if empty
    [SerializeField] public GameObject customerPrefab;          // prefab with CustomerController
    [SerializeField] public float spawnXOffset = -10f;
    [SerializeField] public float spawnInterval = 1f;

    [Header("Customer Limits")]
    [SerializeField] private int maxCustomersEarly = 1;    // Up to 5 orders
    [SerializeField] private int maxCustomersMid = 2;      // From 5 to 10 orders  
    [SerializeField] private int maxCustomersLate = 3;     // From 10 orders (max = number of seats)
    [SerializeField] private int ordersForMidTier = 5;     // When to increase to 2 customers
    [SerializeField] private int ordersForLateTier = 10;   // When to increase to 3 customers

    [HideInInspector]
    public bool[] availableSeatForCustomers;

    // Order statistics
    private static int completedOrders = 0;
    private int currentCustomerLimit = 1;

    public static CustomerManager Instance { get; private set; }

    // Unity Editor Reset method - called when component is added or reset
    void Reset()
    {
        // Set default values
        spawnXOffset = -10f;
        spawnInterval = 1f;
        maxCustomersEarly = 1;
        maxCustomersMid = 2;
        maxCustomersLate = 3;
        ordersForMidTier = 5;
        ordersForLateTier = 10;

        Debug.Log("CustomerManager Reset() called - default values set");
    }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialization with null protection
        try
        {
            // Automatically find all seats if array is empty
            if (seatTransforms == null || seatTransforms.Length == 0)
            {
                AutoFindSeats();
            }

            // Additional check after auto-search
            if (seatTransforms == null || seatTransforms.Length == 0)
            {
                Debug.LogError("No seats found! CustomerManager cannot function without seats.");
                return;
            }

            availableSeatForCustomers = new bool[seatTransforms.Length];
            for (int i = 0; i < availableSeatForCustomers.Length; i++)
                availableSeatForCustomers[i] = true;

            // Set initial customer limit
            UpdateCustomerLimit();

            Debug.Log($"CustomerManager initialized with {seatTransforms.Length} seats, current limit: {currentCustomerLimit} customers");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing CustomerManager: {e.Message}");
        }
    }

    void Start()
    {
        // Subscribe to level changes from RestaurantGameManager
        RestaurantGameManager.OnLevelChanged += OnDifficultyLevelChanged;
        StartCoroutine(SpawnLoop());
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        RestaurantGameManager.OnLevelChanged -= OnDifficultyLevelChanged;
    }

    // Handle difficulty level changes from RestaurantGameManager
    private void OnDifficultyLevelChanged(int oldLevel, int newLevel)
    {
        int oldLimit = currentCustomerLimit;
        UpdateCustomerLimit();

        if (currentCustomerLimit != oldLimit)
        {
            Debug.Log($"Difficulty level changed from {oldLevel} to {newLevel}! Customer limit changed from {oldLimit} to {currentCustomerLimit}");
        }
    }

    // Automatically find all seats in CustomerSpawnPoints
    private void AutoFindSeats()
    {
        GameObject spawnPoints = GameObject.Find("CustomerSpawnPoints");
        if (spawnPoints != null)
        {
            List<Transform> seats = new List<Transform>();

            // Find all child objects that start with "Seat_"
            foreach (Transform child in spawnPoints.transform)
            {
                if (child.name.StartsWith("Seat_"))
                {
                    seats.Add(child);
                }
            }

            // Sort by name for correct order (Seat_0, Seat_1, etc.)
            seats.Sort((a, b) => a.name.CompareTo(b.name));

            seatTransforms = seats.ToArray();
            Debug.Log($"Auto-found {seatTransforms.Length} seats: {string.Join(", ", System.Array.ConvertAll(seatTransforms, s => s.name))}");
        }
        else
        {
            Debug.LogError("CustomerSpawnPoints object not found! Please create it or manually assign seat transforms.");
        }
    }

    // Update customer limit based on RestaurantGameManager level or fallback to orders
    private void UpdateCustomerLimit()
    {
        if (RestaurantGameManager.IsGameStarted())
        {
            // Use RestaurantGameManager level system
            int currentLevel = RestaurantGameManager.GetCurrentLevel();

            if (currentLevel <= 1)
            {
                currentCustomerLimit = maxCustomersEarly; // Level 1: 1 customer
            }
            else if (currentLevel <= 3)
            {
                currentCustomerLimit = maxCustomersMid;   // Level 2-3: 2 customers
            }
            else
            {
                currentCustomerLimit = maxCustomersLate;  // Level 4-5: 3 customers
            }
        }
        else
        {
            // Fallback to old order-based system if RestaurantGameManager not available
            if (completedOrders < ordersForMidTier)
            {
                currentCustomerLimit = maxCustomersEarly;
            }
            else if (completedOrders < ordersForLateTier)
            {
                currentCustomerLimit = maxCustomersMid;
            }
            else
            {
                currentCustomerLimit = maxCustomersLate;
            }
        }
    }

    // Get current number of customers in restaurant
    private int GetCurrentCustomerCount()
    {
        int count = 0;
        for (int i = 0; i < availableSeatForCustomers.Length; i++)
        {
            if (!availableSeatForCustomers[i]) // Seat is occupied
            {
                count++;
            }
        }
        return count;
    }

    // Public method to increment completed orders counter
    public static void AddCompletedOrder()
    {
        completedOrders++;
        Debug.Log($"Order completed! Total orders: {completedOrders}");

        if (Instance != null)
        {
            int oldLimit = Instance.currentCustomerLimit;
            Instance.UpdateCustomerLimit();

            if (Instance.currentCustomerLimit != oldLimit)
            {
                Debug.Log($"Customer limit increased from {oldLimit} to {Instance.currentCustomerLimit}!");
            }
        }
    }

    // Get current statistics (for UI or debugging)
    public static int GetCompletedOrders() => completedOrders;
    public int GetCurrentCustomerLimit() => currentCustomerLimit;

    // Calculate adjusted spawn interval based on difficulty
    private float GetAdjustedSpawnInterval()
    {
        if (RestaurantGameManager.IsGameStarted())
        {
            // Use difficulty multiplier from RestaurantGameManager
            float difficultyMultiplier = RestaurantGameManager.GetDifficultyMultiplier();
            // Higher difficulty = shorter spawn interval (more customers spawn faster)
            return spawnInterval / difficultyMultiplier;
        }
        else
        {
            // Fallback to normal spawn interval
            return spawnInterval;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Check if customer limit is not exceeded
            int currentCustomers = GetCurrentCustomerCount();

            if (currentCustomers < currentCustomerLimit)
            {
                // Get list of all available seats
                List<int> availableSeats = GetAvailableSeats();

                // If there are available seats, choose random one
                if (availableSeats.Count > 0)
                {
                    int randomSeatIndex = availableSeats[Random.Range(0, availableSeats.Count)];
                    SpawnCustomerAtSeat(randomSeatIndex);

                    Debug.Log($"Spawned customer ({currentCustomers + 1}/{currentCustomerLimit}). Orders completed: {completedOrders}");
                }
            }

            // Calculate spawn interval based on difficulty
            float adjustedSpawnInterval = GetAdjustedSpawnInterval();
            yield return new WaitForSeconds(adjustedSpawnInterval);
        }
    }

    // New method to get list of available seats
    private List<int> GetAvailableSeats()
    {
        List<int> availableSeats = new List<int>();

        for (int i = 0; i < availableSeatForCustomers.Length; i++)
        {
            if (availableSeatForCustomers[i])
            {
                availableSeats.Add(i);
            }
        }

        Debug.Log($"Available seats: [{string.Join(", ", availableSeats)}] out of {availableSeatForCustomers.Length} total seats");
        return availableSeats;
    }

    void SpawnCustomerAtSeat(int seatIndex)
    {
        // Data validity checks
        if (availableSeatForCustomers == null || seatTransforms == null)
        {
            Debug.LogError("CustomerManager data is null! Cannot spawn customer.");
            return;
        }

        if (seatIndex < 0 || seatIndex >= availableSeatForCustomers.Length || seatIndex >= seatTransforms.Length)
        {
            Debug.LogError($"Invalid seat index {seatIndex}! Available seats: 0-{availableSeatForCustomers.Length - 1}");
            return;
        }

        // Additional check that seat is actually available
        if (!availableSeatForCustomers[seatIndex])
        {
            Debug.LogWarning($"Trying to spawn customer at already occupied seat {seatIndex}!");
            return;
        }

        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is null! Cannot spawn customer.");
            return;
        }

        Vector3 spawnPos = entryDoor != null ? entryDoor.position : Vector3.zero;
        spawnPos.y = 1f; // Elevate customer so feet touch floor (capsule height=2, center at +1)

        GameObject go = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
        var ctrl = go.GetComponent<CustomerController>();
        if (ctrl == null)
        {
            Debug.LogError("CustomerController component not found on spawned customer!");
            Destroy(go);
            return;
        }

        ctrl.mySeat = seatIndex;
        Debug.Log($"Assigned seat {seatIndex} to customer {ctrl.gameObject.name}");

        // Set destination so feet touch floor with no random offset
        Vector3 destination = seatTransforms[seatIndex] != null ? seatTransforms[seatIndex].position : Vector3.zero;
        destination.y = 1f; // Elevate destination so feet touch floor

        // Add a forward offset so the customer stands slightly in front of the seat
        destination -= seatTransforms[seatIndex].forward * -1f; // Adjust the distance if needed

        ctrl.destination = destination;

        // we pass the doors
        ctrl.entryPoint = entryDoor;
        ctrl.exitPoint = exitDoor;

        // Immediately occupy the seat
        availableSeatForCustomers[seatIndex] = false;

        string seatName = seatTransforms[seatIndex] != null ? seatTransforms[seatIndex].name : $"Seat_{seatIndex}";
        Debug.Log($"Spawned customer at seat {seatIndex} ({seatName})");
    }

    public void ResetGame()
    {
        // Reset static variables
        completedOrders = 0;
        
        if (Instance != null)
        {
            // Reset instance variables
            Instance.currentCustomerLimit = Instance.maxCustomersEarly;
            
            // Clear all seats (mark as available)
            if (Instance.availableSeatForCustomers != null)
            {
                for (int i = 0; i < Instance.availableSeatForCustomers.Length; i++)
                {
                    Instance.availableSeatForCustomers[i] = true;
                }
            }
            
            // Destroy all existing customers
            CustomerController[] existingCustomers = FindObjectsOfType<CustomerController>();
            for (int i = 0; i < existingCustomers.Length; i++)
            {
                if (existingCustomers[i] != null)
                {
                    Destroy(existingCustomers[i].gameObject);
                }
            }
            Reset();
            Debug.Log("CustomerManager game state reset!");
        }
    }
}