using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choppable : MonoBehaviour, IChopped
{
    [SerializeField] private bool isChopped = false;

    public bool IsChopped() => isChopped;

    public void Chop()
    {
        if (!isChopped)
        {
            isChopped = true;
            Debug.Log($"{gameObject.name} has been chopped!");
        }
    }
}
