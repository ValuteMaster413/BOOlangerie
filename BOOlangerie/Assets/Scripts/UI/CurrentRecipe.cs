using UnityEngine;
using BOO.Data;

[System.Serializable]
public class CurrentRecipe
{
    public IngredientData selectedBase;
    public IngredientData selectedFilling;
    public IngredientData selectedTopping;
    public bool IsValid()
    {
        return selectedBase != null;
    }

    public void Reset()
    {
        selectedBase = null;
        selectedFilling = null;
        selectedTopping = null;
    }
}
