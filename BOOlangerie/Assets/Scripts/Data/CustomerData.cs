using BOO.Core;
using UnityEngine;

namespace BOO.Data
{
    [CreateAssetMenu(menuName = "BOOlangerie/Customer")]
    public class CustomerData : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite sprite;
        public Sprite monsterSprite;
        public AudioClip theme;
        public AudioClip enterSfx;
        public AudioClip monsterSfx;
        public OrderRound[] rounds;
    }
}