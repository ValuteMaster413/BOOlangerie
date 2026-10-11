using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BOO.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BlurredBackdrop : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [Range(1, 5)][SerializeField] private int blurSteps = 3;

        private CanvasGroup group;
        private RenderTexture blurred;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            if (image == null) image = GetComponent<RawImage>();
        }

        private void OnEnable()
        {
            group.alpha = 0f;
            StartCoroutine(Capture());
        }

        private void OnDisable()
        {
            Release();
        }

        private IEnumerator Capture()
        {
            yield return new WaitForEndOfFrame();

            Texture2D shot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            shot.Apply();

            Release();
            blurred = Blur(shot);
            Destroy(shot);

            image.texture = blurred;
            group.alpha = 1f;
        }

        private RenderTexture Blur(Texture source)
        {
            int width = source.width;
            int height = source.height;
            Texture from = source;
            RenderTexture current = null;

            for (int i = 0; i < blurSteps; i++)
            {
                width = Mathf.Max(1, width / 2);
                height = Mathf.Max(1, height / 2);

                RenderTexture next = RenderTexture.GetTemporary(width, height, 0);
                next.filterMode = FilterMode.Bilinear;
                Graphics.Blit(from, next);

                if (current != null) RenderTexture.ReleaseTemporary(current);
                current = next;
                from = next;
            }

            return current;
        }

        private void Release()
        {
            if (blurred == null) return;

            image.texture = null;
            RenderTexture.ReleaseTemporary(blurred);
            blurred = null;
        }
    }
}