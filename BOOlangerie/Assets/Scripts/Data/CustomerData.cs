using BOO.Core;
using UnityEngine;

namespace BOO.Data
{
    [CreateAssetMenu(menuName = "BOOlangerie/Customer")]
    public class CustomerData : ScriptableObject
    {
        public string id;
        public string displayName;

        [Header("Art")]
        public Sprite sprite;
        public Sprite[] entranceFrames;
        public Sprite[] successFrames;
        public Sprite[] failFrames;
        public float framesPerSecond = 8f;
        public bool flickerOnEnter;
        public bool lightsOutOnFail;

        [Header("Audio")]
        public AudioClip theme;
        public AudioClip voiceSfx;

        [Header("Orders")]
        public OrderRound[] rounds;
    }
}
