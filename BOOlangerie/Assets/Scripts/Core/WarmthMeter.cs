using System;
using UnityEngine;

namespace BOO.Core
{
    public class WarmthMeter : MonoBehaviour
    {
        public static WarmthMeter Instance { get; private set; }

        [Range(0f, 1f)][SerializeField] private float startValue = 0.3f;
        [Range(0f, 1f)][SerializeField] private float gainOnGood = 0.15f;
        [Range(0f, 1f)][SerializeField] private float lossOnBad = 0.1f;

        public float Value { get; private set; }
        public event Action<float> Changed;

        private void Awake()
        {
            Instance = this;
            Value = startValue;
        }

        private void Start()
        {
            GameManager.Instance.OrderServed += OnOrderServed;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OrderServed -= OnOrderServed;
        }

        private void OnOrderServed(OrderResult result)
        {
            float change = result == OrderResult.Good ? gainOnGood : -lossOnBad;
            Value = Mathf.Clamp01(Value + change);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(result == OrderResult.Good ? "meter_up" : "meter_down");
            }

            Debug.Log($"Warmth: {Value:0.00}");
            Changed?.Invoke(Value);
        }
    }
}