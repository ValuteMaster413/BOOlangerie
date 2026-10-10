using BOO.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    public class WarmthMeterUI : MonoBehaviour
    {
        [SerializeField] private WarmthMeter meter;
        [SerializeField] private Slider slider;
        [SerializeField] private float fillSpeed = 0.5f;

        private void Start()
        {
            slider.value = meter.Value;
        }

        private void Update()
        {
            slider.value = Mathf.MoveTowards(slider.value, meter.Value, fillSpeed * Time.deltaTime);
        }
    }
}