using UnityEngine;
using System.Collections.Generic;

public static class PizzaIngredientConfigs
{
    public static IngredientVisualConfig GetConfigForIngredient(IngredientType type, 
        GameObject saucePrefab, GameObject cheesePrefab, GameObject baconPrefab, 
        GameObject pineapplePrefab, GameObject pepperoniPrefab)
    {
        switch (type)
        {
            case IngredientType.Sauce:
                return new IngredientVisualConfig(
                    saucePrefab,
                    new Vector3(0f, 0.0124f, 0.003f),
                    Quaternion.Euler(-90f, 0f, 0f),
                    new Vector3(1.55f, 1.55f, 1.472f),
                    0.005f
                );
            
            case IngredientType.Cheese:
                return new IngredientVisualConfig(
                    cheesePrefab,
                    new Vector3(-0.001f, 0.024f, 0f),
                    Quaternion.Euler(0f, 0f, 0f),
                    new Vector3(0.2f, 0.1f, 0.2f),
                    0.01f
                );
            
            case IngredientType.Bacon:
                return new IngredientVisualConfig(
                    baconPrefab,
                    new Vector3(-0.013f, 0.02f, -0.02f),
                    Quaternion.Euler(90f, 0f, 0f),
                    new Vector3(0.19f, 0.23f, 0.2f),
                    0.03f
                );
            
            case IngredientType.Pineapple:
                return new IngredientVisualConfig(
                    pineapplePrefab,
                    new Vector3(-0.009f, 0.0471f, -0.004f),
                    Quaternion.Euler(0f, 0f, 0f),
                    new Vector3(0.15f, 0.15f, 0.15f),
                    0.03f
                );
            
            case IngredientType.Pepperoni:
                return new IngredientVisualConfig(
                    pepperoniPrefab,
                    new Vector3(0.0025f, 0.03f, 0.0f),
                    Quaternion.Euler(0f, 0f, 0f),
                    new Vector3(0.18f, 0.18f, 0.19f),
                    0.03f
                );
            
            default:
                return new IngredientVisualConfig(null, Vector3.zero, Quaternion.identity, Vector3.one, 0f);
        }
    }
}

public enum IngredientType
{
    Sauce,
    Cheese,
    Bacon,
    Pineapple,
    Pepperoni
}
