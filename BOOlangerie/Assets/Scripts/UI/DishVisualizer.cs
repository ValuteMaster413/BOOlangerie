using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BOO.Data;

public class DishVisualizer : MonoBehaviour
{
    [Header("Visual Configurations Database")]
    [SerializeField] private List<DishRecipeVisualData> visualConfigs;

    [Header("UI Image Components")]
    [SerializeField] private Image baseImage;
    [SerializeField] private Image fillingImage;
    [SerializeField] private Image toppingImage;

    public void DisplayDish(CurrentRecipe recipe)
    {
        if (recipe == null || recipe.selectedBase == null)
        {
            ClearVisuals();
            return;
        }
        
        DishRecipeVisualData config = visualConfigs.Find(c => c.baseIngredient == recipe.selectedBase);

        if (config == null)
        {
            Debug.LogWarning($"Visual config not found for base: {recipe.selectedBase.displayName}");
            ClearVisuals();
            return;
        }
        
        SetElementVisual(baseImage, config.baseSprite, true);

        Sprite fillingSprite = config.GetFillingSprite(recipe.selectedFilling);
        if (fillingSprite != null)
        {
            SetElementVisual(fillingImage, fillingSprite, true);
        }
        else
        {
            SetElementVisual(fillingImage, null, false);
        }

        Sprite toppingSprite = config.GetToppingSprite(recipe.selectedTopping);
        if (toppingSprite != null)
        {
            SetElementVisual(toppingImage, toppingSprite, true);
        }
        else
        {
            SetElementVisual(toppingImage, null, false);
        }
    }

    public void ClearVisuals()
    {
        SetElementVisual(baseImage, null, false);
        SetElementVisual(fillingImage, null, false);
        SetElementVisual(toppingImage, null, false);
    }

    private void SetElementVisual(Image imgComponent, Sprite sprite, bool isVisible)
    {
        if (imgComponent == null) return;

        if (isVisible && sprite != null)
        {
            imgComponent.sprite = sprite;
            imgComponent.gameObject.SetActive(true);
        }
        else
        {
            imgComponent.sprite = null;
            imgComponent.gameObject.SetActive(false);
        }
    }
}