using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float floatHeight = 0.0001f; // Adjust this value to control the floating height
    [SerializeField] private float floatSpeed = 2f; // Adjust this value to control the floating speed
    [SerializeField] private RectTransform toastPanel;
    [SerializeField] private TMPro.TextMeshProUGUI toastText;
    private Vector2 originalToastPosition;
    private Coroutine toastCoroutine;

    private GameObject heldItem;
    public bool IsHoldingItem => heldItem != null;
    public GameObject HeldItem => heldItem;
    private void Start()
    {
        if (toastPanel != null)
        {
            originalToastPosition = toastPanel.anchoredPosition;
            toastPanel.gameObject.SetActive(false);
        }
    }

    public void PickUp(GameObject item)
    {
        IPickable pickable = item.GetComponent<IPickable>();
        if (pickable != null)
        {
            if (heldItem != null)
            {
                if (heldItem.GetComponent<Ingredient>() != null)
                {
                    // If already holding an item, show a message and shake the held item
                    InvalidAction("Already holding an item: " + heldItem.GetComponent<Ingredient>().GetIngredientName(), 2f);
                    return;
                }
                else if (heldItem.GetComponent<Tool>() != null)
                {
                    InvalidAction("You can't use " + heldItem.GetComponent<Tool>().GetToolName() + " like this!", 2f);
                    return;
                }
                return;
            }

            heldItem = item;
            heldItem.transform.SetParent(holdPoint);
            heldItem.transform.localPosition = item.GetComponent<IPickable>().GetHandPositionOffset();
            heldItem.transform.localRotation = Quaternion.Euler(item.GetComponent<IPickable>().GetHandRotationOffset());
        }

    }

    public void InvalidAction(string message, float duration = 2f)
    {
        if (heldItem != null)
        {
            ShowToast(message, duration);
            ShakeHeldItem();
        }
        else
        {
            ShowToast("You are not holding anything!", duration);
        }
    }

    public void ShakeHeldItem(float intensity = 0.005f, float duration = 0.3f)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        if (heldItem == null) yield break;

        Transform shakeTarget = heldItem.transform;
        Vector3 originalPosition = shakeTarget.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (shakeTarget == null) yield break;

            float xOffset = Random.Range(-intensity, intensity);
            float yOffset = Random.Range(-intensity, intensity);
            shakeTarget.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (shakeTarget != null)
            shakeTarget.localPosition = originalPosition;
    }
    
    public void Remove()
    {
        if (heldItem != null)
        {

            Collider playerCollider = GetComponent<Collider>();
            Collider itemCollider = heldItem.GetComponent<Collider>();
            if (playerCollider != null && itemCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, itemCollider);
            }
            heldItem.transform.SetParent(null);
            Destroy(heldItem);
            heldItem = null;
        }
    }

    public void Drop()
    {
        if (heldItem != null)
        {
            heldItem.transform.SetParent(null);
            // Move the dropped item a short distance in front of the player to avoid overlap
            Vector3 dropOffset = transform.forward * 1.5f; // Just a little bit in front
            heldItem.transform.position = holdPoint.position + dropOffset;
            heldItem.transform.rotation = Quaternion.identity;

            // Optional: ignore collision between player and item if needed
            Collider playerCollider = GetComponent<Collider>();
            Collider itemCollider = heldItem.GetComponent<Collider>();
            if (playerCollider != null && itemCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, itemCollider);
            }

            heldItem = null;
        }
    }
    public void MoveItemUpDown()
    {
        if (heldItem != null)
        {
            Vector3 newPosition = heldItem.transform.localPosition;
            newPosition.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight; // Adjust the multiplier for height
            heldItem.transform.localPosition = newPosition;
        }
    }

    public void ShowToast(string message, float duration = 2f)
    {
        if (toastPanel == null || toastText == null)
        {
            Debug.LogWarning("Toast UI elements are not assigned.");
            return;
        }

        if (toastCoroutine != null)
        {
            StopCoroutine(toastCoroutine); // 🔥 cancel any current toast
        }
        // Reset position to original
        toastPanel.anchoredPosition = originalToastPosition;
        toastCoroutine = StartCoroutine(ShowToastCoroutine(message, duration));
    }

    private IEnumerator ShowToastCoroutine(string message, float duration)
    {
        toastText.text = message;

        // Start from hidden state
        toastPanel.gameObject.SetActive(true);
        Color textColor = toastText.color;
        textColor.a = 0f;
        toastText.color = textColor;

        Vector2 startPos = toastPanel.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, 30f);

        float fadeTime = 0.3f;
        float elapsed = 0f;

        // Fade in & slide up
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);

            textColor.a = t;
            toastText.color = textColor;
            toastPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // Fade out & slide down
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);

            textColor.a = 1f - t;
            toastText.color = textColor;
            toastPanel.anchoredPosition = Vector2.Lerp(endPos, startPos, t);

            yield return null;
        }

        toastPanel.gameObject.SetActive(false);
    }

}
