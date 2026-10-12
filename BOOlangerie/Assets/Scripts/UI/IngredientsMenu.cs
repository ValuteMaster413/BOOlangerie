using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BOO.Data;
using BOO.Core;

public class IngredientsMenuUI : MonoBehaviour
{
    [Header("All Ingredients Database")]
    [SerializeField] private List<IngredientData> allIngredients = new List<IngredientData>();

    [Header("Tab Buttons")]
    [SerializeField] private Button baseTabButton;
    [SerializeField] private Button fillingTabButton;
    [SerializeField] private Button toppingTabButton;

    [Header("Ingredient Slots")]
    [SerializeField] private List<IngredientSlotUI> ingredientSlots;

    [Header("Action Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button serveButton;

    [Header("Visualizer")]
    [SerializeField] private DishVisualizer dishVisualizer;

    [Header("Description Panel")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Open / Closed List")]
    [SerializeField] private GameObject expandedPanel;
    [SerializeField] private GameObject collapsedPanel;
    [SerializeField] private Button collapseButton;
    [SerializeField] private Button expandButton;
    [SerializeField] private Image[] pickedIcons;

    [Header("Looks")]
    [SerializeField] private Color barColor = Color.white;
    [SerializeField] private Color selectedBarColor = new Color(0.69f, 0.69f, 0.69f);
    [SerializeField] private Color nameColor = Color.black;
    [SerializeField] private Color selectedNameColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.78f, 0.73f, 0.67f);

    private IngredientKind currentCategory = IngredientKind.Base;
    private CurrentRecipe currentRecipe = new CurrentRecipe();
    private Sprite[] emptyPickSprites;
    private bool tabsFrozen;

    private void Awake()
    {
        if (baseTabButton != null) baseTabButton.onClick.AddListener(() => OnTabClicked(IngredientKind.Base));
        if (fillingTabButton != null) fillingTabButton.onClick.AddListener(() => OnTabClicked(IngredientKind.Filling));
        if (toppingTabButton != null) toppingTabButton.onClick.AddListener(() => OnTabClicked(IngredientKind.Topping));

        if (resetButton != null) resetButton.onClick.AddListener(ResetRecipe);
        if (serveButton != null) serveButton.onClick.AddListener(ServeDish);

        if (collapseButton != null) collapseButton.onClick.AddListener(() => OnListToggled(true));
        if (expandButton != null) expandButton.onClick.AddListener(() => OnListToggled(false));

        int pickCount = pickedIcons != null ? pickedIcons.Length : 0;
        emptyPickSprites = new Sprite[pickCount];
        for (int i = 0; i < pickCount; i++)
        {
            if (pickedIcons[i] != null) emptyPickSprites[i] = pickedIcons[i].sprite;
        }
    }

    private void OnEnable()
    {
        SetCollapsed(false);
        SwitchCategory(IngredientKind.Base);
        RefreshDish();
    }

    private void Start()
    {
        FreezeTabLayout();
        RefreshTabs();
    }

    private void OnDestroy()
    {
        if (baseTabButton != null) baseTabButton.onClick.RemoveAllListeners();
        if (fillingTabButton != null) fillingTabButton.onClick.RemoveAllListeners();
        if (toppingTabButton != null) toppingTabButton.onClick.RemoveAllListeners();

        if (resetButton != null) resetButton.onClick.RemoveListener(ResetRecipe);
        if (serveButton != null) serveButton.onClick.RemoveListener(ServeDish);

        if (collapseButton != null) collapseButton.onClick.RemoveAllListeners();
        if (expandButton != null) expandButton.onClick.RemoveAllListeners();
    }

    private void OnTabClicked(IngredientKind category)
    {
        PlayListSfx();
        SwitchCategory(category);
    }

    private void OnListToggled(bool collapsed)
    {
        PlayListSfx();
        SetCollapsed(collapsed);
    }

    private void PlayListSfx()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ingredient_list");
    }

    private void SetCollapsed(bool collapsed)
    {
        if (expandedPanel != null) expandedPanel.SetActive(!collapsed);
        if (collapsedPanel != null) collapsedPanel.SetActive(collapsed);
    }

    public void SwitchCategory(IngredientKind category)
    {
        currentCategory = category;
        UpdateVisibleIngredients();
        RefreshTabs();
    }

    private void UpdateVisibleIngredients()
    {
        List<IngredientData> filteredIngredients = allIngredients.FindAll(ing => ing != null && ing.kind == currentCategory);

        for (int i = 0; i < ingredientSlots.Count; i++)
        {
            if (i < filteredIngredients.Count)
            {
                ingredientSlots[i].gameObject.SetActive(true);
                ingredientSlots[i].Setup(filteredIngredients[i], OnIngredientSelected);
            }
            else
            {
                ingredientSlots[i].gameObject.SetActive(false);
            }
        }

        RefreshSlots();
        ShowDescription(SelectedIn(currentCategory));
    }

    private void OnIngredientSelected(IngredientData ingredient)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRandomSFX("ingr_placed");

        switch (ingredient.kind)
        {
            case IngredientKind.Base:
                currentRecipe.selectedBase = ingredient;
                Debug.Log($"Base selected: {ingredient.displayName}");
                break;
            case IngredientKind.Filling:
                currentRecipe.selectedFilling = ingredient;
                Debug.Log($"Filling selected: {ingredient.displayName}");
                break;
            case IngredientKind.Topping:
                currentRecipe.selectedTopping = ingredient;
                Debug.Log($"Topping selected: {ingredient.displayName}");
                break;
        }

        RefreshSlots();
        ShowDescription(ingredient);
        RefreshDish();
    }

