namespace GameData
{
    /// <summary>
    /// Interface used by classes that save and load game save state.
    /// </summary>
    /// <typeparam name="T">The game save state data type.</typeparam>
    public interface IGameStateSaver<T>
    {
        T SaveGameState();

        void LoadGameState(T state);
    }
}
