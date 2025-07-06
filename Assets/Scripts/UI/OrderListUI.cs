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

    // A simple theme for styling the UI from code
    private readonly struct UITheme
    {
        public readonly Color PanelBackground;
        public readonly Color RowBackground;
        public readonly Color RowBackgroundAlternate; // For zebra striping
        public readonly Color HeaderText;
        public readonly Color PrimaryText;
        public readonly Color SeparatorColor;

        public UITheme(float a)
        {
            PanelBackground = new Color(0.1f, 0.1f, 0.12f, 0.9f);
            RowBackground = new Color(0.15f, 0.15f, 0.17f, 0.9f);
            RowBackgroundAlternate = new Color(0.2f, 0.2f, 0.22f, 0.9f); // A slightly lighter grey
            HeaderText = new Color(0.9f, 0.9f, 0.9f, 1f);
            PrimaryText = new Color(0.8f, 0.8f, 0.8f, 1f);
            SeparatorColor = new Color(1f, 1f, 1f, 0.1f);
        }
    }
    private readonly UITheme theme = new UITheme(1);

    // Dictionary to track active order rows for performance
    private Dictionary<CustomerController, GameObject> activeOrderRows = new Dictionary<CustomerController, GameObject>();

    void Start()
    {
        ConfigureUILayout();
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

        if (isPanelActive)
        {
            UpdateOrderList();
        }
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

            VerticalLayoutGroup panelLayout = orderListPanel.GetComponent<VerticalLayoutGroup>();
            if (panelLayout == null) panelLayout = orderListPanel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(20, 20, 20, 20);
            panelLayout.spacing = 10;
            panelLayout.childAlignment = TextAnchor.UpperCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            ContentSizeFitter panelFitter = orderListPanel.GetComponent<ContentSizeFitter>();
            if (panelFitter == null) panelFitter = orderListPanel.AddComponent<ContentSizeFitter>();
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Configure the title
            Transform titleTransform = orderListPanel.transform.Find("Title");
            if (titleTransform != null)
            {
                TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.fontSize = 32;
                    titleText.fontStyle = FontStyles.Bold;
                    titleText.color = theme.HeaderText;

                    LayoutElement titleLayout = titleText.GetComponent<LayoutElement>();
                    if (titleLayout == null) titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
                    titleLayout.minHeight = 40;
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

        // Configure the container for order rows
        if (orderListContainer != null)
        {
            VerticalLayoutGroup containerLayout = orderListContainer.GetComponent<VerticalLayoutGroup>();
            if (containerLayout == null) containerLayout = orderListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = 5;
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
        headerLayout.padding = new RectOffset(10, 10, 5, 10);
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
    /// Efficiently updates the list of orders, only changing what is necessary.
    /// </summary>
    private void UpdateOrderList()
    {
        if (CustomerManager.Instance == null) return;

        HashSet<CustomerController> toRemove = new HashSet<CustomerController>(activeOrderRows.Keys);
        List<CustomerController> activeCustomers = CustomerManager.Instance.GetActiveCustomers();

        foreach (var customer in activeCustomers)
        {
            // Ignore customers who have finished their order
            if (customer.IsOrderCompleted)
            {
                continue;
            }

            toRemove.Remove(customer);

            if (activeOrderRows.ContainsKey(customer))
            {
                // This customer is already in the list, just update their time left
                GameObject orderRow = activeOrderRows[customer];
                // Ensure the row and its components are not null before updating
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
                // This is a new customer, create a new row for them
                GameObject orderRow = Instantiate(orderRowPrefab, orderListContainer);
                
                // Determine the color based on the current number of rows (for zebra striping)
                bool isEvenRow = (orderListContainer.childCount - 1) % 2 == 0;

                // Configure the layout for the new row
                ConfigureOrderRow(orderRow, isEvenRow);

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

        // Remove rows for customers who are no longer active
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
    private void ConfigureOrderRow(GameObject orderRow, bool isEven)
    {
        // Add a background image for a card-like effect with alternating colors
        Image rowImage = orderRow.GetComponent<Image>();
        if (rowImage == null) rowImage = orderRow.AddComponent<Image>();
        rowImage.color = isEven ? theme.RowBackground : theme.RowBackgroundAlternate;
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
