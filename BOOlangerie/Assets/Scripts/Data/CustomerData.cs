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
        public Order[] orders;
    }
}