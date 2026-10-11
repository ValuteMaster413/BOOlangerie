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

    [Header("Description Panel")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    private IngredientKind currentCategory = IngredientKind.Base;

    private void Awake()
    {
        if (baseTabButton != null) baseTabButton.onClick.AddListener(() => SwitchCategory(IngredientKind.Base));
        if (fillingTabButton != null) fillingTabButton.onClick.AddListener(() => SwitchCategory(IngredientKind.Filling));
        if (toppingTabButton != null) toppingTabButton.onClick.AddListener(() => SwitchCategory(IngredientKind.Topping));
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
        
        Debug.Log($"Selected ingredient: {ingredient.displayName}");
    }

    private void ShowDescription(IngredientData ingredient)
    {
        if (descriptionText != null && ingredient != null)
        {
            descriptionText.text = ingredient.description;
        }
    }
}