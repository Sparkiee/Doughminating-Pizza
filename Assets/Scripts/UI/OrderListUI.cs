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
    
    [Header("Bell Notification")]
    [Tooltip("Reference to the BellNotificationUI component that handles the bell notification")]
    public BellNotificationUI bellNotificationUI;

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
    private Dictionary<Customer, GameObject> activeOrderRows = new Dictionary<Customer, GameObject>();

    void Start()
    {
        ConfigureUILayout();
        
        // Find or create the bell notification UI
        if (bellNotificationUI == null)
        {
            bellNotificationUI = FindObjectOfType<BellNotificationUI>();
            if (bellNotificationUI == null)
            {
                // Create a new GameObject with the BellNotificationUI component
                GameObject bellNotificationObj = new GameObject("BellNotificationUI");
                bellNotificationUI = bellNotificationObj.AddComponent<BellNotificationUI>();
                Debug.Log("Created new BellNotificationUI component");
            }
            else
            {
                Debug.Log("Found existing BellNotificationUI component");
            }
        }
        
        // Ensure the bell notification is initialized
        if (bellNotificationUI != null)
        {
            bellNotificationUI.InitializeBellNotification();
        }
        
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
    }

    public void ToggleOrderList()
    {
        // Prevent opening when game is paused
        SC_Player player = FindObjectOfType<SC_Player>();
        if (player != null && player.pauseMenuUI != null && player.pauseMenuUI.activeSelf)
        {
            // If pause menu is active, don't open order list
            return;
        }

        // Prevent opening when the tutorial panel is showing ("Would you like to play the tutorial?")
        // Try to get TutorialManager.Instance, fallback to GameManager.Instance.TutorialPanel
        GameObject tutorialPanel = null;
        if (TutorialManager.Instance != null)
        {
            tutorialPanel = TutorialManager.Instance.GetType().GetField("tutorialPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?.GetValue(TutorialManager.Instance) as GameObject;
        }
        if (tutorialPanel == null && GameManager.Instance != null)
        {
            var gmType = GameManager.Instance.GetType();
            var field = gmType.GetField("TutorialPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
                tutorialPanel = field.GetValue(GameManager.Instance) as GameObject;
        }
        if (tutorialPanel != null && tutorialPanel.activeSelf)
        {
            // If tutorial panel is active, don't open order list
            return;
        }

        isPanelActive = !isPanelActive;
        if (orderListPanel != null)
        {
            orderListPanel.SetActive(isPanelActive);
        }
    }

    /// <summary>
    /// Programmatically configures the UI layout, removing the need for manual setup in the editor.

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

    /// Creates a header row for the order list titles.
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

    /// Efficiently updates the list of orders, only adding new customers and removing completed ones.
    private void UpdateOrderList()
    {
        if (GameManager.Instance == null) return;

        List<GameObject> activeCustomers = new List<GameObject>();
        GameManager.Instance.GetActiveCustomers(activeCustomers);
        HashSet<Customer> currentCustomers = new HashSet<Customer>();

        // First pass: Add new customers and update existing ones
        foreach (var customerObj in activeCustomers)
        {
            Customer customer = customerObj.GetComponent<Customer>();
            if (customer == null) continue;

            // Ignore customers who have finished their order
            if (customer.isServed)
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
                        timeLeftText.text = customer.GetPatience().ToString("F1");
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

                if (customerNameText != null) customerNameText.text = customer.getName();
                if (orderText != null) orderText.text = customer.GetComponent<Order>()?.getOrderText() ?? "No order available.";
                if (timeLeftText != null) timeLeftText.text = customer.GetPatience().ToString("F1");

                activeOrderRows.Add(customer, orderRow);
        // Second pass: Remove customers who are no longer active
        List<Customer> toRemove = new List<Customer>();
        foreach (var customerKey in activeOrderRows.Keys)
        {
            if (!currentCustomers.Contains(customerKey))
            {
                toRemove.Add(customerKey);
            }
        }

        foreach (var customerToRemove in toRemove)
        {
            if (activeOrderRows.TryGetValue(customerToRemove, out GameObject row))
            {
                Destroy(row);
                activeOrderRows.Remove(customerToRemove);
            }
        }
            }
        }
    }

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
}
