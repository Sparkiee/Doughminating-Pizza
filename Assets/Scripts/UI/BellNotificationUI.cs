using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BellNotificationUI : MonoBehaviour
{
    private GameObject bellNotification;
    private TextMeshProUGUI orderCountText;
    
    [Header("Bell Notification Settings")]
    [Tooltip("PNG image for the bell icon. Can be either a Sprite or Texture2D.")]
    public Sprite bellIconSprite;
    [Tooltip("Alternative: Use Texture2D if you can't convert to Sprite.")]
    public Texture2D bellIconTexture;
    [Tooltip("Or specify filename in Resources folder (e.g., 'bell_icon' for bell_icon.png)")]
    public string bellIconResourceName = "";
    [Tooltip("Debug: Shows which method was used to load the icon")]
    public bool showDebugInfo = false;

    private readonly struct UITheme
    {
        public readonly Color HeaderText;

        public UITheme(float a)
        {
            HeaderText = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
    }
    private readonly UITheme theme = new UITheme(1);

    void Start()
    {
        // Add a small delay to ensure Canvas is ready
        Invoke(nameof(CreateBellNotification), 0.1f);
    }

    void Update()
    {
        UpdateBellNotification();
    }

    private void CreateBellNotification()
    {
        // Find the Canvas to attach the bell notification to
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) 
        {
            Debug.LogWarning("BellNotificationUI: No Canvas found in scene!");
            return;
        }

        Debug.Log("BellNotificationUI: Creating bell notification on canvas: " + canvas.name);

        // Create the main bell container
        bellNotification = new GameObject("BellNotification", typeof(RectTransform));
        bellNotification.transform.SetParent(canvas.transform, false);

        // Position it on the right side of the screen
        RectTransform bellRect = bellNotification.GetComponent<RectTransform>();
        bellRect.anchorMin = new Vector2(1f, 1f);
        bellRect.anchorMax = new Vector2(1f, 1f);
        bellRect.pivot = new Vector2(1f, 1f);
        bellRect.anchoredPosition = new Vector2(-20, -20); // 20px margin from top-right
        bellRect.sizeDelta = new Vector2(80, 80);

        // Add background circle
        Image bellBackground = bellNotification.AddComponent<Image>();
        bellBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark semi-transparent background
        bellBackground.sprite = CreateCircleSprite();

        // Create bell icon (PNG or fallback to emoji)
        GameObject bellIcon = new GameObject("BellIcon", typeof(RectTransform));
        bellIcon.transform.SetParent(bellNotification.transform, false);
        
        RectTransform iconRect = bellIcon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        // Try to create the bell icon using available methods
        bool iconCreated = CreateBellIcon(bellIcon);
        
        // Set the image scale to 0.7
        bellIcon.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        
        // If no icon was created, use emoji fallback
        if (!iconCreated)
        {
            TextMeshProUGUI bellIconText = bellIcon.AddComponent<TextMeshProUGUI>();
            bellIconText.text = "🔔"; // Bell emoji
            bellIconText.fontSize = 32;
            bellIconText.alignment = TextAlignmentOptions.Center;
            bellIconText.color = theme.HeaderText;
            if (showDebugInfo) Debug.Log("Bell icon using emoji fallback");
        }

        // Create order count badge
        GameObject countBadge = new GameObject("CountBadge", typeof(RectTransform));
        countBadge.transform.SetParent(bellNotification.transform, false);

        RectTransform badgeRect = countBadge.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.6f, 0.6f);
        badgeRect.anchorMax = new Vector2(1.2f, 1.2f);
        badgeRect.offsetMin = Vector2.zero;
        badgeRect.offsetMax = Vector2.zero;

        Image badgeBackground = countBadge.AddComponent<Image>();
        badgeBackground.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red background
        badgeBackground.sprite = CreateCircleSprite();

        // Add the count text
        GameObject countTextObj = new GameObject("CountText", typeof(RectTransform));
        countTextObj.transform.SetParent(countBadge.transform, false);

        RectTransform textRect = countTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        orderCountText = countTextObj.AddComponent<TextMeshProUGUI>();
        orderCountText.text = "0";
        orderCountText.fontSize = 18;
        orderCountText.fontStyle = FontStyles.Bold;
        orderCountText.alignment = TextAlignmentOptions.Center;
        orderCountText.color = Color.white;

        // Initially hide the bell if no orders
        bellNotification.SetActive(false);
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                colors[y * size + x] = distance <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void UpdateBellNotification()
    {
        if (GameManager.Instance == null || bellNotification == null) return;

        List<GameObject> activeCustomers = new List<GameObject>();
        GameManager.Instance.GetActiveCustomers(activeCustomers);
        int orderCount = 0;

        // Count active orders (non-completed customers)
        foreach (var customerObj in activeCustomers)
        {
            Customer customer = customerObj.GetComponent<Customer>();
            if (customer != null && customer.IsSeated && !customer.isServed)
            {
                orderCount++;
            }
        }

        // Always show the bell notification
        bellNotification.SetActive(true);

        // Update the count text
        if (orderCountText != null)
        {
            if (orderCount > 0)
            {
                orderCountText.text = orderCount.ToString();
                // Display count badge if there are orders:
                GameObject countBadge = orderCountText.transform.parent.gameObject;
                countBadge.SetActive(true);
                orderCountText.gameObject.SetActive(true);
            }
            else
            {
                orderCountText.text = "";
                // dont' display count badge if no orders:
                GameObject countBadge = orderCountText.transform.parent.gameObject;
                countBadge.SetActive(false);
                orderCountText.gameObject.SetActive(false);
            }

        }
    }

    private bool CreateBellIcon(GameObject bellIconGameObject)
    {
        if (bellIconSprite != null)
        {
            // Use the provided PNG sprite
            Image bellIconImage = bellIconGameObject.AddComponent<Image>();
            bellIconImage.sprite = bellIconSprite;
            bellIconImage.color = theme.HeaderText;
            bellIconImage.preserveAspect = true;
            if (showDebugInfo) Debug.Log("Bell icon loaded from Sprite field");
            return true;
        }
        
        if (bellIconTexture != null)
        {
            // Convert Texture2D to Sprite if no sprite is provided
            Image bellIconImage = bellIconGameObject.AddComponent<Image>();
            Sprite textureSprite = Sprite.Create(bellIconTexture, 
                new Rect(0, 0, bellIconTexture.width, bellIconTexture.height), 
                new Vector2(0.5f, 0.5f));
            bellIconImage.sprite = textureSprite;
            bellIconImage.color = theme.HeaderText;
            bellIconImage.preserveAspect = true;
            if (showDebugInfo) Debug.Log("Bell icon loaded from Texture2D field");
            return true;
        }
        
        if (!string.IsNullOrEmpty(bellIconResourceName))
        {
            // Load sprite from Resources folder
            Image bellIconImage = bellIconGameObject.AddComponent<Image>();
            Sprite resourceSprite = Resources.Load<Sprite>(bellIconResourceName);
            if (resourceSprite != null)
            {
                bellIconImage.sprite = resourceSprite;
                bellIconImage.color = theme.HeaderText;
                bellIconImage.preserveAspect = true;
                if (showDebugInfo) Debug.Log($"Bell icon loaded from Resources: {bellIconResourceName}");
                return true;
            }
            
            // Try loading as Texture2D instead
            Texture2D resourceTexture = Resources.Load<Texture2D>(bellIconResourceName);
            if (resourceTexture != null)
            {
                Sprite textureSprite = Sprite.Create(resourceTexture, 
                    new Rect(0, 0, resourceTexture.width, resourceTexture.height), 
                    new Vector2(0.5f, 0.5f));
                bellIconImage.sprite = textureSprite;
                bellIconImage.color = theme.HeaderText;
                bellIconImage.preserveAspect = true;
                if (showDebugInfo) Debug.Log($"Bell icon loaded from Resources as Texture2D: {bellIconResourceName}");
                return true;
            }
            
            // Remove the image component since we couldn't load anything
            DestroyImmediate(bellIconImage);
        }
        
        return false; // No icon could be created
    }

    public void InitializeBellNotification()
    {
        if (bellNotification == null)
        {
            CreateBellNotification();
        }
    }
}
