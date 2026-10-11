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

        private bool hovered;
        private bool pinned;

        private void Awake()
        {
            FindText();
        }

        private void OnEnable()
        {
            hovered = false;
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            Apply();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            Apply();
        }

        public void SetPinned(bool value)
        {
            pinned = value;
            Apply();
        }

        private void FindText()
        {
            if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        }

        private void Apply()
        {
            FindText();

            bool highlighted = hovered || pinned;
            text.color = highlighted ? hoverColor : normalColor;

            if (hoverOutlineWidth <= 0f) return;

            if (highlighted)
            {
                text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
                text.outlineColor = outlineColor;
            }

            text.outlineWidth = highlighted ? hoverOutlineWidth : 0f;
        }
    }
}