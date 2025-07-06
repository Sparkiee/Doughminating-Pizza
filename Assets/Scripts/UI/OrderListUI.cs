using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class OrderListUI : MonoBehaviour
{
    public GameObject orderListPanel;
    public GameObject orderRowPrefab;
    public Transform orderListContainer;

    private bool isPanelActive = false;
    
    // Bell notification UI elements
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

    // A simple theme for styling the UI from code
    private readonly struct UITheme
    {
        public readonly Color PanelBackground;
        public readonly Color RowBackground;
        public readonly Color RowBackgroundAlternate; // For zebra striping
        public readonly Color HeaderText;
        public readonly Color PrimaryText;

        public UITheme(float a)
        {
            PanelBackground = new Color(0.1f, 0.1f, 0.12f, 0.9f);
            RowBackground = new Color(0.15f, 0.15f, 0.17f, 0.9f);
            RowBackgroundAlternate = new Color(0.2f, 0.2f, 0.22f, 0.9f); // A slightly lighter grey
            HeaderText = new Color(0.9f, 0.9f, 0.9f, 1f);
            PrimaryText = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }
    private readonly UITheme theme = new UITheme(1);

    // Dictionary to track active order rows for performance
    private Dictionary<CustomerController, GameObject> activeOrderRows = new Dictionary<CustomerController, GameObject>();

    void Start()
    {
        ConfigureUILayout();
        CreateBellNotification();
        if (orderListPanel != null)
        {
            orderListPanel.SetActive(isPanelActive);
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
        {
            ToggleOrderList();
        }

        // Close the order panel when Esc is pressed (game pause)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && isPanelActive)
        {
            isPanelActive = false;
            if (orderListPanel != null)
            {
                orderListPanel.SetActive(false);
            }
        }

        if (isPanelActive)
        {
            UpdateOrderList();
        }
        
        // Always update the bell notification count
        UpdateBellNotification();
    }

    public void ToggleOrderList()
    {
        isPanelActive = !isPanelActive;
        if (orderListPanel != null)
        {
            orderListPanel.SetActive(isPanelActive);
        }
    }

    /// <summary>
    /// Creates the bell notification icon on the right side of the screen.
    /// </summary>
    private void CreateBellNotification()
    {
        // Find the Canvas to attach the bell notification to
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

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

    /// <summary>
    /// Creates a simple circle sprite for the bell background and badge.
    /// </summary>
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

    /// <summary>
    /// Updates the bell notification with the current order count.
    /// </summary>
    private void UpdateBellNotification()
    {
        if (CustomerManager.Instance == null || bellNotification == null) return;

        List<CustomerController> activeCustomers = CustomerManager.Instance.GetActiveCustomers();
        int orderCount = 0;

        // Count active orders (non-completed customers)
        foreach (var customer in activeCustomers)
        {
            if (!customer.IsOrderCompleted)
            {
                orderCount++;
            }
        }

        // Show/hide bell based on order count
        bool hasOrders = orderCount > 0;
        bellNotification.SetActive(hasOrders);

        // Update the count text
        if (hasOrders && orderCountText != null)
        {
            orderCountText.text = orderCount.ToString();
        }
    }

    /// <summary>
    /// Programmatically configures the UI layout, removing the need for manual setup in the editor.
    /// </summary>
    private void ConfigureUILayout()
    {
        // Configure the main panel
        if (orderListPanel != null)
        {
            // Set background color
            Image panelImage = orderListPanel.GetComponent<Image>();
            if (panelImage == null) panelImage = orderListPanel.AddComponent<Image>();
            panelImage.color = theme.PanelBackground;

            // Configure the panel's RectTransform for better sizing
            RectTransform panelRect = orderListPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Set a wider fixed size to accommodate longer order descriptions
                panelRect.sizeDelta = new Vector2(800, 500); // Wider: 800x500
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
            }

            VerticalLayoutGroup panelLayout = orderListPanel.GetComponent<VerticalLayoutGroup>();
            if (panelLayout == null) panelLayout = orderListPanel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(25, 25, 25, 25);
            panelLayout.spacing = 12;
            panelLayout.childAlignment = TextAnchor.UpperCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            // Remove ContentSizeFitter from the main panel to prevent conflicts
            ContentSizeFitter panelFitter = orderListPanel.GetComponent<ContentSizeFitter>();
            if (panelFitter != null)
            {
                DestroyImmediate(panelFitter);
            }

            // Configure the title
            Transform titleTransform = orderListPanel.transform.Find("Title");
            if (titleTransform != null)
            {
                TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.fontSize = 36;
                    titleText.fontStyle = FontStyles.Bold;
                    titleText.color = theme.HeaderText;

                    LayoutElement titleLayout = titleText.GetComponent<LayoutElement>();
                    if (titleLayout == null) titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
                    titleLayout.minHeight = 45;
                }
            }

            // Create Header Row if it doesn't exist
            CreateHeaderRow();

            // Ensure the container for the dynamic rows is the last element
            // so it appears below the title and header.
            if (orderListContainer != null)
            {
                orderListContainer.SetAsLastSibling();
            }
        }

        // Configure the container for order rows with ScrollRect for long lists
        if (orderListContainer != null)
        {
            VerticalLayoutGroup containerLayout = orderListContainer.GetComponent<VerticalLayoutGroup>();
            if (containerLayout == null) containerLayout = orderListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = 8;
            containerLayout.padding = new RectOffset(0, 0, 0, 10);

            // Add ContentSizeFitter only to the container, not the main panel
            ContentSizeFitter containerFitter = orderListContainer.GetComponent<ContentSizeFitter>();
            if (containerFitter == null) containerFitter = orderListContainer.gameObject.AddComponent<ContentSizeFitter>();
            containerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Configure the container's RectTransform
            RectTransform containerRect = orderListContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                // Let the container expand naturally within the fixed panel size
                containerRect.anchorMin = new Vector2(0, 0);
                containerRect.anchorMax = new Vector2(1, 1);
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// Creates a header row for the order list titles.
    /// </summary>
    private void CreateHeaderRow()
    {
        if (orderListPanel == null || orderListPanel.transform.Find("HeaderRow") != null) return;

        GameObject headerRowObj = new GameObject("HeaderRow", typeof(RectTransform));
        headerRowObj.transform.SetParent(orderListPanel.transform, false);

        HorizontalLayoutGroup headerLayout = headerRowObj.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding = new RectOffset(15, 15, 8, 8);
        headerLayout.spacing = 20;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;

        // Create the text labels for the header
        CreateHeaderLabel(headerRowObj, "NameLabel", "Name", 0);
        CreateHeaderLabel(headerRowObj, "OrderLabel", "Order", 1); // This one is flexible
        CreateHeaderLabel(headerRowObj, "TimeLabel", "Time Left", 0);

    }

    /// <summary>
    /// Helper method to create a text label for the header.
    /// </summary>
    private void CreateHeaderLabel(GameObject parent, string gameObjectName, string text, float flexibleWidth)
    {
        GameObject labelObj = new GameObject(gameObjectName, typeof(RectTransform));
        labelObj.transform.SetParent(parent.transform, false);

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = text;
        labelText.fontSize = 22;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = theme.HeaderText;

        LayoutElement layoutElement = labelObj.AddComponent<LayoutElement>();
        if (flexibleWidth > 0)
        {
            layoutElement.flexibleWidth = flexibleWidth;
        }
    }

    /// <summary>
    /// Efficiently updates the list of orders, only adding new customers and removing completed ones.
    /// </summary>
    private void UpdateOrderList()
    {
        if (CustomerManager.Instance == null) return;

        List<CustomerController> activeCustomers = CustomerManager.Instance.GetActiveCustomers();
        HashSet<CustomerController> currentCustomers = new HashSet<CustomerController>();

        // First pass: Add new customers and update existing ones
        foreach (var customer in activeCustomers)
        {
            // Ignore customers who have finished their order
            if (customer.IsOrderCompleted)
            {
                continue;
            }

            currentCustomers.Add(customer);

            if (activeOrderRows.ContainsKey(customer))
            {
                // This customer is already in the list, just update their time left
                GameObject orderRow = activeOrderRows[customer];
                if (orderRow != null)
                {
                    TextMeshProUGUI timeLeftText = orderRow.transform.Find("TimeLeft")?.GetComponent<TextMeshProUGUI>();
                    if (timeLeftText != null)
                    {
                        timeLeftText.text = customer.GetPatienceTimeLeft().ToString("F1");
                    }
                }
            }
            else
            {
                // This is a new customer, create a new row for them at the bottom
                GameObject orderRow = Instantiate(orderRowPrefab, orderListContainer);
                
                // Configure the layout for the new row
                ConfigureOrderRow(orderRow);

                // Populate the text fields
                TextMeshProUGUI customerNameText = orderRow.transform.Find("CustomerName").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI orderText = orderRow.transform.Find("Order").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI timeLeftText = orderRow.transform.Find("TimeLeft").GetComponent<TextMeshProUGUI>();

                if (customerNameText != null) customerNameText.text = customer.customerName;
                if (orderText != null) orderText.text = customer.GetWantedIngredientsString();
                if (timeLeftText != null) timeLeftText.text = customer.GetPatienceTimeLeft().ToString("F1");

                activeOrderRows.Add(customer, orderRow);
            }
        }

        // Second pass: Remove customers who are no longer active
        List<CustomerController> toRemove = new List<CustomerController>();
        foreach (var customer in activeOrderRows.Keys)
        {
            if (!currentCustomers.Contains(customer))
            {
                toRemove.Add(customer);
            }
        }

        foreach (var customer in toRemove)
        {
            if (activeOrderRows.TryGetValue(customer, out GameObject row))
            {
                Destroy(row);
                activeOrderRows.Remove(customer);
            }
        }
    }

    /// <summary>
    /// Configures the layout components for a newly instantiated order row.
    /// </summary>
    private void ConfigureOrderRow(GameObject orderRow)
    {
        // Add a background image for a card-like effect
        Image rowImage = orderRow.GetComponent<Image>();
        if (rowImage == null) rowImage = orderRow.AddComponent<Image>();
        rowImage.color = theme.RowBackground;
        // If you have a rounded sprite, you can assign it here for better visuals
        // rowImage.sprite = ...;
        // rowImage.type = Image.Type.Sliced;

        HorizontalLayoutGroup layoutGroup = orderRow.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null) layoutGroup = orderRow.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(15, 15, 15, 15);
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // Make the 'Order' column flexible
        Transform orderTransform = orderRow.transform.Find("Order");
        if (orderTransform != null)
        {
            LayoutElement layoutElement = orderTransform.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = orderTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;
        }

        // Set font sizes and colors
        foreach (var text in orderRow.GetComponentsInChildren<TextMeshProUGUI>())
        {   
            text.fontSize = 20;
            text.color = theme.PrimaryText;
        }
    }

    /// <summary>
    /// Attempts to create a bell icon from available sources.
    /// </summary>
    /// <param name="bellIconGameObject">The GameObject to add the icon to</param>
    /// <returns>True if an icon was successfully created, false if fallback is needed</returns>
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
}
