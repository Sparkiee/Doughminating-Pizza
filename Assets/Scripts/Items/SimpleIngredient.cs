using UnityEngine;

public abstract class SimpleIngredient : Ingredient
{
    public override string getInteractionText()
    {
        return "Pick " + GetIngredientName();
    }
}
