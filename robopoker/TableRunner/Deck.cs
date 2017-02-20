using RoboPoker;

using System;

namespace TableRunner
{
    /// <summary>
    /// A standard 52 card deck with no jokers
    /// </summary>
    internal class Deck
    {
        /// <summary>
        /// The cards in the deck
        /// </summary>
        private Card[] deck;

        /// <summary>
        /// Keeps track of how many cards have been drawn.
        /// </summary>
        private int cardsDrawn;

        /// <summary>
        /// Gets the number of cards left in the deck
        /// </summary>
        public int CardsLeft { get { return this.deck.Length - this.cardsDrawn; } }

        /// <summary>
        /// Create an instance of the deck.
        /// The deck is not shuffled initially and the cards will be in order.
        /// </summary>
        public Deck()
        {
            // Create 52 cards
            this.deck = new Card[52];

            for (int suit = 0; suit < 4; ++suit)
            {
                for (int value = 0; value < 13; ++value)
                {
                    this.deck[suit * 13 + value].Suit = (CardSuit)suit;
                    this.deck[suit * 13 + value].Value = (CardValue)value;
                }
            }
        }

        /// <summary>
        /// Shuffle using the Fisher-Yates in-place shuffle.
        /// O(n)
        /// </summary>
        public void Shuffle()
        {
            // IRoboPlayers might have some way to recreate our random seed value if we're not careful...
            // Let's make this more secure some time.
            Random rnd = new Random();

            for (int i = 51; i >= 0; --i)
            {
                int swapIndex = rnd.Next(0, i + 1);
                Card swapCard = this.deck[swapIndex];
                this.deck[swapIndex] = this.deck[i];
                this.deck[i] = swapCard;
            }

            // Reset the cards in the deck so we can keep playing
            this.cardsDrawn = 0;
        }

        /// <summary>
        /// Draws the top card from the deck and returns it.
        /// It removes the card from the stack so you won't be seeing it again.
        /// After the entire deck has been draw we will shuffle and start drawing again.
        /// </summary>
        /// <returns>The top card from the deck</returns>
        public Card Draw()
        {
            if (cardsDrawn >= this.deck.Length)
            {
                this.Shuffle();
            }

            return this.deck[this.cardsDrawn++];
        }
    }
}
