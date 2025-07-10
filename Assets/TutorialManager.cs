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
                    tutorialMessage.ShowMessage("First, let's start with the basics.\nYou can move around using WASD or Arrow keys and the mouse.", () => {
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
                            tutorialMessage.ClearMessageBackwards(() => {
                                tutorialMessage.ShowMessage("This is " + this.customer.name + "! Let's make him a pizza!", () => {

                                    StartCoroutine(WaitAndContinue2());
                                    IEnumerator WaitAndContinue2() {
                                        yield return new WaitForSeconds(1f);
                                        tutorialMessage.ClearMessageBackwards(() => {
                                            tutorialMessage.ShowMessage("First, we need to grab some dough.", () => {
                                                // set arrow above the dough freezer
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
        
        GameObject held = playerHand.HeldItem;
        Ingredient ingredient = held.GetComponent<Ingredient>();
        while (!(ingredient is Dough))
        {
            yield return null; // Wait for the next frame
        }

        tutorialMessage.ClearMessageBackwards(() => {
            tutorialMessage.ShowMessage("Great! Now let's make a pizza with it.", () => {
                StartCoroutine(WaitAndContinue3());
                IEnumerator WaitAndContinue3()
                {
                    yield return new WaitForSeconds(1f);
                    tutorialMessage.ClearMessageBackwards(() => {
                        tutorialMessage.ShowMessage("You can add ingredients to your pizza by clicking on them in the ingredient panel.", () => {
                            // GameManager.Instance.ShowIngredientPanel();
                        });
                    });
                }
            });
        });
    }
}
