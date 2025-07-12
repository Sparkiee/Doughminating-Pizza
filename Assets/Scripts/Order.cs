using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class IngredientData
{
    public IngredientType ingredientType;
    public Sprite ingredientImage;
}

public class Order : MonoBehaviour
{
    [Header("Ingredient Data")]
    public List<IngredientData> ingredientDataList = new List<IngredientData>();

    private List<IngredientType> ingredients = new List<IngredientType>();
    private List<IngredientType> optionalIngredients = new List<IngredientType>
    {
        IngredientType.Bacon,
        IngredientType.Pepperoni,
        IngredientType.Pineapple
    };
    private List<IngredientType> requiredIngredients = new List<IngredientType>
    {
        IngredientType.Cheese,
        IngredientType.Sauce
    };
    private GameObject orderBubble;
    private GameObject bubbleIcons;

    void Start() {
        // Do not randomize order here; wait until order bubble is initialized
    }

    public void InitializeOrderBubble(GameObject bubble) {
        this.orderBubble = bubble;
        Transform[] children = orderBubble.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.CompareTag("IngredientIcons"))
            {
                bubbleIcons = child.gameObject;
                break;
            }
        }

        if (bubbleIcons == null)
        {
            Debug.LogWarning("IngredientIcons child not found on order bubble!");
        } else {
            RandomizeOrder();
        }
    }

    private void RandomizeOrder() {
        ingredients.Clear();

        // Clear old icons to prevent prefab bloat/corruption
        if (bubbleIcons != null)
        {
            foreach (Transform child in bubbleIcons.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Add required ingredients
        foreach (IngredientType required in requiredIngredients)
        {
            AddIngredient(required);
        }
        // Randomly add optional ingredients
        int optionalCount = Random.Range(0, optionalIngredients.Count + 1); // Random
        for (int i = 0; i < optionalCount; i++)
        {
            IngredientType randomOptional = optionalIngredients[Random.Range(0, optionalIngredients.Count)];
            AddIngredient(randomOptional);
        }
    }

    private void AddIngredient(IngredientType ingredient)
    {
        if (!ingredients.Contains(ingredient) && bubbleIcons != null)
        {
            ingredients.Add(ingredient);
            GameObject ingredientIcon = new GameObject("Icon_" + ingredient.ToString());
            Image iconImage = ingredientIcon.AddComponent<Image>();
            iconImage.sprite = Resources.Load<Sprite>($"UI/Icons/{ingredient.ToString().ToLower()}");
            iconImage.rectTransform.sizeDelta = new Vector2(64, 64); // Set size of the icon
            iconImage.preserveAspect = true;
            iconImage.transform.SetParent(bubbleIcons.transform, false); // Set parent to the bubble icons
            iconImage.transform.localPosition = Vector3.zero; // Adjust as needed
        }
    }

    public bool ComparePizzaToOrder(Pizza pizza)
    {
        if (pizza == null) return false;
        if(pizza.GetCookLevel() != CookState.Cooked) return false;
        if(pizza.GetIngredientNames().Count < requiredIngredients.Count) return false;

        // Get the actual pizza ingredients
        List<string> pizzaIngredients = pizza.GetIngredientNames();
        for (int i = 0; i < ingredients.Count; i++)
        {
            if (!pizzaIngredients.Contains(ingredients[i].ToString()))
            {
                return false;
            }
        }
        return true;
    }

    public void CreateTutorialOrder() {
        ingredients.Clear();
        if (bubbleIcons != null)
        {
            foreach (Transform child in bubbleIcons.transform)
            {
                Destroy(child.gameObject);
            }
        }
        AddIngredient(IngredientType.Cheese);
        AddIngredient(IngredientType.Sauce);
        AddIngredient(IngredientType.Pineapple);
    }
}
