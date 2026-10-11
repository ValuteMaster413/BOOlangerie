using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    public class SpriteLoop : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 6f;

        private void Update()
        {
            if (frames.Length == 0) return;

            image.sprite = frames[(int)(Time.time * framesPerSecond) % frames.Length];
        }
    }
}