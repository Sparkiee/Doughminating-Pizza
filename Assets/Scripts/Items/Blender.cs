using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blender : Tool
{
    private bool isBlending = false;
    private Animator animator;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Blender: Animator component not found!");
        }
    }

    public override void Interact()
    {
        if(isBlending) return; // Prevent multiple interactions at once
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand != null && playerHand.IsHoldingItem)
        {
            GameObject held = playerHand.HeldItem;
            Ingredient ingredient = held.GetComponent<Ingredient>();
            if (ingredient != null && ingredient is Tomato)
            {
                isBlending = true; // Set the flag to true
                if (animator != null)
                {
                    animator.SetTrigger("Blend"); // Trigger the blending animation
                }
                StartCoroutine(Blend(playerHand, ingredient));
            }
            else
            {
                playerHand.InvalidAction("You can only blend tomatoes!", 2f);
            }
        }
    }

    private IEnumerator Blend(PlayerHand playerHand, Ingredient ingredient)
    {
        // Logic for blending the ingredient
        // This could involve changing the ingredient's state or spawning a new blended item
        // For example, you might want to create a new GameObject for the blended item and set its properties
        // You can also play a blending animation or sound here if needed
        playerHand.Remove();
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play(); // Play the blending sound
            yield return new WaitForSeconds(audio.clip.length);
        } else yield return null;

        // Spawn the blended item (e.g., Tomato Sauce)
        IngredientFactory factory = GetComponent<IngredientFactory>();
        if (factory != null)
        {
            factory.Interact(spawnPoint); // Triggers the sauce spawn!
        }
        if(animator != null)
        {
            animator.SetTrigger("StopBlend"); // Trigger the blend complete animation
        }
        isBlending = false; // Reset the flag after blending is done
    }

    public override string getInteractionText()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player").GetComponent<PlayerHand>();
        if (playerHand != null && playerHand.IsHoldingItem)
        {
            GameObject held = playerHand.HeldItem;
            Ingredient ingredient = held.GetComponent<Ingredient>();
            if (ingredient != null && ingredient is Tomato)
            {
                return "Blend " + ingredient.GetIngredientName();
            }
        }

        return "";
    }
}
