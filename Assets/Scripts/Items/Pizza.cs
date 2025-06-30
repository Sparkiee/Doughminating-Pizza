using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza : Ingredient
{
    [SerializeField] private GameObject pizzaUI;
    [SerializeField] private CookState CookLevel = CookState.Raw;

    [SerializeField] private bool hasSauce = false;
    [SerializeField] private bool hasCheese = false;
    [SerializeField] private bool hasBacon = false;
    [SerializeField] private bool hasPineapple = false;
    [SerializeField] private bool hasPepperoni = false;
    [SerializeField] private HashSet<Ingredient> ingredients = new HashSet<Ingredient>();

    [Header("Visual Ingredient Prefabs")]
    [SerializeField] private GameObject saucePrefab;
    [SerializeField] private GameObject cheesePrefab;
    [SerializeField] private GameObject baconPrefab;
    [SerializeField] private GameObject pineapplePrefab;
    [SerializeField] private GameObject pepperoniPrefab;

    // Store references to spawned visual ingredients
    private Dictionary<IngredientType, GameObject> visualIngredients = new Dictionary<IngredientType, GameObject>();

    private Renderer pizzaRenderer;
    private Material pizzaMaterial;

    // Public properties to access ingredient states
    public bool HasSauce => hasSauce;
    public bool HasCheese => hasCheese;
    public bool HasBacon => hasBacon;
    public bool HasPineapple => hasPineapple;
    public bool HasPepperoni => hasPepperoni;
    public CookState GetCookLevel() => CookLevel;

    void Start()
    {
        Debug.Log("Pizza created with cook level: " + CookLevel);

        // Initialize panel UI
        if (pizzaUI != null)
        {
            GameObject uiInstance = Instantiate(pizzaUI);
            uiInstance.transform.SetParent(this.transform);
            uiInstance.transform.localPosition = new Vector3(0, 0, 1) * 1f;
            pizzaUI = uiInstance;
        }

        // Initialize renderer and material for color changing
        pizzaRenderer = GetComponent<Renderer>();
        if (pizzaRenderer != null)
        {
            // Use .material to get a unique instance for this pizza
            pizzaMaterial = pizzaRenderer.material;
        }
    }

    public void SetPizzaColor(Color color)
    {
        if (pizzaMaterial != null)
            pizzaMaterial.color = color;
    }

    public override void Interact()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand != null && playerHand.IsHoldingItem)
        {
            GameObject held = playerHand.HeldItem;
            Ingredient ingredient = held.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                if (this.CookLevel != CookState.Raw)
                {
                    playerHand.InvalidAction("Pizza is " + this.CookLevel.ToString().ToLower() + "! Can't add!", 2f);
                    return;
                }

                if (TryAddIngredient(ingredient))
                {
                    playerHand.Remove();
                    AddIngredient(ingredient);
                }
                else
                {
                    playerHand.InvalidAction("You can't add " + ingredient.GetIngredientName() + " to the pizza!", 2f);
                }
            }

            Tool tool = held.GetComponent<Tool>();
            if (tool != null)
            {
                playerHand.InvalidAction("You can't use " + tool.GetToolName() + " on the pizza!", 2f);
            }
        }
        else if (playerHand != null && !playerHand.IsHoldingItem)
        {
            base.Interact(); // Call the base interact method to pick up the pizza
        }
    }

    private bool TryAddIngredient(Ingredient ingredient)
    {
        switch (ingredient)
        {
            case Sauce _ when !hasSauce:
                hasSauce = true;
                SpawnVisualIngredient(IngredientType.Sauce);
                return true;
            
            case Cheese _ when !hasCheese:
                hasCheese = true;
                SpawnVisualIngredient(IngredientType.Cheese);
                return true;
            
            case Bacon _ when !hasBacon:
                hasBacon = true;
                SpawnVisualIngredient(IngredientType.Bacon);
                return true;
            
            case Pineapple _ when !hasPineapple:
                hasPineapple = true;
                SpawnVisualIngredient(IngredientType.Pineapple);
                return true;
            
            case Pepperoni _ when !hasPepperoni:
                hasPepperoni = true;
                SpawnVisualIngredient(IngredientType.Pepperoni);
                return true;
            
            default:
                return false;
        }
    }


    private void SpawnVisualIngredient(IngredientType ingredientType)
    {
        IngredientVisualConfig config = PizzaIngredientConfigs.GetConfigForIngredient(
            ingredientType, saucePrefab, cheesePrefab, baconPrefab, pineapplePrefab, pepperoniPrefab);

        if (config.prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {ingredientType}!");
            return;
        }

        // Calculate position using the pizza's transform
        Vector3 localPosition = transform.InverseTransformPoint(transform.position + config.relativeOffset);
        localPosition.z += config.heightAdjustment;
        Vector3 worldPosition = transform.TransformPoint(localPosition);

        // Instantiate and set up the ingredient
        GameObject visualIngredient = Instantiate(config.prefab, worldPosition, config.worldRotation);
        visualIngredient.transform.localScale = config.localScale;
        visualIngredient.transform.SetParent(this.transform, true);

        // Store reference to the visual ingredient
        visualIngredients[ingredientType] = visualIngredient;

        Debug.Log($"Spawned visual {ingredientType} at world position: {worldPosition}, world rotation: {config.worldRotation.eulerAngles}");
    }

    public void Cook()
    {
        if (this.CookLevel == CookState.Raw)
        {
            this.CookLevel = CookState.Cooked;
        }
        else if (this.CookLevel == CookState.Cooked)
        {
            this.CookLevel = CookState.Burnt;
        }

        pizzaUI.GetComponent<PizzaUIController>().setCookLevel(this.CookLevel);
    }

    public void SetCookState(CookState newState)
    {
        CookLevel = newState;
        Debug.Log($"Pizza cook state changed to: {newState}");

        if (pizzaUI != null)
        {
            pizzaUI.GetComponent<PizzaUIController>().setCookLevel(this.CookLevel);
        }
    }

    public override string getInteractionText()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (!playerHand.IsHoldingItem)
        {
            return "Pick up the pizza";
        }
        else
        {
            if (playerHand.HeldItem.GetComponent<Ingredient>() != null)
            {
                Ingredient heldIngredient = playerHand.HeldItem.GetComponent<Ingredient>();
                return "Add " + heldIngredient.GetIngredientName() + " to the pizza";
            }
            if (playerHand.HeldItem.GetComponent<Tool>() != null)
            {
                return "Use " + playerHand.HeldItem.GetComponent<Tool>().GetToolName() + " on the pizza";
            }
        }
        return "Pick " + GetIngredientName();
    }

    public void AddIngredient(Ingredient ingredient)
    {
        pizzaUI.GetComponent<PizzaUIController>().addIngredient(ingredient);
    }
}