using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BOO.Core;
using BOO.Data;

public class DishVisualizer : MonoBehaviour
{
    [Header("Visual Configurations Database")]
    [SerializeField] private List<DishRecipeVisualData> visualConfigs;

    [Header("UI Image Components")]
    [SerializeField] private Image baseImage;
    [SerializeField] private Image fillingImage;
    [SerializeField] private Image toppingImage;

    [Header("Placement")]
    [SerializeField] private Vector2 dishPosition = new Vector2(-36f, 72f);

    public void DisplayDish(CurrentRecipe recipe)
    {
        if (recipe == null)
        {
            ClearVisuals();
            return;
        }

        DisplayDish(recipe.selectedBase, recipe.selectedFilling, recipe.selectedTopping);
    }

    public void DisplayDish(Order order)
    {
        if (order == null)
        {
            ClearVisuals();
            return;
        }

        DisplayDish(order.baseItem, order.filling, order.topping);
    }

    public void DisplayDish(IngredientData baseItem, IngredientData filling, IngredientData topping)
    {
        if (baseItem == null)
        {
            ClearVisuals();
            return;
        }

        DishRecipeVisualData config = visualConfigs.Find(c => c != null && c.baseIngredient == baseItem);

        if (config == null)
        {
            SetElementVisual(baseImage, IconOf(baseItem), true);
            SetElementVisual(fillingImage, IconOf(filling), true);
            SetElementVisual(toppingImage, IconOf(topping), true);
            return;
        }

        SetElementVisual(baseImage, config.baseSprite, true);

        Sprite fillingSprite = config.GetFillingSprite(filling);
        if (fillingSprite == null) fillingSprite = IconOf(filling);
        SetElementVisual(fillingImage, fillingSprite, true);

        Sprite toppingSprite = config.GetToppingSprite(topping);
        if (toppingSprite == null) toppingSprite = IconOf(topping);
        SetElementVisual(toppingImage, toppingSprite, true);
    }

    public void SetTint(Color tint)
    {
        if (baseImage != null) baseImage.color = tint;
        if (fillingImage != null) fillingImage.color = tint;
        if (toppingImage != null) toppingImage.color = tint;
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

    private void PlaceOnCanvas(Image imgComponent, Sprite sprite)
    {
        Vector2 canvasCenter = new Vector2(sprite.texture.width, sprite.texture.height) * 0.5f;

        imgComponent.rectTransform.sizeDelta = sprite.rect.size;
        imgComponent.rectTransform.anchoredPosition = dishPosition + sprite.rect.center - canvasCenter;
    }

    private void SetElementVisual(Image imgComponent, Sprite sprite, bool isVisible)
    {
        if (imgComponent == null) return;

        if (isVisible && sprite != null)
        {
            imgComponent.sprite = sprite;
            PlaceOnCanvas(imgComponent, sprite);
            imgComponent.gameObject.SetActive(true);
        }
        else
        {
            imgComponent.sprite = null;
            imgComponent.gameObject.SetActive(false);
        }
    }
}
