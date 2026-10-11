using System;
using BOO.Data;

namespace BOO.Core
{
    [Serializable]
    public class Order
    {
        public IngredientData baseItem;
        public IngredientData filling;
        public IngredientData topping;

        public bool Matches(Order other)
        {
            return other != null && baseItem == other.baseItem && filling == other.filling && topping == other.topping;
        }
    }

    [Serializable]
    public class OrderRound
    {
        public Order[] accepted;
    }
}