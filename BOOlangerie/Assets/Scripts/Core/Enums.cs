namespace BOO.Core
{
    public enum GameState { Intro, Ordering, Prep, Reaction, Ending }

    public enum OrderResult { Good, Bad }

    public enum IngredientKind { Base, Filling, Topping}

    public enum Flavor 
    { 
        //Base
        Crust, Dough, Brioche,
        //Filling
        Fungi, Apple, Yam,
        //Topping
        Bacony, Crunchy, Nut,
        //General
        Sweet, Salty, Savory 
    }
}
