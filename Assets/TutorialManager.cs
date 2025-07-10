using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialMessage tutorialMessage;
    [SerializeField] private GameObject tutorialArrow;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Vector3 movementTutorialStartPosition;

    private GameObject customer;
    [SerializeField] private GameObject doughFreezer;
    [SerializeField] private GameObject WorkCounter;
    [SerializeField] private GameObject RollingPin;


    public void RunTutorial() {
        tutorialPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialMessage.ShowMessage("Welcome to Joe's Pizza!\n Hope you are ready to learn the basics!", () =>{
            StartCoroutine(WaitAndContinue());
            IEnumerator WaitAndContinue()
            {
                yield return new WaitForSeconds(1f);
                tutorialMessage.ClearMessageBackwards(() => {
                    tutorialMessage.ShowMessage("First, let's start with the basics.\nYou can move around using WASD and the mouse.", () => {
                        StartCoroutine(WaitForMovement());
                    });
                });
            }

        });
    }

    private IEnumerator WaitForMovement()
    {
        this.movementTutorialStartPosition = Camera.main.transform.position;
        while (Vector3.Distance(Camera.main.transform.position, this.movementTutorialStartPosition) < 0.1f)
        {
            yield return null; // Wait for the next frame
        }
        tutorialMessage.ClearMessageBackwards(() => {
            tutorialMessage.ShowMessage("Great! Now let's learn how to interact with customers.", () => {
                this.customer = GameManager.Instance.SpawnTutorialCustomer();
                StartCoroutine(WaitAndContinue());
                IEnumerator WaitAndContinue()
                {
                    yield return new WaitForSeconds(1f);
                    tutorialMessage.ClearMessageBackwards(() => {
                        tutorialMessage.ShowMessage("Look someone is approaching!", () => {
                            tutorialArrow.SetActive(true);
                            tutorialArrow.transform.position = this.customer.transform.position + new Vector3(0, 2, 0);
                            tutorialArrow.transform.SetParent(this.customer.transform, true);
                            StartCoroutine(WaitForCustomerArrival());
                            IEnumerator WaitForCustomerArrival()
                            {
                                yield return new WaitForSeconds(2f);
                            }
                            tutorialMessage.ClearMessageBackwards(() => {
                                tutorialMessage.ShowMessage("This is " + this.customer.name + "! Let's make him a pizza!", () => {

                                    StartCoroutine(WaitAndContinue2());
                                    IEnumerator WaitAndContinue2() {
                                        yield return new WaitForSeconds(1f);
                                        tutorialMessage.ClearMessageBackwards(() => {
                                            tutorialMessage.ShowMessage("First, we need to grab some dough.", () => {
                                                // set arrow above the dough freezer
                                                tutorialArrow.SetActive(true);
                                                tutorialArrow.transform.position = doughFreezer.transform.position + new Vector3(0, 2, 0);
                                                tutorialArrow.transform.SetParent(doughFreezer.transform, true);
                                                StartCoroutine(WaitForDough());
                                            });
                                        });
                                    }
                                });
                            });
                        });
                    });
                }
            });
        });
    }

    private IEnumerator WaitForDough()
    {
        PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
        
        while (true)
        {
            GameObject held = playerHand.HeldItem;
            if (held != null)
            {
                Ingredient ingredient = held.GetComponent<Ingredient>();
                if (ingredient != null && ingredient is Dough)
                {
                    break; // Player is holding dough
                }
            }
            // Wait until the player has picked up the dough
            yield return null; // Wait for the next frame
        }
        tutorialArrow.SetActive(false); // Hide the arrow
        tutorialMessage.ClearMessageBackwards(() => {
            tutorialMessage.ShowMessage("Great! Now let's make a pizza with it.", () => {
                StartCoroutine(WaitAndContinue3());
                IEnumerator WaitAndContinue3()
                {
                    yield return new WaitForSeconds(1f);
                    tutorialMessage.ClearMessageBackwards(() => {
                        tutorialMessage.ShowMessage("First of all, place the dough on top of the counter", () => {
                            tutorialArrow.SetActive(true);
                            tutorialArrow.transform.position = WorkCounter.transform.position + new Vector3(0, 1, 0);
                            tutorialArrow.transform.SetParent(WorkCounter.transform, true);
                            StartCoroutine(WaitForDoughPlaced());
                            IEnumerator WaitForDoughPlaced()
                            {
                                int initialChildCount = WorkCounter.transform.childCount;
                                while (WorkCounter.transform.childCount <= initialChildCount)
                                {
                                    yield return null; // Wait for the next frame
                                }
                                tutorialArrow.SetActive(false);
                                tutorialMessage.ClearMessageBackwards(() => {
                                    tutorialMessage.ShowMessage("Nice! You've placed the dough on the counter.\n Now grab the Rolling Pin!", null);
                                    tutorialArrow.SetActive(true);
                                    tutorialArrow.transform.position = RollingPin.transform.position + new Vector3(0, 1, 0);
                                    tutorialArrow.transform.SetParent(RollingPin.transform, true);
                                    StartCoroutine(WaitForRollingPin());
                                    IEnumerator WaitForRollingPin()
                                    {
                                        while (true)
                                        {
                                            GameObject held = playerHand.HeldItem;
                                            if (held != null)
                                            {
                                                Tool item = held.TryGetComponent<Tool>(out Tool tool) ? tool : null;
                                                if (item != null && item is RollingPin)
                                                {
                                                    break; // Player is holding the Rolling Pin
                                                }
                                            }
                                            yield return null; // Wait for the next frame
                                        }
                                        tutorialArrow.SetActive(false);
                                        tutorialMessage.ClearMessageBackwards(() => {
                                            tutorialMessage.ShowMessage("Great! Now use the Rolling Pin to flatten the dough.", () => {
                                                StartCoroutine(WaitToFlatten());
                                                IEnumerator WaitToFlatten()
                                                {
                                                    while(true) {
                                                        this.WorkCounter.transform.Find("Pizza").TryGetComponent<Pizza>(out Pizza pizza);
                                                        if (pizza != null)
                                                        {
                                                            break; // Player has flattened the dough
                                                        }
                                                        yield return new WaitForSeconds(1f);
                                                    }
                                                    tutorialMessage.ClearMessageBackwards(() => {
                                                        tutorialMessage.ShowMessage("Once you're done, it's time to make some sauce!", () => {
                                                            // Set arrow above the work counter
                                                            tutorialArrow.SetActive(true);
                                                            tutorialArrow.transform.position = WorkCounter.transform.position + new Vector3(0, 1, 0);
                                                            tutorialArrow.transform.SetParent(WorkCounter.transform, true);
                                                        });
                                                    });
                                                }
                                            });
                                        });
                                    }
                                });
                            }
                        });
                    });
                }
            });
        });
    }
}
