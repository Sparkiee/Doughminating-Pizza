using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class CustomerSeat {
    public Transform location;
    public bool isOccupied;
}

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int patienceTime = 90;
    [SerializeField] private int customersPerLevel = 2;

    private bool gameStarted = false;

    [Header("Game Locations")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    // Dictionary to track customer locations and their served status
    // Key: Transform (location), Value: bool (isCustomerHere)
    [SerializeField] private List<CustomerSeat> customerSeats = new List<CustomerSeat>();

    [Header("Customer Prefabs")]
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private GameObject orderBubblePrefab;
    [SerializeField] private GameObject patienceBarPrefab;

    [Header("Game State")]
    [SerializeField] private int totalCustomers = 0;
    [SerializeField] private int servedCustomers = 0;
    [SerializeField] private int failedCustomers = 0;
    [SerializeField] private List<GameObject> activeCustomers = new List<GameObject>();

    [Header("Sound Effects")]
    [SerializeField] private AudioClip successfulOrderSound;
    [SerializeField] private AudioClip failedOrderSound;

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI levelText;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            StartGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartGame() {
        if(this.gameStarted) return;
        this.gameStarted = true;
        this.levelText.text = currentLevel.ToString();
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop() {
        while (this.gameStarted) {
            // Define how many customers to spawn based on the current level
            int simultaneousCustomers;
            switch (currentLevel) {
                case <= 2:
                    simultaneousCustomers = 1;
                    break;
                case <= 5:
                    simultaneousCustomers = 2;
                    break;
                default:
                    simultaneousCustomers = 3; // Default for higher levels
                    break;
            }
            int currentCustomers = activeCustomers.Count;
            if (currentCustomers < simultaneousCustomers) {
                int customersToSpawn = simultaneousCustomers - currentCustomers;
                for (int i = 0; i < customersToSpawn; i++) {
                    SpawnCustomer();
                }
            }
            yield return new WaitForSeconds(2f); // Wait before spawning more customers
        }
        yield return null;
    }

    private void SpawnCustomer() {
        // Logic to spawn a customer
        int randomIndex = Random.Range(0, customerPrefabs.Length);
        string customerName = GenerateRandomName();
        // Instantiate the customer at the entry point
        GameObject customer = Instantiate(customerPrefabs[randomIndex], new Vector3(entryPoint.position.x, entryPoint.position.y + 0.9f, entryPoint.position.z), Quaternion.identity);
        customer.name = customerName;
        // Dynamically add a script/component to the customer if needed
        // For example, if you have a CustomerController script:
        if (customer.GetComponent<Customer>() == null)
        {
            customer.AddComponent<Customer>();

        }
        if (patienceBarPrefab != null) {
            Vector3 patienceBarPosition = customer.transform.position + new Vector3(0, 2.0f, 0); // Adjust Y offset as needed
            GameObject patienceBar = Instantiate(patienceBarPrefab, patienceBarPosition, Quaternion.identity, customer.transform);
            customer.GetComponent<Customer>().SetPatience(patienceTime, patienceBar);
        }

        if (orderBubblePrefab != null) {
            Vector3 orderBubblePosition = customer.transform.position + new Vector3(-0.5f, 1.5f, 0); // Adjust Y offset as needed
            GameObject orderBubble = Instantiate(orderBubblePrefab, orderBubblePosition, Quaternion.identity, customer.transform);
            customer.GetComponent<Customer>().AddOrderBubble(orderBubble);
            Order order = customer.AddComponent<Order>();
            order.InitializeOrderBubble(orderBubble);
        }

        this.activeCustomers.Add(customer);
        this.totalCustomers++;

        customer.GetComponent<Customer>().WalkToCounter(GetAvailableSeat());
        customer.GetComponent<Customer>().SetExitPoint(exitPoint.transform);
        customer.GetComponent<Customer>().SetSuccessfulOrderSound(successfulOrderSound);
        customer.GetComponent<Customer>().SetFailedOrderSound(failedOrderSound);
    }

    public void CustomerServed(GameObject customer, bool hasFailed) {
        if (customer == null) return;
        this.servedCustomers++;
        if (hasFailed) {
            this.failedCustomers++;
            this.patienceTime -= 5;
        }
        this.activeCustomers.Remove(customer);
        if(servedCustomers - failedCustomers >= customersPerLevel * currentLevel) {
            currentLevel++;
            levelText.text = currentLevel.ToString();
            Debug.Log($"Level {currentLevel} completed! Total served: {servedCustomers}");
        }
        Destroy(customer);
    }

    private string GenerateRandomName() {
        string[] names = { "Aaron", "Adam", "Alice", "Bob", "Charlie", "Diana", "Evyevy", "Ethan", "Fiona", "Gandalf", "Gordon", "Hannah", "Hobbit", "Ivy", "Jack", "Joe", "Joseph", "Kira", "Liam", "Max", "Mia", "Nora", "Oscar", "Penny", "Quinn", "Riley", "Sam", "Shrek" };
        return names[Random.Range(0, names.Length)];
    }

    private CustomerSeat GetAvailableSeat() {
        // Collect all available seats
        List<CustomerSeat> availableSeats = new List<CustomerSeat>();
        foreach (CustomerSeat seat in customerSeats) {
            if (!seat.isOccupied) {
                availableSeats.Add(seat);
            }
        }
        if (availableSeats.Count == 0) return null;
        // Pick a random available seat
        int randomIndex = Random.Range(0, availableSeats.Count);
        availableSeats[randomIndex].isOccupied = true;
        // To get the global position, use:
        return availableSeats[randomIndex];
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f; // Reset time scale
    }
}
