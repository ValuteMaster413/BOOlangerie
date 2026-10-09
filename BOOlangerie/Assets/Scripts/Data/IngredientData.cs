using BOO.Core;
using UnityEngine;

namespace BOO.Data
{
    [CreateAssetMenu(menuName = "BOOlangerie/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        [TextArea] public string description;
        public IngredientKind kind;
        public Flavor flavor;
    }
}