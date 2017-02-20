namespace RoboPoker.SamplePlayers
{
    class RaisingPlayer : IRoboPoker
    {
        /// <summary>
        /// This player's name
        /// </summary>
        private string myName;

        /// <summary>
        /// Gets the name of this implementation
        /// </summary>
        public string ImplementationName { get { return "RaisingPlayer"; } }

        /// <summary>
        /// Start this instance
        /// </summary>
        /// <param name="playerName">The name of this player</param>
        public void SetUp(string playerName)
        {
            this.myName = playerName;
        }

        /// <summary>
        /// Raise by the minimum bet.
        /// </summary>
        /// <param name="hand">My hand.</param>
        /// <param name="state">The table state.</param>
        /// <returns>A raise by the minimum bet.</returns>
        public TurnAction DoAction(Card[] hand, TableState state)
        {
            return TurnAction.Raise(state.MinimumBet);
        }
    }
}
