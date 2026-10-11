using BOO.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BOO.UI
{
    public class ButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        private Selectable selectable;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CanPlay()) AudioManager.Instance.PlayHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CanPlay()) AudioManager.Instance.PlayClick();
        }

        private bool CanPlay()
        {
            if (AudioManager.Instance == null) return false;

            return selectable == null || selectable.interactable;
        }
    }
}