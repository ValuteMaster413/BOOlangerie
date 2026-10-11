using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BOO.UI
{
    public class HoverTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Color normalColor = Color.black;
        [SerializeField] private Color hoverColor = Color.white;
        [Range(0f, 1f)][SerializeField] private float hoverOutlineWidth = 0f;
        [SerializeField] private Color outlineColor = Color.black;

        private void Awake()
        {
            if (text == null) text = GetComponentInChildren<TMP_Text>();
        }

        private void OnEnable()
        {
            Apply(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Apply(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Apply(false);
        }

        private void Apply(bool hovered)
        {
            text.color = hovered ? hoverColor : normalColor;

            if (hoverOutlineWidth <= 0f) return;

            if (hovered)
            {
                text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
                text.outlineColor = outlineColor;
            }

            text.outlineWidth = hovered ? hoverOutlineWidth : 0f;
        }
    }
}