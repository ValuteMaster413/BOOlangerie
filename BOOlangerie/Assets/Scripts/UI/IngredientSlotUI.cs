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
    private Sprite emptyIcon;
    private bool hasEmptyIcon;

    public IngredientData Ingredient => assignedIngredient;

    public void Setup(IngredientData ingredient, Action<IngredientData> clickCallback)
    {
        assignedIngredient = ingredient;
        onClickCallback = clickCallback;

        if (nameText != null) nameText.text = ingredient.displayName;

        if (iconImage != null)
        {
            if (!hasEmptyIcon)
            {
                emptyIcon = iconImage.sprite;
                hasEmptyIcon = true;
            }

            iconImage.sprite = ingredient.icon != null ? ingredient.icon : emptyIcon;
            iconImage.enabled = iconImage.sprite != null;
            iconImage.preserveAspect = true;
        }

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onClickCallback?.Invoke(assignedIngredient));
        }
    }

    public void SetLook(Color barColor, Color nameColor)
    {
        if (slotButton != null && slotButton.image != null) slotButton.image.color = barColor;
        if (nameText != null) nameText.color = nameColor;
    }
}
