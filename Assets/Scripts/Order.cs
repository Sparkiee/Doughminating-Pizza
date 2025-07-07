using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{

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



    void Start() {
        // Randomize the order when the script starts
        RandomizeOrder();
        // Debug.Log("Order initialized with ingredients: " + string.Join(", ", ingredients));
    }

    private void RandomizeOrder() {
        ingredients.Clear();

        // Add required ingredients
        foreach (IngredientType required in requiredIngredients)
        {
            ingredients.Add(required);
        }
        // Randomly add optional ingredients
        int optionalCount = Random.Range(0, optionalIngredients.Count + 1); // Random
        for (int i = 0; i < optionalCount; i++)
        {
            IngredientType randomOptional = optionalIngredients[Random.Range(0, optionalIngredients.Count)];
            AddIngredient(randomOptional);
        }

        Debug.Log("Order randomized with ingredients: " + string.Join(", ", ingredients));
    }

    private void AddIngredient(IngredientType ingredient)
    {
        if (!ingredients.Contains(ingredient))
        {
            ingredients.Add(ingredient);
        }
    }

    // public bool MatchesOrder(Pizza pizza)
    // {
    //     List<string> pizzaIngredients = pizza.GetIngredientNames();

    //     // Check that all required ingredients are present
    //     foreach (string required in requiredIngredients)
    //     {
    //         if (!pizzaIngredients.Contains(required))
    //         {
    //             Debug.Log($"Missing required ingredient: {required}");
    //             return false;
    //         }
    //     }

    //     // Check that no extra ingredients are present
    //     foreach (string ingredient in pizzaIngredients)
    //     {
    //         if (!ingredients.Contains(ingredient))
    //         {
    //             Debug.Log($"Extra ingredient on pizza not in order: {ingredient}");
    //             return false;
    //         }
    //     }

    //     Debug.Log("Pizza matches the order!");
    //     return true;
    // }

}
