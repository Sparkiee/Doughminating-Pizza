using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles
{
    /// <summary>
    /// Enhanced automatic door controller that opens for customers and players.
    /// Supports both trigger-based detection and distance-based detection for better reliability.
    /// Compatible with the original opencloseDoor functionality.
    /// </summary>
    public class opencloseDoor : MonoBehaviour
    {
        [Header("Door Animation")]
        public Animator openandclose;

        [Header("Door Settings")]
        [Tooltip("Delay before closing the door after all entities leave")]
        public float closeDelay = 2.0f;

        [Tooltip("Distance to detect approaching customers (0 = trigger only)")]
        public float detectionDistance = 4.0f;

        [Tooltip("Enable distance-based detection for customers")]
        public bool useDistanceDetection = true;

        [Header("Animation Names")]
        [Tooltip("Name of the opening animation")]
        public string openAnimationName = "Opening";

        [Tooltip("Name of the closing animation")]
        public string closeAnimationName = "Closing";

        [Header("Debug")]
        [Tooltip("Show debug messages")]
        public bool debugMode = false;

        // Only operate on Entry/Exit doors
        private bool isRelevantDoor = false;
        private Coroutine closeRoutine;
        private bool isDoorOpen = false;
        private List<GameObject> entitiesInRange = new List<GameObject>();
        
        void Start()
        {
            // Only operate on Entry/Exit doors
            isRelevantDoor = (gameObject.name == "EntryDoor" || gameObject.name == "ExitDoor" || gameObject.name == "ExtInt_FFK_Door_01_01");
            if (!isRelevantDoor)
            {
                enabled = false;
                return;
            }

            // Ensure we have an animator
            if (openandclose == null)
            {
                openandclose = GetComponent<Animator>();
                if (openandclose == null)
                {
                    openandclose = GetComponentInChildren<Animator>();
                }
            }

            if (openandclose == null)
            {
                Debug.LogError($"opencloseDoor on {gameObject.name}: No Animator found!");
                return;
            }

            // Set up trigger collider if needed
            EnsureTriggerCollider();

            if (debugMode)
            {
                Debug.Log($"opencloseDoor initialized on {gameObject.name}");
            }
        }
        
        void Update()
        {
            if (useDistanceDetection && detectionDistance > 0)
            {
                CheckNearbyEntities();
            }
        }
        
        /// <summary>
        /// Ensures the door has a trigger collider
        /// </summary>
        void EnsureTriggerCollider()
        {
            Collider[] colliders = GetComponents<Collider>();
            bool hasTrigger = false;
            
            foreach (Collider col in colliders)
            {
                if (col.isTrigger)
                {
                    hasTrigger = true;
                    break;
                }
            }
            
            if (!hasTrigger)
            {
                BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(detectionDistance * 2, 3f, detectionDistance * 2);
                
                if (debugMode)
                {
                    Debug.Log($"Added trigger collider to {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Check for nearby customers and players using distance detection
        /// </summary>
        void CheckNearbyEntities()
        {
            // Check for customers
            GameObject[] customers = GameObject.FindGameObjectsWithTag("Customer");
            foreach (GameObject customer in customers)
            {
                if (customer == null) continue;

                float distance = Vector3.Distance(transform.position, customer.transform.position);

                // Only open before customer enters (approaching)
                if (distance <= detectionDistance && !entitiesInRange.Contains(customer))
                {
                    Vector3 directionToDoor = (transform.position - customer.transform.position).normalized;
                    Vector3 customerDirection = customer.transform.forward;
                    float dot = Vector3.Dot(customerDirection, directionToDoor);
                    if (dot > 0.2f) // Approaching
                    {
                        entitiesInRange.Add(customer);
                        OpenDoor($"Customer {customer.name}");
                    }
                }
                // Close only after customer is inside and moving away
                else if (distance > detectionDistance * 0.7f && entitiesInRange.Contains(customer))
                {
                    Vector3 directionToDoor = (transform.position - customer.transform.position).normalized;
                    Vector3 customerDirection = customer.transform.forward;
                    float dot = Vector3.Dot(customerDirection, directionToDoor);
                    if (dot < -0.2f) // Moving away from door
                    {
                        entitiesInRange.Remove(customer);
                        CheckIfShouldClose();
                    }
                }
            }

            // Check for player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance <= detectionDistance && !entitiesInRange.Contains(player))
                {
                    entitiesInRange.Add(player);
                    OpenDoor("Player");
                }
                else if (distance > detectionDistance * 0.7f && entitiesInRange.Contains(player))
                {
                    entitiesInRange.Remove(player);
                    CheckIfShouldClose();
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Only operate for relevant doors
            if (!isRelevantDoor) return;
            if (other.CompareTag("Customer") || other.CompareTag("Player"))
            {
                if (!entitiesInRange.Contains(other.gameObject))
                {
                    entitiesInRange.Add(other.gameObject);
                }
                OpenDoor(other.name);
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Only operate for relevant doors
            if (!isRelevantDoor) return;
            if (other.CompareTag("Customer") || other.CompareTag("Player"))
            {
                if (entitiesInRange.Contains(other.gameObject))
                {
                    entitiesInRange.Remove(other.gameObject);
                }
                CheckIfShouldClose();
            }
        }
        
        /// <summary>
        /// Opens the door
        /// </summary>
        void OpenDoor(string entityName = "Entity")
        {
            if (openandclose == null) return;

            // Cancel any pending close operation
            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
                closeRoutine = null;
            }

            if (!isDoorOpen)
            {
                Debug.Log($"[Door] Opening {gameObject.name} for {entityName}");
                openandclose.Play(openAnimationName);
                isDoorOpen = true;
            }
        }
        
        /// <summary>
        /// Checks if the door should close based on entities in range
        /// </summary>
        void CheckIfShouldClose()
        {
            // Clean up null references
            entitiesInRange.RemoveAll(entity => entity == null);
            
            if (entitiesInRange.Count == 0)
            {
                // No entities in range, start close timer
                if (closeRoutine != null)
                {
                    StopCoroutine(closeRoutine);
                }
                closeRoutine = StartCoroutine(CloseAfterDelay());
            }
        }

        IEnumerator CloseAfterDelay()
        {
            Debug.Log($"[Door] {gameObject.name} will close in {closeDelay} seconds");
            yield return new WaitForSeconds(closeDelay);

            // Double-check that no entities are still in range
            entitiesInRange.RemoveAll(entity => entity == null);

            if (entitiesInRange.Count == 0 && openandclose != null)
            {
                Debug.Log($"[Door] Closing {gameObject.name}");
                openandclose.Play(closeAnimationName);
                isDoorOpen = false;
            }

            closeRoutine = null;
        }
        
        /// <summary>
        /// Public methods for external control
        /// </summary>
        public void ForceOpen()
        {
            OpenDoor("External Script");
        }
        
        public void ForceClose()
        {
            entitiesInRange.Clear();
            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
                closeRoutine = null;
            }
            
            if (openandclose != null)
            {
                openandclose.Play(closeAnimationName);
                isDoorOpen = false;
            }
        }
        
        public bool IsDoorOpen()
        {
            return isDoorOpen;
        }
        
        /// <summary>
        /// Debug visualization
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (useDistanceDetection && detectionDistance > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, detectionDistance);
            }
            
            Gizmos.color = isDoorOpen ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
        }
    }
}