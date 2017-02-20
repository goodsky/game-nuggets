namespace RoboPoker
{
    /// <summary>
    /// Standard face card suits
    /// </summary>
    public enum CardSuit
    {
        Spade,
        Heart,
        Club,
        Diamond
    }

    /// <summary>
    /// Possible values on the card
    /// </summary>
    public enum CardValue
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    /// <summary>
    /// A single card from a 52 card deck
    /// </summary>
    public struct Card
    {
        /// <summary>
        /// The suit of the card.
        /// Spade, Heart, Club or Diamond
        /// </summary>
        public CardSuit Suit;

        /// <summary>
        /// The value of the card.
        /// 2 - 10
        /// Jack
        /// Queen
        /// King
        /// Ace
        /// </summary>
        public CardValue Value;
    }
}