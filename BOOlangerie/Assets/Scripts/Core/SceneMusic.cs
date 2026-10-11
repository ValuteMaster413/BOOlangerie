using UnityEngine;

namespace BOO.Core
{
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip clip;

        private void Start()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(clip);
        }
    }
}