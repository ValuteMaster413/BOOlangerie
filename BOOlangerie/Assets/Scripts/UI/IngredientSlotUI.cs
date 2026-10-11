using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BOO.Data;
using System;

public class IngredientSlotUI : MonoBehaviour
{
    [SerializeField] private Button slotButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;

    private IngredientData assignedIngredient;
    private Action<IngredientData> onClickCallback;

    public IngredientData Ingredient => assignedIngredient;

    public void Setup(IngredientData ingredient, Action<IngredientData> clickCallback)
    {
        assignedIngredient = ingredient;
        onClickCallback = clickCallback;

        if (nameText != null) nameText.text = ingredient.displayName;

        if (iconImage != null)
        {
            iconImage.sprite = ingredient.icon;
            iconImage.enabled = ingredient.icon != null;
            iconImage.preserveAspect = true;
        }

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onClickCallback?.Invoke(assignedIngredient));
        }
    }

    public void SetColor(Color color)
    {
        if (slotButton != null && slotButton.image != null) slotButton.image.color = color;
    }
}
