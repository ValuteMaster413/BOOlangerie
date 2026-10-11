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

    private IngredientKind currentCategory = IngredientKind.Base;
    private CurrentRecipe currentRecipe = new CurrentRecipe();

    private void Awake()
    {
        if (baseTabButton != null) baseTabButton.onClick.AddListener(() => SwitchCategory(IngredientKind.Base));
        if (fillingTabButton != null) fillingTabButton.onClick.AddListener(() => SwitchCategory(IngredientKind.Filling));
        if (toppingTabButton != null) toppingTabButton.onClick.AddListener(() => SwitchCategory(IngredientKind.Topping));
        
        if (resetButton != null) resetButton.onClick.AddListener(ResetRecipe);
        if (serveButton != null) serveButton.onClick.AddListener(ServeDish);
    }

    private void Start()
    {
        SwitchCategory(IngredientKind.Base);
    }

    private void OnDestroy()
    {
        if (baseTabButton != null) baseTabButton.onClick.RemoveAllListeners();
        if (fillingTabButton != null) fillingTabButton.onClick.RemoveAllListeners();
        if (toppingTabButton != null) toppingTabButton.onClick.RemoveAllListeners();
        
        if (resetButton != null) resetButton.onClick.RemoveListener(ResetRecipe);
        if (serveButton != null) serveButton.onClick.RemoveListener(ServeDish);
    }

    public void SwitchCategory(IngredientKind category)
    {
        currentCategory = category;
        UpdateVisibleIngredients();
    }

    private void UpdateVisibleIngredients()
    {
        List<IngredientData> filteredIngredients = allIngredients.FindAll(ing => ing.kind == currentCategory);

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
        
        if (descriptionText != null)
        {
            descriptionText.text = "Select an ingredient...";
        }
    }

    private void OnIngredientSelected(IngredientData ingredient)
    {
        ShowDescription(ingredient);
        
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
        
        if (dishVisualizer != null)
        {
            dishVisualizer.DisplayDish(currentRecipe);
        }
    }

    private void ShowDescription(IngredientData ingredient)
    {
        if (descriptionText != null && ingredient != null)
        {
            descriptionText.text = ingredient.description;
        }
    }
    
    private void ResetRecipe()
    {
        currentRecipe.Reset();
        Debug.Log("Recipe reset!");
        
        if (dishVisualizer != null)
        {
            dishVisualizer.ClearVisuals();
        }
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