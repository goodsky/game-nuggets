using System;
using System.Collections.Generic;

namespace RoboPoker
{
    /// <summary>
    /// A recording of a round during the game.
    /// This can be used to look back at the history of the executing game.
    /// </summary>
    public class RoundState
    {
        /// <summary>
        /// The final pot for this round
        /// </summary>
        public int FinalPot { get; private set; }

        /// <summary>
        /// The final winner(s) for this round
        /// If a pot is split, both winners will be listed
        /// </summary>
        public string[] Winner { get; private set; }

        /// <summary>
        /// Dictionary of revealed hands during this round.
        /// The key is the name of the player whose had was revealed.
        /// If a player does not reveal their hand during the round it will not be contained in the dictionary.
        /// </summary>
        public Dictionary<string, Card[]> Hands { get; private set; }

        /// <summary>
        /// List of actions, in order that they happened in the round.
        /// The name of the player taking the action is followed by their action.
        /// </summary>
        public List<RoundAction> Actions { get; private set; }

        /// <summary>
        /// Create an instance of a round summary with all the details of the round.
        /// </summary>
        /// <param name="finalPot">The final pot in the round</param>
        /// <param name="winner">The name(s) of the winner(s) for the round</param>
        /// <param name="hands">A dictionary of hands for the round</param>
        /// <param name="actions">A list of the round actions made in the round</param>
        internal RoundState(int finalPot, string[] winner, Dictionary<string, Card[]> hands, List<RoundAction> actions)
        {
            this.FinalPot = finalPot;
            this.Winner = winner;
            this.Hands = hands;
            this.Actions = actions;
        }
    }

    /// <summary>
    /// An action taken by a player during a round.
    /// </summary>
    public class RoundAction
    {
        public string PlayerName { get; private set; }

        public TurnAction PlayerAction { get; private set; }

        internal RoundAction(string name, TurnAction action)
        {
            this.PlayerName = name;
            this.PlayerAction = action;
        }
    }
}
