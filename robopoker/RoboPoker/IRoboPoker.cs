namespace RoboPoker
{
    /// <summary>
    /// Main interface to implement a RoboPoker AI
    /// </summary>
    public interface IRoboPoker
    {
        /// <summary>
        /// Gets a unique name to identify the AI implementation.
        /// Used to track performance of different AIs.
        /// </summary>
        string ImplementationName { get; }

        /// <summary>
        /// This method is guaranteed to be called before any actions are processed.
        /// Your player AI will be assigned a name from a configuration file
        /// </summary>
        /// <param name="playerName"></param>
        void SetUp(string playerName);

        /// <summary>
        /// Take the action for your turn.
        /// </summary>
        /// <param name="hand">The two cards in your hand</param>
        /// <param name="state">The context of the table state. This class should contain all information needed to make your turn decision.</param>
        /// <returns>The result of your turn; either a Call, Raise or Fold action.</returns>
        TurnAction DoAction(Card[] hand, TableState state);
    }
}
