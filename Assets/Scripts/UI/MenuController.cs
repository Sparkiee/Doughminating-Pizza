using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private float buttonAnimSpeed = 9;
    private bool tap = true;
    public AudioClip tapAudio;

    private RaycastHit hitInfo;
    private Ray ray;
    private Camera menuCamera;
    
    // public GameObject playerRecord; Personal best
    
    void Awake()
    {
        // Find the camera in the scene
        menuCamera = Camera.main;
        if (menuCamera == null)
        {
            menuCamera = FindObjectOfType<Camera>();
        }
        
        // Reset any lingering game state
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Destroy any lingering RestaurantGameManager instances
        RestaurantGameManager[] managers = FindObjectsOfType<RestaurantGameManager>();
        for (int i = 0; i < managers.Length; i++)
        {
            if (managers[i] != null)
            {
                Destroy(managers[i].gameObject);
            }
        }
        
        Debug.Log("MenuController initialized");
    }
    
    void Update()
    {
        if (tap)
        {
            StartCoroutine(tapManager());
        }
    }

    IEnumerator tapManager()
    {
        // Check for mouse input using new Input System
        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            yield break;
        }
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Make sure we have a camera
        if (menuCamera == null)
        {
            Debug.LogWarning("No camera found for menu raycast!");
            yield break;
        }

        ray = menuCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject objectHit = hitInfo.transform.gameObject;
            Debug.Log($"Hit object: {objectHit.name}");
            
            switch (objectHit.name)
            {
                case "ButtonPlay-01":
                    Debug.Log("Play button clicked!");
                    playAudio(tapAudio);
                    StartCoroutine(animateButton(objectHit));
                    // PlayerRecord() 
                    yield return new WaitForSeconds(1.0f);
                    LoadGameScene();
                    break;
            }
        }
    }
    
    private void LoadGameScene()
    {
        Debug.Log("Loading game scene...");
        SceneManager.LoadScene("FFK Sample Scene");
    }

    IEnumerator animateButton(GameObject _btn)
    {
        tap = false;
        Vector3 startingScale = _btn.transform.localScale;  
        Vector3 destinationScale = startingScale * 0.85f;                           

        //Scale up
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime * buttonAnimSpeed;
            _btn.transform.localScale = new Vector3(Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
                Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
                _btn.transform.localScale.z);
            yield return 0;
        }
        
        float r = 0.0f;
        if (_btn.transform.localScale.x >= destinationScale.x)
        {
            while (r <= 1.0f)
            {
                r += Time.deltaTime * buttonAnimSpeed;
                _btn.transform.localScale = new Vector3(Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
                    Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
                    _btn.transform.localScale.z);
                yield return 0;
            }
        }

        if (r >= 1)
            tap = true;
    }

    void playAudio(AudioClip _clip)
    {
        GetComponent<AudioSource>().clip = _clip;

        if (!GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
        }
    }
}
