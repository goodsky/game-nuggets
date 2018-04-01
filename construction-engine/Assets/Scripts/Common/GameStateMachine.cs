using GridTerrain;

namespace Common
{
    /// <summary>
    /// Enumeration of possible top-level game states.
    /// </summary>
    public enum GameState
    {
        /// <summary>The default game state. You are selecting your next state.</summary>
        Selecting,

        /// <summary>Constructing a new entity on the campus.</summary>
        Constructing,

        /// <summary>Modifying the campus terrain.</summary>
        EditingTerrain
    }

    /// <summary>
    /// Storage for the global game state.
    /// </summary>
    public static class GameStateMachine
    {
        public static GameState Current { get; private set; }

        public static void SetState(GameState next)
        {
            GameState old = Current;

            if (Current == next)
            {
                return;
            }

            // there could be a lot more logic here
            // but I haven't decided how much is helpful
            Current = next;

            switch (old)
            {
                case GameState.EditingTerrain:
                    EditableTerrain.Singleton.StopEditing();
                    break;
            }

            switch (next)
            {
                case GameState.EditingTerrain:
                    EditableTerrain.Singleton.StartEditing();
                    break;
            }
        }
    }
}
