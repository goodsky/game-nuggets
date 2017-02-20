using System;

namespace RoboPoker.SamplePlayers
{
    class RandomPlayer : IRoboPoker
    {
        /// <summary>
        /// This player's name
        /// </summary>
        private string myName;

        /// <summary>
        /// Random class
        /// </summary>
        private Random random;

        /// <summary>
        /// Gets the name of this implementation
        /// </summary>
        public string ImplementationName { get { return "RandomPlayer"; } }

        /// <summary>
        /// Start this instance
        /// </summary>
        /// <param name="playerName">The name of this player</param>
        public void SetUp(string playerName)
        {
            this.myName = playerName;
            this.random = new Random();
        }

        /// <summary>
        /// Do something random. Who knows? I sure don't.
        /// </summary>
        /// <param name="hand">My hand.</param>
        /// <param name="state">The table state.</param>
        /// <returns>A random action.</returns>
        public TurnAction DoAction(Card[] hand, TableState state)
        {
            int randomValue = this.random.Next(1, 101);

            if (randomValue > 90)
                return TurnAction.Fold;

            if (randomValue > 50)
                return TurnAction.Call;

            int myMoney = state.Players[this.myName].Money;

            int raiseAmount = (randomValue / 10 + 1) * state.MinimumBet;
            if (raiseAmount > myMoney)
            {
                raiseAmount = myMoney;
            }

            return TurnAction.Raise(raiseAmount);
        }
    }
}
