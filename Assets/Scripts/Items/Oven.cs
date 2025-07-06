using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Oven : MonoBehaviour, IInteractable
{
    [Header("UI Elements")]
    [SerializeField] private Image ovenTimerImage;
    [SerializeField] private TextMeshProUGUI ovenTimerText;

    [Header("Cooking Settings")]
    [SerializeField] private Vector3 pizzaPlacementPosition = new Vector3(-0.7f, 1.633f, 20.5f);
    [SerializeField] private float cookDuration = 5f;
    [SerializeField] private float burnDuration = 10f;


    private GameObject currentPizza;
    private float cookTimer;
    private bool isCooking;
    private CookState currentState = CookState.Raw;
    [SerializeField] private ParticleSystem smokeEffect;
    private AudioSource ovenTimerAudioSource;
    private Coroutine blinkingCoroutine;


    private void Start()
    {
        if (ovenTimerImage != null)
        {
            ovenTimerImage.fillAmount = 0f;
            ovenTimerImage.color = Color.green;
            ovenTimerImage.transform.localRotation = Quaternion.identity;
            ovenTimerImage.gameObject.SetActive(false);
        }
        if (ovenTimerText != null)
        {
            ovenTimerText.text = "00:00";
        }
        if (smokeEffect)
            ovenTimerAudioSource = GetComponent<AudioSource>();
    }
    public void Interact()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        if (playerHand == null) return;

        if (currentPizza == null && playerHand.IsHoldingItem)
        {
            Pizza pizza = playerHand.HeldItem.GetComponent<Pizza>();
            if (pizza != null)
            {
                playerHand.Drop();
                currentPizza = pizza.gameObject;

                // Place it in the world — don't parent or use local position
                currentPizza.transform.position = pizzaPlacementPosition;
                Vector3 defaultRotation = currentPizza.GetComponent<IPickable>()?.GetDefaultRotation() ?? Vector3.zero;
                currentPizza.transform.rotation = Quaternion.Euler(defaultRotation.x, defaultRotation.y, defaultRotation.z);

                StartCoroutine(CookPizza(pizza));
            }
            else
            {
                playerHand.InvalidAction("You can only place a pizza!", 2f);
            }
        }
        else if (currentPizza != null && playerHand.IsHoldingItem)
        {
            playerHand.InvalidAction("There's an item in the oven already!", 2f);
        }
        else if (currentPizza != null && !playerHand.IsHoldingItem)
        {
            currentPizza.transform.SetParent(null); // Just in case it ever was parented
            playerHand.PickUp(currentPizza);
            currentPizza = null;
            isCooking = false;
            blinkingCoroutine = null;
            StopAllCoroutines();
            ovenTimerAudioSource?.Stop();
            if (ovenTimerText != null)
            {
                ovenTimerText.color = Color.red;
                ovenTimerText.text = "00:00";
                ovenTimerText.enabled = true;
            }
            if (smokeEffect)
                smokeEffect.gameObject.SetActive(false);
        }
    }

    public string getInteractionText()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();

        if (currentPizza == null && playerHand != null && playerHand.IsHoldingItem)
        {
            if (playerHand.HeldItem.GetComponent<Pizza>() != null)
                return "Place pizza in oven";
        }
        else if (currentPizza != null)
        {
            return "Remove pizza from oven";
        }
        return "";
    }

    private IEnumerator BlinkCookedTimer()
    {
        Color originalColor = ovenTimerText.color;
        ovenTimerText.text = "READY!";
        while (currentState == CookState.Cooked)
        {
            ovenTimerText.enabled = !ovenTimerText.enabled;
            ovenTimerText.color = Color.yellow;
            yield return new WaitForSeconds(0.5f);
        }

        // Reset after blinking ends
        ovenTimerText.enabled = true;
        ovenTimerText.color = originalColor;
    }

    private IEnumerator CookPizza(Pizza pizza)
    {
        Debug.Log($"Starting to cook pizza: {pizza.name}");
        isCooking = true;
        currentState = pizza.GetCookLevel();
        cookTimer = currentState switch
        {
            CookState.Raw => 0f,
            CookState.Cooked => cookDuration,
            CookState.Burnt => burnDuration,
            _ => 0f // Default case
        };

        if (ovenTimerText != null)
        {
            float remainingTime = Mathf.Max(0f, burnDuration - cookTimer);
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(remainingTime);
            ovenTimerText.text = timeSpan.ToString(@"mm\:ss");
            Debug.Log($"Oven Timer Text: {ovenTimerText.text}");
            // ovenTimerText.color = Color.Lerp(Color.green, Color.red, cookTimer / burnDuration);
            ovenTimerText.gameObject.SetActive(true);
        }

        // Capture the pizza's original color
        Color originalColor = pizza.GetComponent<Renderer>().material.color;
        // burnt dough colour for the pizza
        Color burntColor = new Color32(128, 64, 26, 255); // Dark brown color for burnt pizza

        // 💥 Instant cook cheat logic
        if (CheatManager.Instance.IsCheatActive(CheatManager.Cheat.cheatName.InstantPizza))
        {
            pizza.SetCookState(CookState.Cooked);
            pizza.SetPizzaColor(new Color32(164, 143, 96, 255)); // Set to cooked color (darker brown)
            currentState = CookState.Cooked;

            if (ovenTimerText != null)
            {
                ovenTimerText.text = "READY!";
                ovenTimerText.color = Color.yellow;
                ovenTimerText.enabled = true;
            }

            if (ovenTimerAudioSource != null)
                ovenTimerAudioSource.Play();

            if (blinkingCoroutine == null)
                blinkingCoroutine = StartCoroutine(BlinkCookedTimer());

            yield break;
        }

        while (isCooking)
        {
            cookTimer += Time.deltaTime;

            float t = Mathf.Clamp01(cookTimer / burnDuration);
            pizza.SetPizzaColor(Color.Lerp(originalColor, burntColor, t));

            // Countdown display only before cooked
            if (ovenTimerText != null && cookTimer < cookDuration)
            {
                float remainingTime = Mathf.Max(0f, cookDuration - cookTimer);
                System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(remainingTime);
                ovenTimerText.text = timeSpan.ToString(@"mm\:ss");
            }

            // Start blinking + audio when pizza is cooked
            if (cookTimer >= cookDuration)
            {
                if (pizza.GetCookLevel() == CookState.Raw)
                {
                    pizza.SetPizzaColor(originalColor); // Reset to original color before cooking
                    pizza.SetCookState(CookState.Cooked);
                    currentState = CookState.Cooked;
                }

                if (ovenTimerAudioSource != null && !ovenTimerAudioSource.isPlaying)
                    ovenTimerAudioSource.Play();

                if (ovenTimerText != null && blinkingCoroutine == null)
                    blinkingCoroutine = StartCoroutine(BlinkCookedTimer());
            }

            // Stop blinking and transition to burnt
            if (cookTimer >= burnDuration)
            {
                if (pizza.GetCookLevel() == CookState.Cooked)
                {
                    pizza.SetPizzaColor(burntColor); // Set to burnt color
                    if (CheatManager.Instance.IsCheatActive(CheatManager.Cheat.cheatName.NoBurn))
                    {
                        pizza.SetCookState(CookState.Cooked);
                        currentState = CookState.Cooked;

                        if (ovenTimerText != null)
                        {
                            ovenTimerText.text = "READY!";
                            ovenTimerText.color = Color.yellow;
                        }
                        if (ovenTimerAudioSource != null && !ovenTimerAudioSource.isPlaying)
                            ovenTimerAudioSource.Play();

                        // Just wait for the next frame instead of looping instantly
                        yield return null;
                        continue;
                    }
                    pizza.SetCookState(CookState.Burnt);
                    currentState = CookState.Burnt;
                }

                if (ovenTimerText != null)
                {
                    if (blinkingCoroutine != null)
                    {
                        StopCoroutine(blinkingCoroutine);
                        blinkingCoroutine = null;
                    }
                    ovenTimerText.enabled = true;
                    ovenTimerText.color = Color.red;
                    ovenTimerText.text = "BURNT!";
                }

                if (smokeEffect)
                {
                    smokeEffect.gameObject.SetActive(true);
                    smokeEffect.Play();
                }

                break;
            }

            yield return null;
        }


        // Clean up
        // if (ovenTimerImage != null)
        // {
        //     ovenTimerImage.fillAmount = 0f;
        //     ovenTimerImage.color = Color.green;
        //     ovenTimerImage.transform.localRotation = Quaternion.identity;
        if (ovenTimerAudioSource != null && ovenTimerAudioSource.isPlaying)
        {
            ovenTimerAudioSource.Stop();
        }
    }
}