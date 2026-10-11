using System.Collections.Generic;
using BOO.Data;

namespace BOO.Core
{
    public static class OrderEvaluator
    {
        public static OrderResult Evaluate(Order wanted, Order made)
        {
            if (wanted == null || made == null) return OrderResult.Bad;
            if (made.baseItem != wanted.baseItem) return OrderResult.Bad;

            var wantedToppings = new HashSet<IngredientData>(wanted.toppings);
            return wantedToppings.SetEquals(made.toppings) ? OrderResult.Good : OrderResult.Bad;
        }
    }
}