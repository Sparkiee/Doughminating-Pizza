using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour, IInteractable
{
    private Transform targetSeat;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving && targetSeat != null)
        {
            // Move towards the target seat
            float step = moveSpeed * Time.deltaTime; // Calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetSeat.position, step);

            // Check if reached the target seat
            if (Vector3.Distance(transform.position, targetSeat.position) < 0.001f)
            {
                isMoving = false; // Stop moving when reached
            }
        }
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
        // Implement interaction logic here
        Debug.Log("Customer interacted with.");
    }

    public string getInteractionText()
    {
        return "Interact with Customer";
    }
}
