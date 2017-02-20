using RoboPoker;
using System;

namespace TableRunner
{
    /// <summary>
    /// Simulates the state of a player around the table
    /// </summary>
    internal class Player
    {
        /// <summary>
        /// Name of the player
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Implementation for the RoboPoker player
        /// </summary>
        public IRoboPoker Implementation { get; private set; }

        /// <summary>
        /// Money in front of this player, minus their bet
        /// </summary>
        public int Money { get; set; }

        /// <summary>
        /// Money this player has bet
        /// </summary>
        public int Bet { get; private set; }

        /// <summary>
        /// Cards this player has in their hand
        /// </summary>
        public Card[] Cards { get; private set; }

        /// <summary>
        /// Indicates whether this player has folded or not yet
        /// </summary>
        public bool Folded { get; private set; }

        /// <summary>
        /// Indicates whether this player is AllIn
        /// </summary>
        public bool AllIn { get; private set; }

        /// <summary>
        /// Indicates whether this player has dropped out of the game
        /// This will happen at the start of a round when a player has less money than the big blind
        /// </summary>
        public bool Dropped { get; private set; }

        /// <summary>
        /// Gets the player state for this player
        /// </summary>
        public PlayerState State
        {
            get
            {
                return new PlayerState(this.Name, this.Money, this.Bet, this.Cards, this.Folded, this.AllIn, this.Dropped);
            }
        }

        /// <summary>
        /// Create a player
        /// </summary>
        /// <param name="name">Name of the player AI</param>
        /// <param name="implementation">Implementation of the RoboPoker player</param>
        public Player(string name, IRoboPoker implementation)
        {
            this.Name = name;
            this.Implementation = implementation;

            this.Money = 0;
            this.Bet = 0;
            this.Folded = false;
        }

        /// <summary>
        /// Reset values at the start of a new round
        /// </summary>
        public void ResetRound()
        {
            this.AllIn = false;
            this.Folded = false;
            this.Bet = 0;
        }

        /// <summary>
        /// Start the round and give the player their cards
        /// </summary>
        /// <param name="cards">The player's hand</param>
        public void StartRound(Card[] cards)
        {
            this.Cards = cards;
        }

        /// <summary>
        /// When you need to bet a blind value
        /// </summary>
        /// <param name="blindValue">The blinds value you must pay</param>
        public void PostBlind(int blindValue)
        {
            this.Bet = blindValue;
            this.Money -= blindValue;
        }

        /// <summary>
        /// Handle this player's action
        /// </summary>
        /// <param name="action">The action this player is trying to do</param>
        /// <param name="called">The number of players called currently on the table.</param>
        /// <param name="folded">The number of players folded on the table.</param>
        /// <param name="bet">The actual bet from the table. This makes sure the bet will be raised whenever we raise.</param>
        /// <param name="pot">The pot on the table.</param>
        public void ResolveAction(TurnAction action, ref int called, ref int folded, ref int bet, ref int pot)
        {
            switch (action.Type)
            {
                case ActionType.Call:
                    if (this.Money + this.Bet - bet < 0)
                    {
                        Logger.Log(LogLevel.Info, "Player {0} tried to call, but they can't afford to. Player {0} goes all in with ${1}.", this.Name, this.Bet + this.Money);
                        this.Bet += this.Money;
                        this.Money = 0;
                        this.GoAllIn();
                        ++folded; // I count going all in as the same folding, since you get no more actions until next round (if you're lucky)
                        return;
                    }

                    Logger.Log(LogLevel.Info, "Player {0} called the bet of ${1}.", this.Name, bet);
                    this.Money -= (bet - this.Bet);
                    this.Bet = bet;
                    ++called;
                    break;

                case ActionType.Raise:
                    if (this.Money + this.Bet - action.RaiseAmount - bet < 0)
                    {
                        Logger.Log(LogLevel.Info, "Player {0} tried to raise ${1}, but they can't afford to. Player {0} goes all in with ${2}.", this.Name, action.RaiseAmount, this.Bet + this.Money);
                        this.Bet += this.Money;
                        this.Money = 0;
                        this.GoAllIn();
                        ++folded; // I count going all in as the same folding, since you get no more actions until next round (if you're lucky)

                        bet = Math.Max(bet, this.Bet);
                        break;
                    }

                    int newBet = bet + action.RaiseAmount;
                    Logger.Log(LogLevel.Info, "Player {0} raised the bet by ${1}. New bet is at ${2}.", this.Name, action.RaiseAmount, newBet);
                    this.Money -= (newBet - this.Bet);
                    this.Bet = newBet;
                    called = 1;

                    bet = newBet;
                    break;

                case ActionType.Fold:
                    Logger.Log(LogLevel.Info, "Player {0} folds.", this.Name);
                    this.Fold();
                    ++folded;
                    break;
            }
        }

        /// <summary>
        /// Make this player fold
        /// </summary>
        public void Fold()
        {
            this.Folded = true;
        }

        /// <summary>
        /// Make this player all in
        /// </summary>
        public void GoAllIn()
        {
            this.AllIn = true;
        }

        /// <summary>
        /// Make this player drop
        /// </summary>
        public void Drop()
        {
            this.Dropped = true;
            this.Bet = 0;
        }

        /// <summary>
        /// Recieve winnings from a round!
        /// </summary>
        /// <param name="amountWon">The amount this player won in the round (usually 0...)</param>
        public void AwardWinnings(int amountWon)
        {
            this.Money += amountWon;
        }
    }
}
