namespace RoboPoker
{
    /// <summary>
    /// This class stores information about the state of a player around the table.
    /// This state reflects the current state of the player in the round.
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// The name of this player
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The total amount of money in posession of this player.
        /// This includes the money bet in the current round.
        /// </summary>
        public int Money { get; private set; }

        /// <summary>
        /// The current amount bet on the round.
        /// </summary>
        public int Bet { get; private set; }

        /// <summary>
        /// Boolean indicating whether this player revealed their hand this round
        /// </summary>
        public bool RevealedCards { get; private set; }

        /// <summary>
        /// The player's cards if they revealed them on the round.
        /// If RevealedCards is false then this will be an empty array
        /// </summary>
        public Card[] Cards { get; private set; }

        /// <summary>
        /// True if this player has folded and is not in the current round. 
        /// </summary>
        public bool Folded { get; private set; }

        /// <summary>
        /// true if this player is all-in on the current round.
        /// </summary>
        public bool AllIn { get; private set; }

        /// <summary>
        /// True if this player has dropped out of the current game.
        /// </summary>
        public bool Dropped { get; private set; }

        /// <summary>
        /// Create an instance of a player state
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="money">Money in the possesion of the player</param>
        /// <param name="bet">Current amount bet by this player</param>
        /// <param name="cards">The cards of the player. An empty array if the player did not reveal their hand.</param>
        /// <param name="folded">A flag indicating whether the player has folded</param>
        /// <param name="allIn">A flag indicating whether the player is all in</param>
        /// <param name="dropped">A flag indicatign whether the player has dropped out of the game</param>
        internal PlayerState(string name, int money, int bet, Card[] cards, bool folded, bool allIn, bool dropped)
        {
            this.Name = name;
            this.Money = money;
            this.Bet = bet;
            this.RevealedCards = (cards != null && cards.Length > 0);
            this.Cards = cards;
            this.Folded = folded;
            this.AllIn = allIn;
            this.Dropped = dropped;
        }
    }
}
