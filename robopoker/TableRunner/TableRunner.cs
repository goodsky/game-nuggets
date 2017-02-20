using RoboPoker;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TableRunner
{
    /// <summary>
    /// The host class holds the logic for a RoboPoker game.
    /// The RoboPoker library is stateless and requires this runner to handle the flow of the game.
    /// </summary>
    internal class TableRunner
    {
        /// <summary>
        /// The deck for the table
        /// </summary>
        private Deck deck = null;

        /// <summary>
        /// The players around the table
        /// </summary>
        private List<Player> players = null;

        /// <summary>
        /// A history of the rounds taken at this table this game
        /// </summary>
        private List<RoundState> rounds = null;

        /// <summary>
        /// Set up this table runner as well as the players for the table
        /// </summary>
        /// <param name="players">List of players to start the round</param>
        /// <returns></returns>
        public bool SetUp(List<Player> players)
        {
            // setup the deck
            this.deck = new Deck();

            // setup the players
            this.players = players;
            foreach (var player in this.players)
            {
                player.Implementation.SetUp(player.Name);
            }

            return true;
        }

        /// <summary>
        /// Run the simulation with the players loaded during setup.
        /// This will Run rounds until one player remains or the maximum rounds has been hit.
        /// </summary>
        /// <returns>True if the game completed successfully, false otherwise</returns>
        public bool RunTable()
        {
            if (this.players == null)
            {
                Console.WriteLine("ERROR: No players were loaded to run on this table.");
                return false;
            }

            foreach (var player in this.players)
            {
                player.Money = Config.Instance.StartingCash;
            }

            int dealer = new Random().Next(0, this.players.Count);
            int bigBlind = Config.Instance.BigBlindStart;

            this.rounds = new List<RoundState>();
            for (int round = 1; round <= Config.Instance.MaxRounds; ++round)
            {
                Logger.Log(LogLevel.Info, "*********************");
                Logger.Log(LogLevel.Info, "Starting Round #{0}", round);

                foreach (var player in this.players)
                {
                    player.ResetRound();
                    if (!player.Dropped && player.Money < bigBlind)
                    {
                        Logger.Log(LogLevel.Info, "   {0} was forced to Drop! ${1} left", player.Name, player.Money);
                        player.Drop();
                    }

                    Logger.Log(LogLevel.Info, "   {0}- ${1} {2}", player.Name, player.Money, player.Dropped ? "(dropped)" : string.Empty);
                }

                Logger.Log(LogLevel.Info, "*********************");

                dealer = this.NextPlayer(dealer);

                var result = this.RunRound(dealer, bigBlind); 

                if (result == null)
                {
                    break;
                }

                Logger.Log(LogLevel.Info, string.Empty);

                if (Config.Instance.BigBlindTurnPeriod != -1 && round % Config.Instance.BigBlindTurnPeriod == 0)
                {
                    bigBlind += Config.Instance.BigBlindLinear;
                    bigBlind = (int)((double)bigBlind * Config.Instance.BigBlindPercent);
                }

                this.rounds.Add(result);
            }

            return true;
        }

        /// <summary>
        /// Run a round of poker
        /// </summary>
        /// <param name="dealerIndex">The index of the dealer</param>
        /// <param name="bigBlind">The big blind for this round</param>
        /// <param name="output">StreamWriter to write the summary output</param>
        /// <returns>A round state object summary of the round.</returns>
        internal RoundState RunRound(int dealerIndex, int bigBlind)
        {
            // Shuffle the deck first thing
            // Don't forget to do this!
            this.deck.Shuffle();

            // The total amount in the pot
            int pot = 0;

            // Current bet
            int bet = bigBlind;

            // Index of the person with the current bet
            int betIndex;

            // Number of players who have called current bet
            int called = 1;

            // Number of players who have folded
            int folded = 0;

            // The step of the round
            RoundStep step = RoundStep.PreFlop;

            // Cards on the table
            List<Card> communityCards = new List<Card>();

            // Values to report back to the RoundState
            // Will be populated as the game goes forward
            string[] winners = null;
            Dictionary<string, Card[]> revealedCards = new Dictionary<string, Card[]>();
            List<RoundAction> actions = new List<RoundAction>();

            // See which players are still in, and drop those with less money than the big blind
            // Deal cards out to the players who are still in
            int playersIn = 0;
            foreach (var player in this.players)
            {
                if (!player.Dropped)
                {
                    ++playersIn;
                    // The cards are draw terribly out of order. I would be dead in the Wild West.
                    player.StartRound(new[] { this.deck.Draw(), this.deck.Draw() });
                }
            }

            // When the game is over, return null
            if (playersIn == 1)
            {
                return null;
            }

            // Select little blind
            int littleBlind = bigBlind / 2; // I round down because I am a lazy programmer
            int littleBlindIndex;
            
            if (playersIn == 2)
            {
                // In head-to-head, the dealer is little blind
                littleBlindIndex = dealerIndex;
            }
            else
            {
                littleBlindIndex = this.NextPlayer(dealerIndex);
            }

            // Select big blind
            int bigBlindIndex = betIndex = this.NextPlayer(littleBlindIndex);

            // Initial bids
            this.players[littleBlindIndex].PostBlind(littleBlind);
            Logger.Log(LogLevel.Info, "Little Blind {0} puts in ${1}", this.players[littleBlindIndex].Name, littleBlind);

            this.players[bigBlindIndex].PostBlind(bigBlind);
            Logger.Log(LogLevel.Info, "Big Blind {0} puts in ${1}", this.players[bigBlindIndex].Name, bigBlind);

            // Take turns until betting is complete
            int actionIndex;
            actionIndex = this.NextPlayer(bigBlindIndex);

            while (step != RoundStep.Complete && playersIn - folded > 1)
            {
                Logger.Log(LogLevel.Info, string.Empty);
                Logger.Log(LogLevel.Info, "At {0} step", step.ToString());

                switch (step)
                {
                    case RoundStep.Flop:
                        this.deck.Draw(); // Why do I burn a card? Because tradition.
                        communityCards.Add(this.deck.Draw());
                        communityCards.Add(this.deck.Draw());
                        communityCards.Add(this.deck.Draw());
                        Logger.Log(LogLevel.Info, "Flop: {0} / {1} / {2}", PokerUtils.PrintCard(communityCards[0]), PokerUtils.PrintCard(communityCards[1]), PokerUtils.PrintCard(communityCards[2]));
                        break;
                    case RoundStep.Turn:
                    case RoundStep.River:
                        this.deck.Draw();
                        communityCards.Add(this.deck.Draw());
                        Logger.Log(LogLevel.Info, "{0}: {1}", step.ToString(), PokerUtils.PrintCard(communityCards[communityCards.Count - 1]));
                        break;
                }

                // In head-to-head the dealer goes first only in pre-flop, and goes last in all other rounds
                if (playersIn == 2)
                {
                    if (step == RoundStep.PreFlop)
                        actionIndex = dealerIndex;
                    else
                        actionIndex = this.NextPlayer(bigBlindIndex);
                }

                called = 0;
                while (called < playersIn - folded)
                {
                    // Create the Table State for this player's action
                    var playerState = new List<PlayerState>();
                    foreach (var p in this.players) playerState.Add(p.State);
                    var tableState = new TableState(step, actionIndex, playerState, communityCards, rounds, bet, bigBlind, pot);

                    var player = this.players[actionIndex];
                    TurnAction action;
                    try
                    {
                        // Player takes their action
                        action = player.Implementation.DoAction(player.Cards, tableState);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(LogLevel.Error, "Exception during {0}'s turn. Exception: {1}", player.Name, e);
                        action = TurnAction.Fold;
                    }

                    player.ResolveAction(action, ref called, ref folded, ref bet, ref pot);

                    actions.Add(new RoundAction(player.Name, action));
                    actionIndex = this.NextPlayer(actionIndex);
                }

                step++;
            }

            // Check out the result

            return new RoundState(pot, winners, revealedCards, actions);
        }

        /// <summary>
        /// Gets the next player who has not dropped or folded
        /// </summary>
        /// <param name="index">Player where action is at currently</param>
        /// <returns>The next non-folded or dropped player</returns>
        private int NextPlayer(int index)
        {
            index = (index + 1) % this.players.Count;
            while (this.players[index].Dropped || this.players[index].Folded) index = (index + 1) % this.players.Count;
            return index;
        }
    }
}
