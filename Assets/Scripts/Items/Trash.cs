using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand != null && playerHand.IsHoldingItem)
        {
            GameObject held = playerHand.HeldItem;
            Ingredient ingredient = held.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                // Perform trashing logic here
                playerHand.Remove(); // Remove the item from the player's hand
            }
        }
    }

    public string getInteractionText()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand != null && playerHand.IsHoldingItem)
        {
            GameObject held = playerHand.HeldItem;
            Ingredient ingredient = held.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                return "Trash " + ingredient.GetIngredientName();
            }
        }
        return "";
    }
}
