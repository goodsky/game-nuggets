using System.Collections.Generic;
namespace RoboPoker
{
    /// <summary>
    /// Enumeration of possible round states
    /// </summary>
    public enum RoundStep
    {
        PreFlop,
        Flop,
        Turn,
        River,
        Complete
    }

    /// <summary>
    /// This class stores all the context required about the Table to make player decisions.
    /// There is information abou the players around the table, their money and also the history of past rounds at the table.
    /// </summary>
    public class TableState
    {
        /// <summary>
        /// Gets the step the table is on.
        /// PreFlop, Flop, Turn or River
        /// </summary>
        public RoundStep Step { get; private set; }

        /// <summary>
        /// The number of players around the table.
        /// </summary>
        public int PlayerCount { get; private set; }

        /// <summary>
        /// The state of the players around the table.
        /// The players are given in clockwise order, starting with the current player.
        /// </summary>
        public TableStatePlayers Players { get; private set; }

        /// <summary>
        /// The face-up community cards on the table.
        /// As more cards are put face-up this list will increase in size.
        /// </summary>
        public List<Card> CommunityCards { get; private set; }

        /// <summary>
        /// A list of the results from previous rounds
        /// </summary>
        public List<RoundState> RoundHistory { get; private set; }

        /// <summary>
        /// The current bet for the round. This will increase as players raise the bet.
        /// </summary>
        public int CurrentBet { get; private set; }

        /// <summary>
        /// The minimum bet or "big blind". All raises must be at least this amount.
        /// It will increase as the game goes on.
        /// </summary>
        public int MinimumBet { get; private set; }

        /// <summary>
        /// Current value of money in the pot on the table. This includes the money that is currently bet in the round.
        /// </summary>
        public int Pot { get; private set; }

        /// <summary>
        /// Create an instance of the TableState
        /// </summary>
        /// <param name="roundStep">The current step the table is on</param>
        /// <param name="actingPlayer">Index of the current acting player</param>
        /// <param name="players">List of the players</param>
        /// <param name="communityCards">The community cards on the table</param>
        /// <param name="roundHistory">The list of previous rounds</param>
        /// <param name="curBet">The current bet</param>
        /// <param name="minBet">The minimum bet</param>
        /// <param name="pot">The current pot</param>
        internal TableState(RoundStep roundStep, int actingPlayer, List<PlayerState> players, List<Card> communityCards, List<RoundState> roundHistory, int curBet, int minBet, int pot)
        {
            this.Step = roundStep;
            this.PlayerCount = players.Count;
            this.Players = new TableStatePlayers(actingPlayer, players);
            this.CommunityCards = communityCards;
            this.RoundHistory = roundHistory;
            this.CurrentBet = curBet;
            this.MinimumBet = minBet;
            this.Pot = pot;
        }
    }

    /// <summary>
    /// A collection of the players around the table.
    /// May index by player name, or by location around the table.
    /// When indexing by location, 0 will always be the acting player and they will be numbered in a clockwise direction.
    /// </summary>
    public class TableStatePlayers
    {
        /// <summary>
        /// The position around the table for the current player
        /// </summary>
        private int actingPlayerIndex;

        /// <summary>
        /// Players around the table
        /// </summary>
        private List<PlayerState> players;

        /// <summary>
        /// Return the player state of the player sitting 'i' seats clockwise from the current player
        /// </summary>
        /// <param name="i">Number of seats clockwise from the acting player. Negative numbers are ok, because I'm nice. I'll do the mod math for you.</param>
        /// <returns>Player state of the player 'i' seats clockwise from the current player</returns>
        public PlayerState this[int i]
        {
            get
            {
                int index = this.actingPlayerIndex + i;
                while (index < 0) index += this.players.Count;
                return this.players[index%this.players.Count];
            }
        }

        /// <summary>
        /// Returns the player state of the player with the name 'name'.
        /// If no such player exists, null is returned.
        /// </summary>
        /// <param name="name">The name of the player</param>
        /// <returns>Player state of the player with name 'name', null if no such player exists</returns>
        public PlayerState this[string name]
        {
            get
            {
                for (int i = 0; i < this.players.Count; ++i)
                {
                    if (this.players[i].Name == name)
                    {
                        return this.players[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Create an instance of the TableStatePlayers class
        /// </summary>
        /// <param name="actingPlayerIndex">Index of the acting player at the table</param>
        /// <param name="players">Array of the players around the table</param>
        internal TableStatePlayers(int actingPlayerIndex, List<PlayerState> players)
        {
            this.actingPlayerIndex = actingPlayerIndex;
            this.players = players;
        }
    }
}
