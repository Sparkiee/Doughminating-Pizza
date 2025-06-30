using UnityEngine;
using Unity.VectorGraphics;
using System.Collections.Generic;

public static class UIIngredientManager
{
    public static void SetIngredientVisibility(SVGImage uiElement, bool isVisible)
    {
        if (uiElement == null) return;
        
        Color currentColor = uiElement.color;
        uiElement.color = new Color(currentColor.r, currentColor.g, currentColor.b, isVisible ? 1f : 0f);
    }

    public static void InitializeIngredientUI(SVGImage uiElement)
    {
        SetIngredientVisibility(uiElement, false);
    }

    public static Color32 GetCookLevelColor(CookState cookState)
    {
        return cookState switch
        {
            CookState.Raw => new Color32(0xF5, 0xE1, 0xA4, 0xFF),    // Peach color
            CookState.Cooked => new Color32(0xBF, 0x90, 0x01, 0xFF), // Golden brown
            CookState.Burnt => new Color32(0x4D, 0x3D, 0x0c, 0xFF),  // Dark brown
            _ => Color.white
        };
    }
}
