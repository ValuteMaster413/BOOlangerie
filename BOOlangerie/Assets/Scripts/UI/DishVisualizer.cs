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

        DishRecipeVisualData config = visualConfigs.Find(c => c != null && c.baseIngredient == recipe.selectedBase);

        if (config == null)
        {
            SetElementVisual(baseImage, IconOf(recipe.selectedBase), true);
            SetElementVisual(fillingImage, IconOf(recipe.selectedFilling), true);
            SetElementVisual(toppingImage, IconOf(recipe.selectedTopping), true);
            return;
        }

        SetElementVisual(baseImage, config.baseSprite, true);

        Sprite fillingSprite = config.GetFillingSprite(recipe.selectedFilling);
        if (fillingSprite == null) fillingSprite = IconOf(recipe.selectedFilling);
        SetElementVisual(fillingImage, fillingSprite, true);

        Sprite toppingSprite = config.GetToppingSprite(recipe.selectedTopping);
        if (toppingSprite == null) toppingSprite = IconOf(recipe.selectedTopping);
        SetElementVisual(toppingImage, toppingSprite, true);
    }

    public void ClearVisuals()
    {
        SetElementVisual(baseImage, null, false);
        SetElementVisual(fillingImage, null, false);
        SetElementVisual(toppingImage, null, false);
    }

    private Sprite IconOf(IngredientData ingredient)
    {
        return ingredient != null ? ingredient.icon : null;
    }

    private void SetElementVisual(Image imgComponent, Sprite sprite, bool isVisible)
    {
        if (imgComponent == null) return;

        if (isVisible && sprite != null)
        {
            imgComponent.sprite = sprite;
            imgComponent.preserveAspect = true;
            imgComponent.gameObject.SetActive(true);
        }
        else
        {
            imgComponent.sprite = null;
            imgComponent.gameObject.SetActive(false);
        }
    }
}
