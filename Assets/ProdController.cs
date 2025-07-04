using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdController : MonoBehaviour
{
    void Awake()
    {
        #if PRODUCTION
            this.gameObject.SetActive(false);
        #endif
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}