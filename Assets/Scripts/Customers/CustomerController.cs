using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour, IInteractable
{
    //***************************************************************************//
    // This class manages all things related to a customer, including
    // wishlist, ingredients, patience and animations.
    //***************************************************************************//
    
    public Transform entryPoint;    // what do we spawn from
    public Transform exitPoint;     // where are we going

    // Modifiable variables (Only Through Inspector - Do not hardcode!)
    public float customerPatience = 30.0f;              // seconds (default = 30)
    public float customerMoveSpeed = 3f;
    public bool showProductIngredientHelpers = true;    // whether to show customer wish's ingredients as a helper or not
    
    [Header("Interaction Settings")]
    public float customerInteractionDistance = 25f;     // Custom interaction distance for customers
    
    [Header("Animation Settings")]
    public float turnAroundDuration = 0.5f;             // How long it takes to turn around when leaving
    public bool turnClockwise = true;                    // Turn direction: true = clockwise, false = counter-clockwise

    [Header("Customer Separation")]
    public float separationDistance = 2.5f;             // Minimum distance to maintain from other customers
    public float separationForce = 5f;                  // How strong the separation force is
    public float separationRadius = 4f;                 // How far to look for other customers
    public bool enableSeparation = true;                // Toggle separation behavior on/off

    // Audio Clips
    public AudioClip orderIsOkSfx;
    public AudioClip orderIsNotOkSfx;
    public AudioClip receivedSomethingGoodSfx;

    // Customer Moods
    /* Currently we have 4 moods: 
     [0] Default
     [1] Bored 
     [2] Satisfied
     [3] Angry
     we change to appropriate material whenever needed. 
    */
    public Material[] customerMoods;
    private int moodIndex;

    // UI Elements
    public GameObject patienceBarFG;
    public GameObject patienceBarBG;
    
    private float barInitialScaleX;
    private Vector3 barInitialPos;
    
    public GameObject requestBubble;
    public GameObject money3dText;
    
    public int mySeat;
    public Vector3 destination;
    private CustomerManager manager;


    
    [SerializeField] private Transform iconRowTransform;

    // Private customer variables
    private string customerName;                // random name
    private float currentCustomerPatience;      // current patience of the customer
    private bool isOnSeat;                      // is customer on his seat?
    private bool isLeaving;                     // is customer leaving?
    private float creationTime;                 // when was this customer created
    private Vector3 startingPosition;           // reference position
    private List<System.Type> wantedIngredients = new();  // what ingredients does this customer want?
    private GameObject[] ingredientIcons;       // visual representation of wanted ingredients
    private bool patienceBarSliderFlag;         // flag to control patience bar animation
    
    // Health system reference
    private Health playerHealth;

    // Floor level constant - where customer feet should be
    private const float FLOOR_LEVEL = 1f;

    // Available ingredient types
    private static readonly System.Type[] AvailableIngredients = new System.Type[]
    {
        typeof(Sauce),
        typeof(Cheese),
        typeof(Bacon),
        typeof(Pineapple),
        typeof(Pepperoni)
    };

    void Awake()
    {
        manager = FindObjectOfType<CustomerManager>();
        
        // Find the player's health component
        playerHealth = FindObjectOfType<Health>();
        if (playerHealth == null)
        {
            Debug.LogWarning("Player Health component not found! Health damage will not work.");
        }
        
        startingPosition = entryPoint != null 
            ? entryPoint.position 
            : transform.position;
        
        // Make sure customer's feet touch the floor
        startingPosition.y = FLOOR_LEVEL; // Elevate so feet touch the floor
        transform.position = startingPosition;

        // Make sure the customer has the correct tag for door triggers
        gameObject.tag = "Customer";

        requestBubble.SetActive(false);
        patienceBarFG.SetActive(false);
        patienceBarBG.SetActive(false);
        
        
        if (isOnSeat)
        {
            patienceBarFG.SetActive(true);
            patienceBarBG.SetActive(true);
            patienceBarSliderFlag = true;
        }
        
        isLeaving = false;
        
        currentCustomerPatience = customerPatience;
        creationTime = Time.time;
        moodIndex = 0;
        
        barInitialScaleX = patienceBarBG.transform.localScale.x;
        barInitialPos = patienceBarBG.transform.localPosition;


        Init();
        StartCoroutine(goToSeat());
    }

    //***************************************************************************//
    // Initialize all customer related variables
    //***************************************************************************//
    void Init()
    {
        // Give this customer a random real name
        customerName = GetRandomCustomerName();
        gameObject.name = customerName;

        // Apply random appearance
        ApplyRandomAppearance();

        // Add and configure SimpleMaterialHighlighter component
        SimpleMaterialHighlighter highlighter = gameObject.GetComponent<SimpleMaterialHighlighter>();
        if (highlighter == null)
        {
            highlighter = gameObject.AddComponent<SimpleMaterialHighlighter>();
        }
        
        // Set the highlight material (green transparent material)
        Material highlightMat = Resources.Load<Material>("Scripts/New Material");
        if (highlightMat == null)
        {
            // Create a simple green highlight material if we can't load the existing one
            highlightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            highlightMat.color = new Color(0.33f, 0.78f, 0.30f, 0.43f);
            highlightMat.SetFloat("_Surface", 1.0f); // Transparent
            highlightMat.SetFloat("_Blend", 0.0f);
            highlightMat.SetFloat("_DstBlend", 10.0f);
            highlightMat.SetFloat("_ZWrite", 0.0f);
            highlightMat.renderQueue = 3000;
        }
        
        highlighter.highlightMaterial = highlightMat;

        // Load default audio clips if not assigned
        if (orderIsOkSfx == null)
        {
            orderIsOkSfx = Resources.Load<AudioClip>("Audio/MenuTap");
            if (orderIsOkSfx == null)
                Debug.LogWarning("Could not load orderIsOkSfx from Resources/Audio/MenuTap");
        }
        
        if (orderIsNotOkSfx == null)
        {
            orderIsNotOkSfx = Resources.Load<AudioClip>("Audio/MenuTap");
            if (orderIsNotOkSfx == null)
                Debug.LogWarning("Could not load orderIsNotOkSfx from Resources/Audio/MenuTap");
        }

        // Randomly choose ingredients for the customer's order
        chooseRandomIngredients();

        // Set up the customer order display
        setupOrderDisplay();
    }


    //***************************************************************************//
    // After this customer has been instantiated, it starts somewhere outside game scene
    // and then goes to its position (seat) with a nice animation.
    //***************************************************************************//
    private float timeVariance;
    
    IEnumerator goToSeat()
    {
        Debug.Log($"{customerName} is going to seat {mySeat} at position {destination}");
        
        // Until we got to the "chair"
        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = MoveTowardsOnFloor(transform.position, destination, customerMoveSpeed);
            yield return null;
        }
        // As soon as we sit down, we launch the UI and exit
        isOnSeat              = true;
        patienceBarSliderFlag = true;
        requestBubble.SetActive(true);
        patienceBarFG.SetActive(true);
        patienceBarBG.SetActive(true);
        
        Debug.Log($"{customerName} has reached seat {mySeat} and is now seated");
    }

    void Update()
    {
        if (patienceBarSliderFlag)
            StartCoroutine(patienceBar());

        // Apply customer separation if enabled and customer is seated
        if (enableSeparation && isOnSeat && !isLeaving)
        {
            ApplyCustomerSeparation();
        }

        // Manage customer's mood by changing its material
        // updateCustomerMood();
    }

    //***************************************************************************//
    // Make the customer react to events by changing its material (and texture)
    //***************************************************************************//
    void updateCustomerMood()
    {
        if (!isLeaving)
        {
            if (currentCustomerPatience <= customerPatience / 2)
                moodIndex = 1;  // Bored
            else
                moodIndex = 0;  // Default
        }

        // Safely set customer mood material
        if (customerMoods != null && customerMoods.Length > moodIndex && customerMoods[moodIndex] != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = customerMoods[moodIndex];
            }
        }
    }

    //***************************************************************************//
    // Show and animate progress bar based on customer's patience
    //***************************************************************************//
    IEnumerator patienceBar()
    {
        patienceBarSliderFlag = false;

        while (currentCustomerPatience > 0)
        {
            currentCustomerPatience -= Time.deltaTime;
            float ratio = Mathf.Clamp01(currentCustomerPatience / customerPatience);

            // Calculate new scale X
            float newScaleX = barInitialScaleX * ratio;

            // Apply scale to BG (curtain)
            patienceBarBG.transform.localScale = new Vector3(
                newScaleX,
                patienceBarBG.transform.localScale.y,
                patienceBarBG.transform.localScale.z
            );

            // Move the curtain to the right so that the right edge stays in place
            float delta = (newScaleX - barInitialScaleX) * 0.5f;
            patienceBarBG.transform.localPosition = new Vector3(
                barInitialPos.x - delta,
                barInitialPos.y,
                barInitialPos.z
            );

            yield return null;
        }

        patienceBarBG.SetActive(false);
        
        // Take damage when customer leaves due to impatience
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
            Debug.Log($"Player lost 1 health because {customerName} left due to impatience");
        }
        
        StartCoroutine(leave());
    }




    //***************************************************************************//
    // Fill customer's patience bar and make it full again
    //***************************************************************************//
    void fillCustomerPatience()
    {
        currentCustomerPatience = customerPatience;
        patienceBarFG.SetActive(true);
        // reset scale to full size
        patienceBarFG.transform.localScale = new Vector3(1f,
            patienceBarFG.transform.localScale.y,
            patienceBarFG.transform.localScale.z);
    }

    //***************************************************************************//
    // Safe movement that keeps customer on floor level (Y=1)
    //***************************************************************************//
    private Vector3 MoveTowardsOnFloor(Vector3 current, Vector3 target, float speed)
    {
        Vector3 newPosition = Vector3.MoveTowards(current, target, speed * Time.deltaTime);
        newPosition.y = FLOOR_LEVEL; // Force keep on floor level
        return newPosition;
    }

    //***************************************************************************//
    // Apply random appearance to customer
    //***************************************************************************//
    private void ApplyRandomAppearance()
    {
        if (CustomerAppearanceManager.Instance != null)
        {
            CustomerAppearanceData appearance = CustomerAppearanceManager.Instance.GetRandomAppearance();
            if (appearance != null)
            {
                GameObject appliedModel = CustomerAppearanceManager.Instance.ApplyAppearanceToCustomer(gameObject, appearance);
                if (appliedModel != null)
                {
                    Debug.Log($"Customer {customerName} got appearance: {appearance.name}");
                }
                else
                {
                    Debug.LogWarning($"Failed to apply appearance {appearance.name} to customer {customerName}");
                }
            }
            else
            {
                Debug.LogWarning($"No appearance available for customer {customerName}, using default model");
            }
        }
        else
        {
            Debug.LogWarning("CustomerAppearanceManager not found! Customer will use default appearance.");
        }
    }

    //***************************************************************************//
    // Get a random customer name
    //***************************************************************************//
    private string GetRandomCustomerName()
    {
        string[] firstNames = {
            // Male names
            "Alex", "Mike", "John", "David", "Chris", "Steve", "Mark", "Paul", "Tom", "Jack",
            "Nick", "Sam", "Ben", "Dan", "Luke", "Ryan", "Matt", "Josh", "Kyle", "Sean",
            "Adam", "Jake", "Noah", "Liam", "Owen", "Cole", "Zack", "Blake", "Dean", "Eric",
            
            // Female names  
            "Emma", "Olivia", "Ava", "Sophia", "Mia", "Luna", "Grace", "Chloe", "Zoe", "Lily",
            "Maya", "Anna", "Kate", "Rose", "Jane", "Amy", "Eva", "Ella", "Sara", "Leah",
            "Ruby", "Ivy", "Nora", "Aria", "Hazel", "Iris", "Vera", "Nina", "Tara", "Lola"
        };

        string[] lastNames = {
            "Smith", "Johnson", "Brown", "Taylor", "Miller", "Wilson", "Moore", "Davis", "Garcia", "Rodriguez",
            "Martinez", "Anderson", "Jackson", "White", "Harris", "Martin", "Thompson", "Young", "Allen", "King",
            "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker", "Nelson", "Carter", "Mitchell",
            "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart"
        };

        string firstName = firstNames[Random.Range(0, firstNames.Length)];
        string lastName = lastNames[Random.Range(0, lastNames.Length)];
        
        return $"{firstName} {lastName}";
    }

    //***************************************************************************//
    // Choose random ingredients for the customer's order
    //***************************************************************************//
    private void chooseRandomIngredients()
    {
        // Always include sauce and cheese as base ingredients
        wantedIngredients.Add(typeof(Sauce));
        wantedIngredients.Add(typeof(Cheese));

        // Add 0-3 random toppings (now can include all toppings)
        int numToppings = Random.Range(0, 4); // 0, 1, 2, or 3 toppings
        List<System.Type> availableToppings = new List<System.Type>
        {
            typeof(Bacon),
            typeof(Pineapple),
            typeof(Pepperoni)
        };

        for (int i = 0; i < numToppings; i++)
        {
            if (availableToppings.Count > 0)
            {
                int randomIndex = Random.Range(0, availableToppings.Count);
                wantedIngredients.Add(availableToppings[randomIndex]);
                availableToppings.RemoveAt(randomIndex); // Remove to avoid duplicates
            }
        }
        
        Debug.Log($"{customerName} wants: {GetWantedIngredientsString()}");
    }

    //***************************************************************************//
    // Set up visual representation of the customer's order
    //***************************************************************************//
    private void setupOrderDisplay()
    {
        // Create visual representation of wanted ingredients
        ingredientIcons = new GameObject[wantedIngredients.Count];
        for (int i = 0; i < wantedIngredients.Count; i++)
        {
            if (iconRowTransform != null)
            {
                string iconName = wantedIngredients[i].Name.ToLower();
                Sprite iconSprite = Resources.Load<Sprite>($"UI/Icons/{iconName}");

                if (iconSprite != null)
                {
                    GameObject iconGO = new GameObject("Icon_" + iconName);
                    iconGO.transform.SetParent(iconRowTransform, false);

                    Image img = iconGO.AddComponent<Image>();
                    img.sprite = iconSprite;
                    img.rectTransform.sizeDelta = new Vector2(64, 64); // Icon size
                    img.preserveAspect = true;

                    ingredientIcons[i] = iconGO;

                    if (!showProductIngredientHelpers)
                        img.enabled = false;
                        
                    Debug.Log($"Created icon for {iconName} at index {i}");
                }
                else
                {
                    Debug.LogWarning($"Icon sprite not found for: {iconName}. Looking in Resources/UI/Icons/");
                }
            }
        }
    }

    //***************************************************************************//
    // Get wanted ingredients as string for display
    //***************************************************************************//
    public string GetWantedIngredientsString()
    {
        List<string> ingredientNames = new List<string>();
        foreach (System.Type ingredientType in wantedIngredients)
        {
            ingredientNames.Add(ingredientType.Name);
        }
        return string.Join(", ", ingredientNames);
    }

    //***************************************************************************//
    // Get interaction distance for this customer
    //***************************************************************************//
    public float GetInteractionDistance()
    {
        return customerInteractionDistance;
    }

    //***************************************************************************//
    // Check if the delivered pizza matches what the customer wants
    //***************************************************************************//
    public void TryGivePizza(GameObject obj)
    {
        if (isLeaving) return;

        Pizza pizza = obj.GetComponent<Pizza>();
        if (pizza == null)
        {
            OrderIsIncorrect();
            return;
        }

        // Check if pizza has all required ingredients
        bool hasAll = true;
        foreach (System.Type ingType in wantedIngredients)
        {
            if (ingType == typeof(Sauce) && !pizza.HasSauce) hasAll = false;
            if (ingType == typeof(Cheese) && !pizza.HasCheese) hasAll = false;
            if (ingType == typeof(Bacon) && !pizza.HasBacon) hasAll = false;
            if (ingType == typeof(Pineapple) && !pizza.HasPineapple) hasAll = false;
            if (ingType == typeof(Pepperoni) && !pizza.HasPepperoni) hasAll = false;
        }

        // Also check that pizza doesn't have any unwanted ingredients
        if (hasAll)
        {
            if (!wantedIngredients.Contains(typeof(Bacon)) && pizza.HasBacon) hasAll = false;
            if (!wantedIngredients.Contains(typeof(Pineapple)) && pizza.HasPineapple) hasAll = false;
            if (!wantedIngredients.Contains(typeof(Pepperoni)) && pizza.HasPepperoni) hasAll = false;
        }

        if (hasAll)
            orderIsCorrect();
        else
            OrderIsIncorrect();
    }

    //***************************************************************************//
    // If order is delivered correctly
    //***************************************************************************//
    void orderIsCorrect()
    {
        Debug.Log($"{customerName}: Order is correct!");
        moodIndex = 2;  // Satisfied
        
        // Safely set customer mood material
        if (customerMoods != null && customerMoods.Length > moodIndex && customerMoods[moodIndex] != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = customerMoods[moodIndex];
            }
        }
        
        // Increment completed orders counter
        CustomerManager.AddCompletedOrder();
        
        playSfx(orderIsOkSfx);
        InstantiateMoney();
        StartCoroutine(leave());
    }

    //***************************************************************************//
    // If order is NOT delivered correctly
    //***************************************************************************//
    void OrderIsIncorrect()
    {
        Debug.Log($"{customerName}: Order is not correct!");
        moodIndex = 3;  // Angry
        
        // Safely set customer mood material
        if (customerMoods != null && customerMoods.Length > moodIndex && customerMoods[moodIndex] != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = customerMoods[moodIndex];
            }
        }
        
        // Take damage for giving wrong order
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
            Debug.Log($"Player lost 1 health for giving wrong order to {customerName}");
        }
        
        playSfx(orderIsNotOkSfx);
        StartCoroutine(leave());
    }

    //***************************************************************************//
    // Show money earned
    //***************************************************************************//
    void InstantiateMoney()
    {
        if (money3dText == null) return; // Safety check
        
        float leaveTime = Time.time;
        int remainedPatienceBonus = (int)Mathf.Round(customerPatience - (leaveTime - creationTime));
        int finalMoney = 5 + remainedPatienceBonus; // Base price + patience bonus

        GameObject money = Instantiate(money3dText, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        TextMesh textMesh = money.GetComponent<TextMesh>();
        if (textMesh != null)
        {
            textMesh.text = $"${finalMoney}";
        }
    }

    //***************************************************************************//
    // Leave routine with animations
    //***************************************************************************//
    IEnumerator leave()
    {
        // Safely release the seat
        if (manager != null && manager.availableSeatForCustomers != null && 
            mySeat >= 0 && mySeat < manager.availableSeatForCustomers.Length)
    {
        manager.availableSeatForCustomers[mySeat] = true;
            Debug.Log($"{customerName} is leaving and freeing seat {mySeat}");
        }

        if (isLeaving) yield break;
        isLeaving = true;

        // Immediately destroy ingredient icons when customer starts leaving
        if (ingredientIcons != null)
        {
            for (int i = 0; i < ingredientIcons.Length; i++)
            {
                if (ingredientIcons[i] != null)
                {
                    Destroy(ingredientIcons[i]);
                }
            }
            ingredientIcons = null;
            Debug.Log($"{customerName}: Ingredient icons removed");
        }

        // UI closing animations…
        StartCoroutine(animate(Time.time, patienceBarBG, 0.7f, 0.8f));
        yield return new WaitForSeconds(0.3f);  
        StartCoroutine(animate(Time.time, requestBubble, 0.75f, 0.95f));
        yield return new WaitForSeconds(0.4f);

        // Turn customer before leaving
        yield return StartCoroutine(TurnAroundAndLeave());

    }

    //***************************************************************************//
    // Turn customer 90 degrees left and make them leave
    // Customer received order, turns left and goes to exit
    //***************************************************************************//
    IEnumerator TurnAroundAndLeave()
    {
        // Save initial rotation
        Quaternion startRotation = transform.rotation;
        
        // Turn 90 degrees left (counter-clockwise)
        float turnDirection = -90f;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, turnDirection, 0);
        
        // Turn time (configurable in Inspector)
        float turnTime = 0f;
        
        Debug.Log($"{customerName} is turning left to leave...");
        
        // Smooth 90 degree left turn
        while (turnTime < turnAroundDuration)
        {
            turnTime += Time.deltaTime;
            float t = turnTime / turnAroundDuration;
            
            // Use SmoothStep for more natural turn (acceleration at start, deceleration at end)
            t = Mathf.SmoothStep(0f, 1f, t);
            
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }
        
        // Make sure turn is completed exactly
        transform.rotation = targetRotation;
        
        Debug.Log($"{customerName} turned left and is now walking to the exit");
        
        // Small pause after turn (customer "gathers thoughts")
        yield return new WaitForSeconds(0.2f);
        
        // Now go to exit (already turned)
        Vector3 doorPosition = exitPoint != null ? exitPoint.position : transform.position + Vector3.right * 10f;
        doorPosition.y = FLOOR_LEVEL; // Make sure door is also at correct height
        
        // Move to the door, keeping the height Y=1
        while (Vector3.Distance(transform.position, doorPosition) > 0.1f)
        {
            transform.position = MoveTowardsOnFloor(transform.position, doorPosition, customerMoveSpeed);
            yield return null;
        }
        
        // Small pause at the door to ensure trigger detection
        yield return new WaitForSeconds(0.2f);

        // Then move fully outside (off-screen)
        Vector3 offScreenPosition = doorPosition + ((doorPosition - transform.position).normalized * 10f);
        offScreenPosition.y = FLOOR_LEVEL; // And final position also at correct height
        
        while (Vector3.Distance(transform.position, offScreenPosition) > 0.1f)
        {
            transform.position = MoveTowardsOnFloor(transform.position, offScreenPosition, customerMoveSpeed);
            yield return null;
        }

        Destroy(gameObject);
    }


    //***************************************************************************//
    // Animate customer UI elements
    //***************************************************************************//
    IEnumerator animate(float _time, GameObject _go, float _in, float _out)
    {
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime * 10;
            _go.transform.localScale = new Vector3(
                Mathf.SmoothStep(_in, _out, t),
                _go.transform.localScale.y,
                _go.transform.localScale.z
            );
            yield return 0;
        }
        float r = 0.0f;
        if (_go.transform.localScale.x >= _out)
        {
            while (r <= 1.0f)
            {
                r += Time.deltaTime * 2;
                _go.transform.localScale = new Vector3(
                    Mathf.SmoothStep(_out, 0.01f, r),
                    _go.transform.localScale.y,
                    _go.transform.localScale.z
                );
                if (_go.transform.localScale.x <= 0.01f)
                    _go.SetActive(false);
                yield return 0;
            }
        }
    }

    //***************************************************************************//
    // Play sound effect
    //***************************************************************************//
    private void playSfx(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    //***************************************************************************//
    // Implementation of IInteractable interface
    //***************************************************************************//
    public void Interact()
    {
        if (isLeaving) return;

        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        if (playerHand == null) return;

        if (playerHand.IsHoldingItem)
        {
            GameObject heldItem = playerHand.HeldItem;
            Pizza pizza = heldItem.GetComponent<Pizza>();
            
            if (pizza != null)
            {
                // Check if pizza is cooked
                if (pizza.GetCookLevel() == CookState.Raw)
                {
                    playerHand.InvalidAction("This pizza is not cooked yet!", 2f);
                    return;
                }
                else if (pizza.GetCookLevel() == CookState.Burnt)
                {
                    playerHand.InvalidAction("This pizza is burnt! I don't want it!", 2f);
                    OrderIsIncorrect();
                    return;
                }

                // Pizza is cooked, now check ingredients
                TryGivePizza(heldItem);
                playerHand.Remove(); // Remove pizza from player's hand
            }
            else
            {
                playerHand.InvalidAction("I only want a pizza!", 2f);
            }
        }
        else
        {
            playerHand.ShowToast("You need to bring me a pizza!", 2f);
        }
    }

    public string getInteractionText()
    {
        if (isLeaving) return "";

        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        if (playerHand == null) return "";

        // Use only first name (first word) for brevity in UI
        string firstName = customerName.Split(' ')[0];

        if (playerHand.IsHoldingItem)
        {
            Pizza pizza = playerHand.HeldItem.GetComponent<Pizza>();
            if (pizza != null)
            {
                return $"Give pizza to {firstName}\nWants: {GetWantedIngredientsString()}";
            }
            else
            {
                return $"{firstName} only wants a pizza\nWants: {GetWantedIngredientsString()}";
            }
        }
        else
        {
            return $"{firstName} wants: {GetWantedIngredientsString()}";
        }
    }

    //***************************************************************************//
    // Apply separation force to keep customers from crowding together
    //***************************************************************************//
    private void ApplyCustomerSeparation()
    {
        Vector3 separationForceVector = CalculateSeparationForce();
        
        if (separationForceVector.magnitude > 0.01f)
        {
            // Apply the separation force to move away from other customers
            Vector3 newPosition = transform.position + separationForceVector * Time.deltaTime;
            
            // Keep the customer near their seat - don't let them wander too far
            float maxDistanceFromSeat = 2f;
            if (Vector3.Distance(newPosition, destination) <= maxDistanceFromSeat)
            {
                transform.position = MoveTowardsOnFloor(transform.position, newPosition, separationForce);
            }
        }
    }

    //***************************************************************************//
    // Calculate separation force based on nearby customers
    //***************************************************************************//
    private Vector3 CalculateSeparationForce()
    {
        Vector3 separationForceVector = Vector3.zero;
        int neighborCount = 0;

        // Find all other customers within separation radius
        CustomerController[] allCustomers = FindObjectsOfType<CustomerController>();
        
        foreach (CustomerController otherCustomer in allCustomers)
        {
            if (otherCustomer == this || otherCustomer.isLeaving) continue;
            
            float distance = Vector3.Distance(transform.position, otherCustomer.transform.position);
            
            if (distance < separationRadius && distance > 0.1f)
            {
                // Calculate direction away from other customer
                Vector3 directionAway = (transform.position - otherCustomer.transform.position).normalized;
                
                // Stronger force when closer
                float forceMultiplier = Mathf.Clamp01((separationRadius - distance) / separationRadius);
                
                // Only apply force if too close
                if (distance < separationDistance)
                {
                    separationForceVector += directionAway * forceMultiplier;
                    neighborCount++;
                }
            }
        }

        // Average the force if multiple neighbors
        if (neighborCount > 0)
        {
            separationForceVector /= neighborCount;
            separationForceVector = separationForceVector.normalized * separationForce;
        }

        return separationForceVector;
    }

    //***************************************************************************//
    // Draw separation zones in editor for debugging
    //***************************************************************************//
    private void OnDrawGizmos()
    {
        if (!enableSeparation) return;
        
        // Draw separation radius (yellow wire sphere)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        
        // Draw minimum separation distance (red wire sphere)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
        
        // Draw max distance from seat (green wire sphere around destination)
        if (isOnSeat)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(destination, 2f); // maxDistanceFromSeat
        }
    }
}