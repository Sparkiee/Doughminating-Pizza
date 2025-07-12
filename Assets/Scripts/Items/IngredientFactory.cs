using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientFactory : MonoBehaviour, IInteractable
{
    public static float planeOffset = 6.79f;
    [SerializeField] private GameObject prefab;

    public void Interact()
    {
        if (prefab == null)
        {
            Debug.LogWarning("No prefab assigned to IngredientFactory!");
            return;
        }
        
        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        if (playerHand == null)
        {
            Debug.LogError("Player hand not found!");
            return;
        }
        
        if (playerHand.IsHoldingItem) 
        {
            playerHand.InvalidAction("You are already holding an item!", 2f);
            return;
        }

        // Spawn ingredient directly into the player's hand
        Vector3 spawnPosition = transform.position;
        GameObject spawned = Instantiate(prefab, spawnPosition, Quaternion.identity);

        Ingredient ingredient = spawned.GetComponent<Ingredient>();
        if (ingredient != null)
        {
            playerHand.PickUp(spawned);
        }
    }

    public void Interact(Transform point) 
    {
        if (prefab == null) 
        {
            Debug.LogWarning("No prefab assigned to IngredientFactory!");
            return;
        }

        IPickable pickable = prefab.GetComponent<IPickable>();
        if (pickable == null)
        {
            Debug.LogWarning("Prefab doesn't have IPickable component!");
            return;
        }

        SpawnIngredientAtPoint(point, pickable);
    }

    private void SpawnIngredientAtPoint(Transform spawnPoint, IPickable pickable)
    {
        Vector3 spawnRotation = spawnPoint.rotation.eulerAngles;
        Vector3 itemRotation = pickable.GetDefaultRotation();
        float itemOffset = pickable.GetCounterPositionOffset();

        // Calculate final rotation
        Quaternion finalRotation = Quaternion.Euler(
            spawnRotation.x + itemRotation.x,
            spawnRotation.y + itemRotation.y,
            spawnRotation.z + itemRotation.z
        );

        // 1. Instantiate WITHOUT parent
        GameObject spawned = Instantiate(prefab, spawnPoint.position, finalRotation);
        
        // 2. THEN parent it safely using worldPositionStays=true
        spawned.transform.SetParent(spawnPoint, true);

        // 3. AFTER parenting, adjust the local position
        spawned.transform.localPosition = new Vector3(
            spawned.transform.localPosition.x, 
            spawned.transform.localPosition.y + itemOffset + planeOffset,
            spawned.transform.localPosition.z
        );
    }


    public string getInteractionText()
    {
        if (prefab == null)
            return "No ingredient assigned";

        Ingredient ingredient = prefab.GetComponent<Ingredient>();
        if (ingredient != null)
        {
            return "Grab " + ingredient.GetIngredientName();
        }
        return "Grab an ingredient";
    }
}
