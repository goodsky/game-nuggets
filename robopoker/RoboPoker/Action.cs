namespace RoboPoker
{
    /// <summary>
    /// Three possible action types
    /// </summary>
    public enum ActionType
    {
        // Call the current bet
        Call,

        // Raise the current bet the value indicated in the Action
        Raise,

        // Drop out of the current round
        Fold
    }

    /// <summary>
    /// This class represents a single action made by a player.
    /// You may call, raise or fold with a single action.
    /// If you return a ActionType.Raise then the 'raise' argument must be set, otherwise the value of 'raise' will be ignored.
    /// </summary>
    public class TurnAction
    {
        /// <summary>
        /// Global call action
        /// </summary>
        public static TurnAction Call = new TurnAction(ActionType.Call);

        /// <summary>
        /// Global fold action
        /// </summary>
        public static TurnAction Fold = new TurnAction(ActionType.Fold);

        /// <summary>
        /// Helper raise action method
        /// </summary>
        /// <param name="raise">The amount to raise. Must be greater than or equal to the big blind.</param>
        /// <returns>A Raise action to return the raise amount</returns>
        public static TurnAction Raise(int raise)
        {
            return new TurnAction(ActionType.Raise, raise);
        }

        /// <summary>
        /// The type of action
        /// </summary>
        public ActionType Type { get; private set; }

        /// <summary>
        /// The value of a raise
        /// </summary>
        public int RaiseAmount { get; private set; }

        /// <summary>
        /// Create an action for a player's turn.
        /// </summary>
        /// <param name="type">The type of action. Call, Raise or Fold</param>
        /// <param name="raise">The amount to raise by when a Raise action is indicated</param>
        public TurnAction(ActionType type, int raise = 0)
        {
            this.Type = type;
            this.RaiseAmount = raise;
        }
    }
}
