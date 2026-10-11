namespace BOO.Core
{
    public static class OrderEvaluator
    {
        public static OrderResult Evaluate(OrderRound wanted, Order made)
        {
            if (wanted == null || made == null) return OrderResult.Bad;

            foreach (Order accepted in wanted.accepted)
            {
                if (accepted.Matches(made)) return OrderResult.Good;
            }

            return OrderResult.Bad;
        }
    }
}