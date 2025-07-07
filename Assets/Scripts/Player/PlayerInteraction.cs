using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    // SerializeField allows you to modify the variable in the Unity Inspector
    // without making it public
    [SerializeField] private float interactionDistance = 20f; // Distance within which the player can interact with objects
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text interactionTextUI;// Text displayed when the player is close to an interactable object 
    [SerializeField] private GameObject interactionPanel; // Panel that contains the interaction text

    private IInteractable currentInteractable; // Reference to the current interactable object
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PlayerInteraction script started.");
        // Find the camera in the scene and assign it to playerCamera
        // playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("PlayerInteraction: Camera not found. Please assign a camera in the inspector.");
        }

        // Warm-up raycast to avoid initial lag
        Ray dummyRay = new Ray(transform.position, Vector3.forward);
        Physics.Raycast(dummyRay, out _);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInteractionCheck();
        HandleInteractionInput();
    }
    void HandleInteractionCheck()
    {
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, Color.red);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>()
                                      ?? hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                string interactionText = interactable.getInteractionText();

                if (!string.IsNullOrEmpty(interactionText))
                {
                    interactionTextUI.text = interactionText;
                    interactionPanel.SetActive(true); // ✅ Show the whole panel
                }
                else
                {
                    interactionPanel.SetActive(false); // 🔒 Hide if text is empty
                }

                return;
            }
        }
        if (currentInteractable != null)
        {
            currentInteractable = null;

        }
        interactionPanel.SetActive(false);
    }


    void HandleInteractionInput()
    {
        if (currentInteractable != null && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            currentInteractable.Interact();
        }
    }
}
