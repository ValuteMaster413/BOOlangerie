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

    public void Setup(IngredientData ingredient, Action<IngredientData> clickCallback)
    {
        assignedIngredient = ingredient;
        onClickCallback = clickCallback;

        if (nameText != null) nameText.text = ingredient.displayName;
        if (iconImage != null && ingredient.icon != null) iconImage.sprite = ingredient.icon;

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onClickCallback?.Invoke(assignedIngredient));
        }
    }
}