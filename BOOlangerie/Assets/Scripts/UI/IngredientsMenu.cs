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

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.953f, 0.894f, 0.78f);
    [SerializeField] private Color selectedColor = new Color(0.961f, 0.722f, 0.255f);

    private IngredientKind currentCategory = IngredientKind.Base;
    private CurrentRecipe currentRecipe = new CurrentRecipe();

    private void Awake()
    {
        if (baseTabButton != null) baseTabButton.onClick.AddListener(() => OnTabClicked(IngredientKind.Base));
        if (fillingTabButton != null) fillingTabButton.onClick.AddListener(() => OnTabClicked(IngredientKind.Filling));
        if (toppingTabButton != null) toppingTabButton.onClick.AddListener(() => OnTabClicked(IngredientKind.Topping));

        if (resetButton != null) resetButton.onClick.AddListener(ResetRecipe);
        if (serveButton != null) serveButton.onClick.AddListener(ServeDish);
    }

    private void OnEnable()
    {
        SwitchCategory(IngredientKind.Base);
        RefreshDish();
    }

    private void OnDestroy()
    {
        if (baseTabButton != null) baseTabButton.onClick.RemoveAllListeners();
        if (fillingTabButton != null) fillingTabButton.onClick.RemoveAllListeners();
        if (toppingTabButton != null) toppingTabButton.onClick.RemoveAllListeners();

        if (resetButton != null) resetButton.onClick.RemoveListener(ResetRecipe);
        if (serveButton != null) serveButton.onClick.RemoveListener(ServeDish);
    }

    private void OnTabClicked(IngredientKind category)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("ingredient_list");
        SwitchCategory(category);
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

    private void RefreshSlots()
    {
        IngredientData selected = SelectedIn(currentCategory);

        foreach (IngredientSlotUI slot in ingredientSlots)
        {
            bool isSelected = selected != null && slot.Ingredient == selected;
            slot.SetColor(isSelected ? selectedColor : normalColor);
        }
    }

    private void RefreshTabs()
    {
        SetTabColor(baseTabButton, IngredientKind.Base);
        SetTabColor(fillingTabButton, IngredientKind.Filling);
        SetTabColor(toppingTabButton, IngredientKind.Topping);
    }

    private void SetTabColor(Button tab, IngredientKind category)
    {
        if (tab == null || tab.image == null) return;

        tab.image.color = category == currentCategory ? selectedColor : normalColor;
    }

    private void RefreshDish()
    {
        if (dishVisualizer != null) dishVisualizer.DisplayDish(currentRecipe);
        if (serveButton != null) serveButton.interactable = currentRecipe.IsValid();
    }

    private void ShowDescription(IngredientData ingredient)
    {
        if (descriptionText == null) return;

        if (ingredient == null)
        {
            descriptionText.text = $"Pick a {currentCategory.ToString().ToLowerInvariant()}...";
            return;
        }

        descriptionText.text = $"<size=130%>{ingredient.displayName}</size>\n{ingredient.description}";
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
