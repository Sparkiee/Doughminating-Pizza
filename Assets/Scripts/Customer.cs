using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour, IInteractable
{
    private Transform targetSeat;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private bool isMoving = false;
    private bool isServed = false;

    private GameObject orderBubble;
    private GameObject patienceBar;
    private GameObject patienceBarFill;

    private float patience;
    private float currentPatience;

    private float barInitialScaleX;
    private Vector3 barInitialPos;

    private Coroutine patienceCoroutine;

    private Transform exitPoint;

    private Health playerHealth;

    private Camera playerCamera;
    
    void Start()
    {
        this.playerHealth = FindObjectOfType<Health>();
        if (this.playerHealth == null)
        {
            Debug.LogError("Health component not found in the scene!");
        }

        this.playerCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving && isServed)
        {
            Debug.Log("Customer is leaving the restaurant.");
            // Move towards the exit point
            float step = moveSpeed * Time.deltaTime; // Calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, step);

            // Check if reached the exit point
            if (Vector3.Distance(transform.position, exitPoint.position) < 0.001f)
            {
                Destroy(gameObject); // Remove customer from the scene
            }
        }
        else if(isMoving && targetSeat != null)
        {
            // Move towards the target seat
            float step = moveSpeed * Time.deltaTime; // Calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetSeat.position, step);

            // Check if reached the target seat
            if (Vector3.Distance(transform.position, targetSeat.position) < 0.001f)
            {
                if(!isServed) {
                    OnArriveAtSeat();
                }
            }
        }

        this.orderBubble?.transform.LookAt(playerCamera.transform.position);
    }

    void Awake()
    {

    }
    
    public void WalkToCounter(Transform counter)
    {
        this.targetSeat = counter;
        this.isMoving = true;
    }

    public void Interact()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        if (playerHand == null) return;

        if(playerHand.IsHoldingItem && playerHand.HeldItem.TryGetComponent<Ingredient>(out Ingredient ingredient))
        {
            // Implement logic for when the player is holding an ingredient
            Debug.Log("Player is holding an ingredient.");
            if(ingredient.TryGetComponent<Pizza>(out Pizza pizza))
            {
                // Logic for serving pizza
                Debug.Log("Serving pizza to customer.");
                // CustomerManager.Instance.ServePizzaToCustomer(this, pizza);
            } else {
                // TODO: Implement logic for other ingredients
                playerHealth.TakeDamage(1);
            }
            playerHand.Remove();
            Leave();
        } else {
                playerHand.InvalidAction("You can't do this!", 2f);
        }
    }

    public string getInteractionText()
    {
        return "Interact with Customer";
    }

    private void OnArriveAtSeat()
    {
        // Logic for when the customer arrives at the seat
        Debug.Log("Customer has arrived at the seat.");
        isMoving = false;
        this.orderBubble?.SetActive(true);
        this.patienceBar?.SetActive(true);
        if(patienceCoroutine != null)
        {
            StopCoroutine(patienceCoroutine);
        }
        patienceCoroutine = StartCoroutine(PatienceCountdown());
    }

    public void AddOrderBubble(GameObject bubble)
    {
        if (bubble != null)
        {
            this.orderBubble = bubble;
            this.orderBubble.SetActive(false);
        }
    }

    public void SetPatience(float patience, GameObject patienceBar)
    {
        if (patienceBar == null)
        {
            Debug.LogWarning("Patience bar is null!");
            return;
        }
        this.patienceBar = patienceBar;
        this.patienceBarFill = patienceBar.transform.Find("PatienceBarFG").gameObject;
        patienceBar.SetActive(false);
        this.patience = patience;
        this.currentPatience = patience;

        this.barInitialScaleX = patienceBar.transform.localScale.x;
        this.barInitialPos = patienceBar.transform.localPosition;
    }

    public void SetExitPoint(Transform exitPoint)
    {
        this.exitPoint = exitPoint;
        Debug.Log($"Exit point set to: {exitPoint.position}");
    }

    private IEnumerator PatienceCountdown()
    {
        float elapsedTime = 0f;

        while (elapsedTime < patience && !isServed)
        {
            currentPatience = Mathf.Max(0, patience - elapsedTime);
            float ratio = Mathf.Clamp01(currentPatience / patience);

            if (patienceBar != null)
            {
                // Scale and shift the bar to simulate depletion from left to right
                float newScaleX = barInitialScaleX * ratio;

                patienceBar.transform.localScale = new Vector3(
                    newScaleX,
                    patienceBar.transform.localScale.y,
                    patienceBar.transform.localScale.z
                );

                float delta = (newScaleX - barInitialScaleX) * 0.5f;
                patienceBar.transform.localPosition = new Vector3(
                    barInitialPos.x - delta,
                    barInitialPos.y,
                    barInitialPos.z
                );
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Customer patience has run out!");

        // Optional: trigger fail or leave logic
        playerHealth?.TakeDamage(1);
        Destroy(gameObject); // or StartCoroutine(Leave())
    }

    public void Leave()
    {
        StopCoroutine(patienceCoroutine);
        StartCoroutine(RotateTowardsExit());
        this.orderBubble?.SetActive(false);
        this.patienceBar?.SetActive(false);
    }

    private IEnumerator RotateTowardsExit()
    {
        Vector3 direction = (exitPoint.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            yield return null;
        }
        
        transform.rotation = targetRotation; // Snap to final rotation
        isMoving = true;
        isServed = true;
        Debug.Log("Customer is now moving towards the exit.");
    }

}
