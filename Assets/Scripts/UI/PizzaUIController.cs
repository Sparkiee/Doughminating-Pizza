using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

public class PizzaUIController : MonoBehaviour
{
    private Transform player;
    private Transform panel;
    private UnityEngine.UI.Image cookCircle;
    
    // Dictionary to store ingredient UI elements for easier management
    private Dictionary<System.Type, SVGImage> ingredientUIElements = new Dictionary<System.Type, SVGImage>();

    void Start()
    {
        InitializeComponents();
        InitializeIngredientUI();
    }

    void Update()
    {
        UpdatePanelRotation();
    }

    private void InitializeComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        panel = transform.Find("Panel");
        
        Transform cookCircleTransform = transform.Find("Panel/CookCircle");
        if (cookCircleTransform != null)
        {
            cookCircle = cookCircleTransform.GetComponent<UnityEngine.UI.Image>();
        }
    }

    private void InitializeIngredientUI()
    {
        // Initialize ingredient UI elements
        AddIngredientUIElement<Sauce>("Panel/Sauce");
        AddIngredientUIElement<Cheese>("Panel/Cheese");
        AddIngredientUIElement<Bacon>("Panel/Bacon");
        AddIngredientUIElement<Pineapple>("Panel/Pineapple");
        AddIngredientUIElement<Pepperoni>("Panel/Pepperoni");
    }

    private void AddIngredientUIElement<T>(string path) where T : Ingredient
    {
        Transform elementTransform = transform.Find(path);
        if (elementTransform != null)
        {
            SVGImage svgImage = elementTransform.GetComponent<SVGImage>();
            if (svgImage != null)
            {
                ingredientUIElements[typeof(T)] = svgImage;
                UIIngredientManager.InitializeIngredientUI(svgImage);
            }
        }
    }

    private void UpdatePanelRotation()
    {
        if (player != null && panel != null)
        {
            Vector3 lookDirection = panel.position - player.position;
            lookDirection.y = 0; // Remove vertical tilt if desired
            panel.forward = lookDirection.normalized;
        }
    }

    public void addIngredient(Ingredient ingredient)
    {
        System.Type ingredientType = ingredient.GetType();
        
        if (ingredientUIElements.TryGetValue(ingredientType, out SVGImage uiElement))
        {
            UIIngredientManager.SetIngredientVisibility(uiElement, true);
        }
    }

    public void setCookLevel(CookState cookState)
    {
        if (cookCircle != null)
        {
            cookCircle.color = UIIngredientManager.GetCookLevelColor(cookState);
        }
    }
}