    private IngredientData SelectedIn(IngredientKind category)
    {
        switch (category)
        {
            case IngredientKind.Base: return currentRecipe.selectedBase;
            case IngredientKind.Filling: return currentRecipe.selectedFilling;
            default: return currentRecipe.selectedTopping;
        }
    }

    private Button TabFor(IngredientKind category)
    {
        switch (category)
        {
            case IngredientKind.Base: return baseTabButton;
            case IngredientKind.Filling: return fillingTabButton;
            default: return toppingTabButton;
        }
    }

    private void RefreshSlots()
    {
        IngredientData selected = SelectedIn(currentCategory);

        foreach (IngredientSlotUI slot in ingredientSlots)
        {
            bool isSelected = selected != null && slot.Ingredient == selected;
            slot.SetLook(isSelected ? selectedBarColor : barColor, isSelected ? selectedNameColor : nameColor);
        }
    }

    private void RefreshTabs()
    {
        SetTabColor(baseTabButton, IngredientKind.Base);
        SetTabColor(fillingTabButton, IngredientKind.Filling);
        SetTabColor(toppingTabButton, IngredientKind.Topping);

        if (!tabsFrozen) return;

        BringToFront(toppingTabButton);
        BringToFront(fillingTabButton);
        BringToFront(baseTabButton);
        BringToFront(TabFor(currentCategory));
    }

    private void SetTabColor(Button tab, IngredientKind category)
    {
        if (tab == null || tab.image == null) return;

        tab.image.color = category == currentCategory ? Color.white : inactiveTabColor;
    }

    private void BringToFront(Button tab)
    {
        if (tab != null) tab.transform.SetAsLastSibling();
    }

    private void FreezeTabLayout()
    {
        tabsFrozen = true;
        if (baseTabButton == null) return;

        LayoutGroup layout = baseTabButton.transform.parent.GetComponent<LayoutGroup>();
        if (layout == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);

        Button[] tabs = { baseTabButton, fillingTabButton, toppingTabButton };
        Vector3[] positions = new Vector3[tabs.Length];
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] != null) positions[i] = tabs[i].transform.position;
        }

        layout.enabled = false;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] != null) tabs[i].transform.position = positions[i];
        }
    }

    private void RefreshDish()
    {
        if (dishVisualizer != null) dishVisualizer.DisplayDish(currentRecipe);
        if (serveButton != null) serveButton.interactable = currentRecipe.IsValid();

        SetPickedIcon(0, currentRecipe.selectedBase);
        SetPickedIcon(1, currentRecipe.selectedFilling);
        SetPickedIcon(2, currentRecipe.selectedTopping);
    }

    private void SetPickedIcon(int index, IngredientData ingredient)
    {
        if (pickedIcons == null || index >= pickedIcons.Length || pickedIcons[index] == null) return;

        Sprite icon = ingredient != null && ingredient.icon != null ? ingredient.icon : emptyPickSprites[index];

        pickedIcons[index].sprite = icon;
        pickedIcons[index].enabled = icon != null;
        pickedIcons[index].preserveAspect = true;
    }

    private void ShowDescription(IngredientData ingredient)
    {
        if (descriptionText == null) return;

        descriptionText.text = ingredient != null
            ? ingredient.description
            : $"Pick a {currentCategory.ToString().ToLowerInvariant()}...";
    }

    private void ResetRecipe()
    {
        currentRecipe.Reset();
        Debug.Log("Recipe reset!");

        RefreshSlots();
        ShowDescription(null);
        RefreshDish();
    }

    private void ServeDish()
    {
        if (!currentRecipe.IsValid())
        {
            Debug.LogWarning("Cannot serve! You need at least a Base.");
            return;
        }

        Order order = new Order();
        order.baseItem = currentRecipe.selectedBase;
        order.filling = currentRecipe.selectedFilling;
        order.topping = currentRecipe.selectedTopping;

        Debug.Log($"Serving dish -> Base: {currentRecipe.selectedBase?.displayName}, " +
                  $"Filling: {currentRecipe.selectedFilling?.displayName ?? "None"}, " +
                  $"Topping: {currentRecipe.selectedTopping?.displayName ?? "None"}");


        currentRecipe.Reset();
        FindObjectOfType<BakeryViewController>()?.ShowCounterStation();

        GameManager.Instance.Serve(order);
    }
}
