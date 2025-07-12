using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dough : Ingredient
{
    [SerializeField] private GameObject pizzaPrefab;
    public override void Interact()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand.IsHoldingItem && playerHand.HeldItem.GetComponent<Tool>() != null)
        {
            Tool item = playerHand.HeldItem.GetComponent<Tool>();
            if (item is RollingPin)
            {
                GameObject pizza = Instantiate(pizzaPrefab, this.gameObject.transform.position, Quaternion.identity);
                pizza.transform.SetParent(this.gameObject.transform.parent);
                float counterOffset = this.gameObject.GetComponent<IPickable>().GetCounterPositionOffset();
                Vector3 defaultRotation = pizza.GetComponent<IPickable>().GetDefaultRotation();
                pizza.transform.localRotation = Quaternion.Euler(defaultRotation.x, defaultRotation.y, defaultRotation.z);
                pizza.transform.localPosition = new Vector3(
                        pizza.transform.localPosition.x,
                        pizza.transform.localPosition.y - counterOffset,
                        pizza.transform.localPosition.z
                    );
                Destroy(this.gameObject);
            }
        }
        else
        {
            base.Interact();
        }
    }

    public override string getInteractionText()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand != null && playerHand.IsHoldingItem)
        {
            GameObject held = playerHand.HeldItem;
            Tool item = held.GetComponent<Tool>();
            if (item != null && item is RollingPin)
            {
                return "Roll out " + this.GetIngredientName();
            }
        }
        return "Pick " + this.GetIngredientName();
    }
}
