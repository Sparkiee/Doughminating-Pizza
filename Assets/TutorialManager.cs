using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    [SerializeField] private TutorialMessage tutorialMessage;
    [SerializeField] private GameObject tutorialArrow;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Vector3 movementTutorialStartPosition;

    private GameObject customer;
    private Pizza tutorialPizza;
    [SerializeField] private GameObject doughFreezer;
    [SerializeField] private GameObject WorkCounter;
    [SerializeField] private GameObject RollingPin;
    [SerializeField] private GameObject tomatoBox;
    [SerializeField] private GameObject cheeseBox;
    [SerializeField] private GameObject pineappleBox;
    [SerializeField] private GameObject ovenPosition;
    [SerializeField] private GameObject oven;
    [SerializeField] private GameObject blender;
    [SerializeField] private GameObject blenderOutput;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void RunTutorial()
    {
        
        Time.timeScale = 1f;
        SC_Player player = GameObject.FindWithTag("Player").GetComponent<SC_Player>();
        if (player == null) return;
        player.ToggleTutorial(true);
        tutorialPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialMessage.ShowMessage("Welcome to Joe's Pizza!\nYour first day can be quite scary, but we will teach you the basics!", () =>
        {
            StartCoroutine(WaitAndContinue());
            IEnumerator WaitAndContinue()
            {
                yield return new WaitForSeconds(1f);
                tutorialMessage.ClearMessageBackwards(() =>
                {
                    tutorialMessage.ShowMessage("First, let's start with the basics.\nYou can move around using WASD and the mouse.", () =>
                    {
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
            yield return null;
        }
        tutorialMessage.ClearMessageBackwards(() =>
        {
            tutorialMessage.ShowMessage("Great! Now let's learn how to interact with customers.", () =>
            {
                this.customer = GameManager.Instance.SpawnTutorialCustomer();
                StartCoroutine(WaitAndContinue());
                IEnumerator WaitAndContinue()
                {
                    yield return new WaitForSeconds(1f);
                    tutorialMessage.ClearMessageBackwards(() =>
                    {
                        tutorialArrow.SetActive(true);
                        tutorialArrow.transform.position = this.customer.transform.position + new Vector3(0, 2, 0);
                        tutorialArrow.transform.SetParent(this.customer.transform, true);
                        tutorialMessage.ShowMessage("Look someone is approaching!", () =>
                        {
                            StartCoroutine(WaitForCustomerArrival());
                        });
                    });
                }
                IEnumerator WaitForCustomerArrival()
                {
                    yield return new WaitForSeconds(1f);
                    tutorialMessage.ClearMessageBackwards(() =>
                    {
                        tutorialMessage.ShowMessage("This is " + this.customer.name + "! Let's make him a pizza!", () =>
                        {
                            StartCoroutine(WaitAndContinue2());
                        });
                    });
                }
                IEnumerator WaitAndContinue2()
                {
                    yield return new WaitForSeconds(1f);
                    tutorialMessage.ClearMessageBackwards(() =>
                    {
                        tutorialMessage.ShowMessage("First, we need to grab some dough.", () =>
                        {
                            tutorialArrow.SetActive(true);
                            tutorialArrow.transform.position = doughFreezer.transform.position + new Vector3(0, 2, 0);
                            tutorialArrow.transform.SetParent(doughFreezer.transform, true);
                            StartCoroutine(WaitForDough());
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
                    break;
                }
            }
            yield return null;
        }
        tutorialArrow.SetActive(false);
        tutorialMessage.ClearMessageBackwards(() =>
        {
            tutorialMessage.ShowMessage("Great! Now let's make a pizza with it.", () =>
            {
                StartCoroutine(WaitAndContinue3());
            });
        });

        IEnumerator WaitAndContinue3()
        {
            yield return new WaitForSeconds(1f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("First of all, place the dough on top of the counter", () =>
                {
                    tutorialArrow.SetActive(true);
                    tutorialArrow.transform.position = WorkCounter.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(WorkCounter.transform, true);
                    StartCoroutine(WaitForDoughPlaced());
                });
            });
        }

        IEnumerator WaitForDoughPlaced()
        {
            int initialChildCount = WorkCounter.transform.childCount;
            while (WorkCounter.transform.childCount <= initialChildCount)
            {
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Nice! You've placed the dough on the counter.\n Now grab the Rolling Pin!", null);
                tutorialArrow.SetActive(true);
                tutorialArrow.transform.position = RollingPin.transform.position + new Vector3(0, 1, 0);
                tutorialArrow.transform.SetParent(RollingPin.transform, true);
                StartCoroutine(WaitForRollingPin());
            });
        }

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
                        break;
                    }
                }
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now use the Rolling Pin to flatten the dough.", () =>
                {
                    StartCoroutine(WaitToFlatten());
                });
            });
        }

        IEnumerator WaitToFlatten()
        {
            while (true)
            {
                Pizza pizza = null;
                foreach (Transform child in WorkCounter.transform)
                {
                    if (child.TryGetComponent<Pizza>(out pizza))
                    {
                        this.tutorialPizza = pizza;
                        tutorialArrow.SetActive(false);
                        break;
                    }
                }
                if (pizza != null)
                {
                    this.tutorialPizza = pizza;
                    tutorialArrow.SetActive(false);
                    break;
                }
                yield return new WaitForSeconds(1f);
            }
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Once you're done, it's time to make some sauce!", () =>
                {
                    StartCoroutine(WaitAndContinue4());
                });
            });
        }

        IEnumerator WaitAndContinue4()
        {
            yield return new WaitForSeconds(1f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("You can find the tomatoes in the box next to the counter.", () =>
                {
                    tutorialArrow.SetActive(true);
                    tutorialArrow.transform.position = tomatoBox.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(tomatoBox.transform, true);
                    StartCoroutine(WaitForTomatoes());
                });
            });
        }

        IEnumerator WaitForTomatoes()
        {
            PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
            while (true)
            {
                GameObject held = playerHand.HeldItem;
                if (held != null)
                {
                    Ingredient ingredient = held.GetComponent<Ingredient>();
                    if (ingredient != null && ingredient is Tomato)
                    {
                        break;
                    }
                }
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now let's make some sauce with it.", () =>
                {
                    StartCoroutine(WaitAndContinue5());
                });
            });
        }

        IEnumerator WaitAndContinue5()
        {
            yield return new WaitForSeconds(1f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("You can use the blender to make sauce.", () =>
                {
                    tutorialArrow.SetActive(true);
                    tutorialArrow.transform.position = blender.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(blender.transform, true);
                    StartCoroutine(WaitForBlender());
                });
            });
        }

        IEnumerator WaitForBlender()
        {
            int currentBlenderOutput = blenderOutput.transform.childCount;
            while (currentBlenderOutput == 0)
            {
                yield return null;
                currentBlenderOutput = blenderOutput.transform.childCount;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now grab the sauce and put it on the pizza!", () =>
                {
                    tutorialArrow.transform.position = this.tutorialPizza.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(this.tutorialPizza.transform, true);
                    tutorialArrow.SetActive(true);
                    StartCoroutine(WaitForSauceOnPizza());
                });
            });
        }

        IEnumerator WaitForSauceOnPizza()
        {
            while (!this.tutorialPizza.HasSauce)
            {
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now let's add some cheese to the pizza.", () =>
                {
                    StartCoroutine(WaitAndContinue6());
                });
            });
        }

        IEnumerator WaitAndContinue6()
        {
            yield return new WaitForSeconds(1f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("You can find the cheese in the box next to the counter.", () =>
                {
                    tutorialArrow.SetActive(true);
                    tutorialArrow.transform.position = cheeseBox.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(cheeseBox.transform, true);
                    StartCoroutine(WaitForCheese());
                });
            });
        }

        IEnumerator WaitForCheese()
        {
            while (true)
            {
                PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
                GameObject held = playerHand.HeldItem;
                if (held != null)
                {
                    Ingredient ingredient = held.GetComponent<Ingredient>();
                    if (ingredient != null && ingredient is Cheese)
                    {
                        break;
                    }
                }
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great now you grabbed the cheese! Let's put it on the pizza!");
                tutorialArrow.transform.position = this.tutorialPizza.transform.position + new Vector3(0, 1, 0);
                tutorialArrow.transform.SetParent(this.tutorialPizza.transform, true);
                tutorialArrow.SetActive(true);
                StartCoroutine(WaitForCheeseOnPizza());
            });
        }

        IEnumerator WaitForCheeseOnPizza()
        {
            while (!this.tutorialPizza.HasCheese)
            {
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great now you've put some cheese on the pizza");
                StartCoroutine(WaitAndContinue7());
            });
        }

        IEnumerator WaitAndContinue7()
        {
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Now we are missing one last final ingredient!", () =>
                {
                    this.GetComponent<AudioSource>().Play();
                    StartCoroutine(WaitForPineapple());
                });
            });
            yield return null;
        }

        IEnumerator WaitForPineapple()
        {
            tutorialArrow.SetActive(true);
            tutorialArrow.transform.position = pineappleBox.transform.position + new Vector3(0, 1, 0);
            tutorialArrow.transform.SetParent(pineappleBox.transform, true);
            PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
            while (true)
            {
                GameObject held = playerHand.HeldItem;
                if (held != null)
                {
                    Ingredient ingredient = held.GetComponent<Ingredient>();
                    if (ingredient != null && ingredient is Pineapple)
                    {
                        break;
                    }
                }
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now let's put it on the pizza!", () =>
                {
                    tutorialArrow.transform.position = this.tutorialPizza.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(this.tutorialPizza.transform, true);
                    tutorialArrow.SetActive(true);
                    StartCoroutine(WaitForPineappleOnPizza());
                });
            });
        }

        IEnumerator WaitForPineappleOnPizza()
        {
            while (!this.tutorialPizza.HasPineapple)
            {
                yield return null;
            }
            tutorialArrow.SetActive(false);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now let's bake the pizza in the oven!", () =>
                {
                    StartCoroutine(WaitAndContinue8());
                });
            });
        }

        IEnumerator WaitAndContinue8()
        {
            yield return new WaitForSeconds(2f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("You can find the oven next to the counter. Make sure to keep an eye on the timer!", () =>
                {
                    StartCoroutine(WaitAndContinue9());
                });
            });
        }

        IEnumerator WaitAndContinue9()
        {
            yield return new WaitForSeconds(2f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("You can place the pizza in the oven by interacting with it.", () =>
                {
                    tutorialArrow.SetActive(true);
                    tutorialArrow.transform.position = ovenPosition.transform.position + new Vector3(0, 1, 0);
                    tutorialArrow.transform.SetParent(oven.transform, true);
                    StartCoroutine(WaitForOven());
                });
            });
        }

        IEnumerator WaitForOven()
        {
            Oven ovenComponent = oven.GetComponent<Oven>();
            while (!ovenComponent.isCooking)
            {
                yield return null;
            }
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("Great! Now wait for the pizza to be ready. Make sure to grab it in time!", () =>
                {
                    StartCoroutine(WaitForPizzaReady());
                });
            });
        }

        IEnumerator WaitForPizzaReady()
        {   
            PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
            while (true)
            {
                GameObject held = playerHand.HeldItem;
                if (held != null)
                {
                    Ingredient ingredient = held.GetComponent<Ingredient>();
                    if (ingredient != null && ingredient is Pizza)
                    {
                        Pizza pizza = (Pizza)ingredient;

                        switch (pizza.GetCookLevel())
                        {
                            case CookState.Cooked:
                                tutorialMessage.ClearMessageBackwards(() =>
                                {
                                    tutorialMessage.ShowMessage("Great! Now you can serve the pizza to the customer!", () =>
                                    {
                                        tutorialArrow.SetActive(true);
                                        tutorialArrow.transform.position = this.customer.transform.position + new Vector3(0, 2, 0);
                                        tutorialArrow.transform.SetParent(this.customer.transform, true);
                                    });
                                });
                                yield break; // 🔑 Exit coroutine

                            case CookState.Burnt:
                                tutorialMessage.ClearMessageBackwards(() =>
                                {
                                    tutorialMessage.ShowMessage("Oh no! The pizza is burnt! You need to make a new one.", () =>
                                    {
                                        StartCoroutine(WaitToGetRidOfPizza());
                                    });
                                });
                                yield break; // 🔑 Exit coroutine

                            case CookState.Raw:
                                tutorialMessage.ClearMessageBackwards(() =>
                                {
                                    tutorialMessage.ShowMessage("The pizza is still raw! You need to wait for it to cook.", () =>
                                    {
                                        StartCoroutine(WaitToGetRidOfPizza());
                                    });
                                });
                                yield break; // 🔑 Exit coroutine
                        }

                    }
                }
                yield return null;
            }
        }

        IEnumerator WaitToGetRidOfPizza()
        {
            tutorialArrow.SetActive(true);
            tutorialArrow.transform.position = ovenPosition.transform.position + new Vector3(0, 1, 0);
            tutorialArrow.transform.SetParent(oven.transform, true);
            yield return new WaitForSeconds(2f);
            tutorialMessage.ClearMessageBackwards(() =>
            {
                tutorialMessage.ShowMessage("You can throw it away if it's burnt, or put it back in the oven if it's raw.", () =>
                {
                    StartCoroutine(WaitForPlayerToGetRidOfPizza());
                });

                IEnumerator WaitForPlayerToGetRidOfPizza()
                {
                    PlayerHand playerHand = GameObject.FindWithTag("Player")?.GetComponent<PlayerHand>();
                    Oven ovenComponent = oven.GetComponent<Oven>();
                    while (playerHand != null && playerHand.IsHoldingItem && ovenComponent != null && !ovenComponent.isCooking)
                    {
                        yield return null;
                    }
                    StartCoroutine(WaitForPizzaReady());
                }
            });
        }

        // IEnumerator WaitAndContinue10()
        // {
        //     yield return new WaitForSeconds(2f);
        //     tutorialMessage.ClearMessageBackwards(() =>
        //     {
        //         tutorialMessage.ShowMessage("You can serve the pizza by interacting with the customer.", () =>
        //         {
        //             tutorialArrow.SetActive(true);
        //             tutorialArrow.transform.position = this.customer.transform.position + new Vector3(0, 2, 0);
        //             tutorialArrow.transform.SetParent(this.customer.transform, true);
        //             // StartCoroutine(WaitForCustomerInteraction());
        //         });
        //     });
        // }
    }

    public void EndTutorial() {
        tutorialMessage.ClearMessageBackwards(() =>
        {
            tutorialMessage.ShowMessage("Congratulations! You've completed the tutorial!\nNow you can start your pizza-making journey! Good luck!", () =>
            {
                StartCoroutine(WaitAndStartGame());
                IEnumerator WaitAndStartGame()
                {
                    yield return new WaitForSeconds(10f);
                }
                tutorialMessage.ClearMessageBackwards(() =>
                {
                    tutorialArrow.SetActive(false);
                    tutorialPanel.SetActive(false);
                });

                SC_Player player = GameObject.FindWithTag("Player").GetComponent<SC_Player>();
                if (player == null) return;
                player.ToggleTutorial(false);

                gameManager.StartGame();
            });
        });
    }
}
