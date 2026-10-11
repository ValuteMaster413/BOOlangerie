using UnityEngine;
using BOO.Data;

[CreateAssetMenu(menuName = "BOOlangerie/DishVisualData")]
public class DishRecipeVisualData : ScriptableObject
{
    [Header("Base")]
    public IngredientData baseIngredient;
    public Sprite baseSprite;

    [Header("Filling Visuals for this Base")]
    public FillingVisualPair[] fillingVisuals;

    [Header("Topping Visuals for this Base")]
    public ToppingVisualPair[] toppingVisuals;

    public Sprite GetFillingSprite(IngredientData filling)
    {
        if (filling == null) return null;
        foreach (FillingVisualPair pair in fillingVisuals)
        {
            if (pair.filling == filling) return pair.sprite;
        }
        return null;
    }

    public Sprite GetToppingSprite(IngredientData topping)
    {
        if (topping == null) return null;
        foreach (ToppingVisualPair pair in toppingVisuals)
        {
            if (pair.topping == topping) return pair.sprite;
        }
        return null;
    }
}

[System.Serializable]
public struct FillingVisualPair
{
    public IngredientData filling;
    public Sprite sprite;
}

[System.Serializable]
public struct ToppingVisualPair
{
    public IngredientData topping;
    public Sprite sprite;
}