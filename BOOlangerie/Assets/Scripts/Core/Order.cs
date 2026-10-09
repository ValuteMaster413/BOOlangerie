using System;
using System.Collections.Generic;
using BOO.Data;

namespace BOO.Core
{
    [Serializable]
    public class Order
    {
        public IngredientData baseItem;
        public List<IngredientData> toppings = new();
    }
}