using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour, IInteractable
{
    private Transform targetSeat;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private bool isMoving = false;
    private bool isLeaving = false;
    private bool hasFailed = true;

    private GameObject orderBubble;
    private GameObject patienceBar;

    private float patience;
    private float currentPatience;
    private CustomerSeat assignedSeat;

    private Coroutine patienceCoroutine;

    private Transform exitPoint;

    private Health playerHealth;

    private Camera playerCamera;

    private AudioClip successfulOrderSound;
    private AudioClip failedOrderSound;

    private Animator animator;

    private bool isTutorialCustomer = false;
    public bool isServed = false;

    void Start()
    {
        this.animator = transform.Find("BaseCharacter")?.GetComponent<Animator>();
        if (this.animator == null)
        {
            Debug.LogError("Animator component not found on BaseCharacter!");
        }
        else
        {
            Debug.Log("Animator component found on BaseCharacter.");
        }
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
        if (isMoving && isLeaving)
        {
            // Move towards the exit point
            float step = 2 * moveSpeed * Time.deltaTime; // Calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, step);

            // Check if reached the exit point
            if (Vector3.Distance(transform.position, exitPoint.position) < 0.001f)
            {
                this.assignedSeat.isOccupied = false; // Mark the seat as unoccupied
                // Destroy(gameObject); // Remove customer from the scene
                GameManager.Instance.CustomerServed(gameObject, this.hasFailed);
            }
        }
        else if (isMoving && targetSeat != null)
        {
            // Move towards the target seat
            float step = moveSpeed * Time.deltaTime; // Calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetSeat.position, step);

            // Check if reached the target seat
            if (Vector3.Distance(transform.position, targetSeat.position) < 0.001f)
            {
                if (!isLeaving)
                {
                    OnArriveAtSeat();
                }
            }
        }
        else if (!isMoving && !isLeaving && !isServed)
        {
            // If not moving, look at the player camera
            if (playerCamera != null)
            {
                transform.LookAt(playerCamera.transform.position);
            }
        }

        this.orderBubble?.transform.LookAt(playerCamera.transform.position);
        this.patienceBar?.transform.LookAt(playerCamera.transform.position);
    }


    public void SetTutorialCustomer()
    {
        this.isTutorialCustomer = true;
    }
    void Awake()
    {

    }

    public void WalkToCounter(CustomerSeat seat)
    {
        this.targetSeat = seat.location;
        this.isMoving = true;
        this.assignedSeat = seat;
        this.transform.LookAt(targetSeat.position);
    }

    public void Interact()
    {
        if (isMoving || isLeaving || isServed) return;
        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        if (playerHand == null) return;
        TryGetComponent<AudioSource>(out AudioSource audioSource);
        if (CheatManager.Instance.IsCheatActive(CheatManager.Cheat.cheatName.AlwaysApprove))
        {
            ;
            this.patienceBar?.SetActive(false);
            this.orderBubble?.SetActive(false);
            this.isServed = true;
            this.hasFailed = false;
            if (this.animator != null)
            {
                this.animator.SetTrigger("Celebrate");
                WaitForSeconds wait = new WaitForSeconds(1f);
                StartCoroutine(WaitAndLeave(wait));
            }
            if (audioSource != null && successfulOrderSound != null)
            {
                audioSource.PlayOneShot(successfulOrderSound);
            }
            playerHand.Remove();
            return;
        }

        if (playerHand.IsHoldingItem && playerHand.HeldItem.TryGetComponent<Ingredient>(out Ingredient ingredient))
        {
            this.isServed = true;

            if (isTutorialCustomer)
            {
                if (ingredient.TryGetComponent<Pizza>(out Pizza tutorialPizza))
                {
                    if (tutorialPizza.GetCookLevel() != CookState.Cooked && !tutorialPizza.HasSauce && !tutorialPizza.HasCheese && !tutorialPizza.HasPineapple)
                    {
                        playerHand.InvalidAction("You need to cook the pizza and add sauce and cheese!", 2f);
                        return;
                    }
                }
                else return; // Only pizzas are allowed for tutorial customers
            }
            this.patienceBar?.SetActive(false);
            this.orderBubble?.SetActive(false);
            // Logic for when the player is holding an ingredient
            if (ingredient.TryGetComponent<Pizza>(out Pizza pizza))
            {
                // Serving pizza
                bool result = (GetComponent<Order>()?.ComparePizzaToOrder(pizza)) ?? false;
                playerHand.Remove();
                if (result)
                {
                    this.hasFailed = false;
                    if (this.animator != null)
                    {
                        this.animator.SetTrigger("Celebrate");
                        WaitForSeconds wait = new WaitForSeconds(1f);
                        StartCoroutine(WaitAndLeave(wait));
                    }
                    if (audioSource != null && successfulOrderSound != null)
                    {
                        audioSource.PlayOneShot(successfulOrderSound);
                    }
                    if (isTutorialCustomer)
                    {
                        TutorialManager.Instance.EndTutorial();
                    }
                    return;
                }
                playerHealth.TakeDamage(1);
            }
            else
            {
                // TODO: Implement logic for other ingredients
                playerHealth.TakeDamage(1);
            }
            if (audioSource != null && failedOrderSound != null)
            {
                audioSource.PlayOneShot(failedOrderSound);
            }
            playerHand.Remove();
            Leave();
        }
        else
        {
            playerHand.InvalidAction("You can't do this!", 2f);
        }
    }

    private IEnumerator WaitAndLeave(WaitForSeconds wait)
    {
        yield return wait;
        Leave();
    }

    public string getInteractionText()
    {
        return "Interact with Customer";
    }

    private void OnArriveAtSeat()
    {
        // Logic for when the customer arrives at the seat
        isMoving = false;
        this.orderBubble?.SetActive(true);
        if (patienceCoroutine != null)
        {
            StopCoroutine(patienceCoroutine);
        }
        if (this.patienceBar != null)
        {
            this.patienceBar?.SetActive(true);
            patienceCoroutine = StartCoroutine(PatienceCountdown());
        }
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
        // this.patienceBarFill = patienceBar.transform.Find("PatienceBarFG").gameObject;
        patienceBar.SetActive(false);
        this.patience = patience;
        this.currentPatience = patience;
    }

    public float GetPatience()
    {
        return this.currentPatience;
    }

    public void SetExitPoint(Transform exitPoint)
    {
        this.exitPoint = exitPoint;
        Debug.Log($"Exit point set to: {exitPoint.position}");
    }

    public void SetSuccessfulOrderSound(AudioClip sound)
    {
        this.successfulOrderSound = sound;
    }

    public void SetFailedOrderSound(AudioClip sound)
    {
        this.failedOrderSound = sound;
    }

    private IEnumerator PatienceCountdown()
    {
        float elapsedTime = 0f;

        while (elapsedTime < patience && !isLeaving)
        {
            currentPatience = Mathf.Max(0, patience - elapsedTime);
            float ratio = Mathf.Clamp01(currentPatience / patience);

            patienceBar.GetComponent<MoodController>()?.setPatience(ratio);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Customer patience has run out!");

        // Optional: trigger fail or leave logic
        playerHealth?.TakeDamage(1);
        Leave();
    }

    public void Leave()
    {
        this.orderBubble?.SetActive(false);
        if (this.patienceBar != null)
        {
            this.patienceBar.SetActive(false);
            StopCoroutine(patienceCoroutine);
        }

        StartCoroutine(RotateTowardsExit());
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

        transform.rotation = targetRotation; // Snap to final rotation to exit door
        // bool to exit the door
        isMoving = true;
        isLeaving = true;
    }


    public string getName()
    {
        return this.name;
    }


}

