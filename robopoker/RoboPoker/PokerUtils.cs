namespace RoboPoker
{
    /// <summary>
    /// Static utility method to help you play Poker
    /// </summary>
    public class PokerUtils
    {
        /// <summary>
        /// Utility to help sort cards by their Value.
        /// Cards are sorted by their value first and then by their suit within the value.
        /// </summary>
        /// <param name="card1">First card to compare</param>
        /// <param name="card2">Card to compare against the first card</param>
        /// <returns>A negative number if card1 goes before card2, a non-zero positive if it goes after</returns>
        public static int CompareByValues(Card card1, Card card2)
        {
            int compareValue = card1.Value.CompareTo(card2.Value);
            if (compareValue != 0)
            {
                return compareValue;
            }

            int compareSuit = card1.Suit.CompareTo(card2.Suit);
            return compareSuit;
        }

        /// <summary>
        /// Utility to help sort cards by Suit.
        /// Cards are sorted by their enum suit value (Spade, Heart, Club, Diamond) and then by their card value within suits.
        /// </summary>
        /// <param name="card1">First card to compare</param>
        /// <param name="card2">Card to compare against the first card</param>
        /// <returns>A negative number if card1 goes before card2, a non-zero positive if it goes after</returns>
        public static int CompareBySuits(Card card1, Card card2)
        {
            int compareSuit = card1.Suit.CompareTo(card2.Suit);
            if (compareSuit != 0)
            {
                return compareSuit;
            }

            int compareValue = card1.Value.CompareTo(card2.Value);
            return compareValue;
        }

        /// <summary>
        /// Print out the card.
        /// If I make the card a class instead of a struct this helper won't be needed
        /// </summary>
        /// <param name="c">The card</param>
        /// <returns>The full card name</returns>
        public static string PrintCard(Card c)
        {
            return string.Format("{0} of {1}s", c.Value.ToString(), c.Suit.ToString());
        }
    }
}
