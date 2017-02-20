namespace RoboPoker.SamplePlayers
{
    class FoldingPlayer : IRoboPoker
    {
        /// <summary>
        /// This player's name
        /// </summary>
        private string myName;

        /// <summary>
        /// Gets the name of this implementation
        /// </summary>
        public string ImplementationName { get { return "FoldingPlayer"; } }

        /// <summary>
        /// Start this instance
        /// </summary>
        /// <param name="playerName">The name of this player</param>
        public void SetUp(string playerName)
        {
            this.myName = playerName;
        }

        /// <summary>
        /// Fold.
        /// </summary>
        /// <param name="hand">My hand.</param>
        /// <param name="state">The state of the table.</param>
        /// <returns>Folding.</returns>
        public TurnAction DoAction(Card[] hand, TableState state)
        {
            return TurnAction.Fold;
        }
    }
}
