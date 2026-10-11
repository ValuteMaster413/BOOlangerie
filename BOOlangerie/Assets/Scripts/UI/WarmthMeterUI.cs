using BOO.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    public class WarmthMeterUI : MonoBehaviour
    {
        [SerializeField] private WarmthMeter meter;
        [SerializeField] private Image fill;
        [SerializeField] private float fillSpeed = 0.5f;

        [Header("Colours")]
        [SerializeField] private Color coldColor = new Color32(92, 198, 208, 255);
        [SerializeField] private Color lukewarmColor = new Color32(245, 184, 65, 255);
        [SerializeField] private Color warmColor = new Color32(233, 86, 43, 255);
        [Range(0f, 1f)][SerializeField] private float lukewarmAt = 0.4f;
        [Range(0f, 1f)][SerializeField] private float warmAt = 0.8f;
        [SerializeField] private float colorChangeTime = 0.6f;

        private float shown;
        private Color fromColor;
        private Color targetColor;
        private float colorProgress = 1f;

        private void Start()
        {
            shown = meter.Value;
            targetColor = ColorFor(meter.Value);
            fromColor = targetColor;

            fill.fillAmount = shown;
            fill.color = targetColor;
        }

        private void Update()
        {
            shown = Mathf.MoveTowards(shown, meter.Value, fillSpeed * Time.deltaTime);
            fill.fillAmount = shown;

            Color wanted = ColorFor(meter.Value);
            if (wanted != targetColor)
            {
                fromColor = fill.color;
                targetColor = wanted;
                colorProgress = 0f;
            }

            if (colorProgress < 1f)
            {
                colorProgress = Mathf.MoveTowards(colorProgress, 1f, Time.deltaTime / Mathf.Max(colorChangeTime, 0.01f));
                fill.color = Color.Lerp(fromColor, targetColor, colorProgress);
            }
        }

        private Color ColorFor(float warmth)
        {
            if (warmth >= warmAt) return warmColor;
            if (warmth >= lukewarmAt) return lukewarmColor;

            return coldColor;
        }
    }
}