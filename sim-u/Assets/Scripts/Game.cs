using Campus;
using Common;
using Faculty;
using GameData;
using Simulation;
using UI;
using UnityEngine;

/// <summary>
/// Main entry point for the game state and data initialization.
/// </summary>
public class Game : MonoBehaviour
{
    [Header("Game Configuration")]
    public TextAsset UIConfig;
    public TextAsset CampusConfig;
    public TextAsset FacultyConfig;    public TextAsset SimulationConfig;

    [Space(10)]
    [Header("DebuggingFlags")]
    public bool AdminMode;
    public bool VisualizeConnections;

    [Header("If no save game is specified use this")]
    public string DefaultSaveGame;

    private static object _singletonLock = new object();
    private static Game _singleton = null;

    /// <summary>
    /// This static string will survive scene transitions.
    /// Set it before loading the game scene to request a particular save game to be loaded.
    /// </summary>
    public static string SavedGameName { get; set; } = null;
    private GameSaveState _saveState = null;

    /// <summary>
    /// Save the game state.
    /// </summary>
    /// <returns>The saved game state.</returns>
    public GameSaveState SaveGame()
    {
        var accessor = new GameAccessor();
        return new GameSaveState
        {
            Version = GameSaveState.CurrentVersion,
            Campus = accessor.CampusManager.SaveGameState(),
            Faculty = accessor.Faculty.SaveGameState(),
            Simulation = accessor.Simulation.SaveGameState(),
        };
    }

    /// <summary>
    /// Load the saved game if one exists.
    /// Uses <see cref="SavedGameName"/> to select the game file.
    /// </summary>
    /// <param name="saveState">The saved game state if it could be loaded.</param>
    /// <returns>True if the saved game exists and was loaded. False otherwise.</returns>
    public bool TryLoadSavedGame(out GameSaveState saveState)
    {
        if (_saveState != null)
        {
            saveState = _saveState;
            return true;
        }

        string saveGameName = Game.SavedGameName;
        if (!string.IsNullOrEmpty(saveGameName) &&
            SavedGameLoader.TryReadFromDisk(saveGameName, out GameSaveState loadedSaveState))
        {
            _saveState = saveState = loadedSaveState;
            return true;
        }

        saveState = null;
        return false;
    }

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected void Awake()
    {
        GameLogger.EnsureSingletonExists();
        GameLogger.Info("Game started.");

        lock (_singletonLock)
        {
            if (_singleton != null)
            {
                GameLogger.FatalError("It appears there are multiple root Game objects.");
            }

            _singleton = this;
        }

        if (string.IsNullOrEmpty(SavedGameName) && !string.IsNullOrEmpty(DefaultSaveGame))
        {
            GameLogger.Info("Using default save game '{0}'", DefaultSaveGame);
            SavedGameName = DefaultSaveGame;
        }

        GameLogger.Info("Creating game objects.");
        InitGameObjects();
    }

    /// <summary>
    /// Cleanup game state and data.
    /// </summary>
    protected void OnDestroy()
    {
        GameLogger.Info("Game exiting.");

        lock (_singletonLock)
        {
            _singleton = null;
        }
    }

    /// <summary>
    /// Initialize the game objects.
    /// </summary>
    private void InitGameObjects()
    {
        UIFactory.LoadEventSystem(gameObject);

        gameObject.AddComponent<GameDataStore>();
        gameObject.AddComponent<GameStateMachine>();

        GameObject ui = UIFactory.LoadUICanvas(gameObject);
        GameDataLoader<UIData>.SetGameData<UIManager>(ui, UIConfig);

        GameObject campus = UIFactory.GenerateEmpty("Campus", transform);
        GameDataLoader<CampusData>.SetGameData<CampusManager>(campus, CampusConfig);

        GameObject faculty = UIFactory.GenerateEmpty("Faculty", transform);
        GameDataLoader<FacultyData>.SetGameData<FacultyManager>(faculty, FacultyConfig);

        GameObject simulation = UIFactory.GenerateEmpty("Simulation", transform);
        GameDataLoader<SimulationData>.SetGameData<SimulationManager>(simulation, SimulationConfig);

        TooltipManager.Initialize(ui.gameObject.transform);
    }
}
