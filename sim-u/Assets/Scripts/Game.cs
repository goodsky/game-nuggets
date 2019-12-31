using Campus;
using Common;
using Faculty;
using GameData;
using Simulation;
using System.IO;
using UI;
using UnityEngine;

/// <summary>
/// Main entry point for the game state and data initialization.
/// </summary>
public class Game : MonoBehaviour
{
    private static readonly string ConfigFolderName = "GameData";
    private static readonly string ConfigFileExtension = ".xml";

    [Header("Game Configuration")]
    public string UIConfig;
    public string CampusConfig;
    public string FacultyConfig;
    public string SimulationConfig;

    [Space(10)]
    [Header("DebuggingFlags")]
    public bool AdminMode;
    public bool VisualizeConnections;

    [Header("If no save game is specified use this")]
    public string DefaultSaveGame;

    private static object _singletonLock = new object();
    private static Game _singleton = null;

    /// <summary>
    /// This static metadata will survive scene transitions.
    /// Set it before loading the game scene to request a particular save file to be loaded.
    /// </summary>
    private static GameSaveStateMetadata _saveStateMetadata { get; set; } = null;
    private GameSaveState _saveState = null;

    /// <summary>
    /// Sets the metadata for the save game that will be used to populate the game
    /// when the main scene is loaded. Only takes effect if set before the 
    /// <see cref="InitGameObjects"/> call.
    /// </summary>
    /// <param name="path">The save game path on disk (within the StreamingAssets/ directory)</param>
    public static void SetGameSaveStateForReload(string path)
    {
        _saveStateMetadata = new GameSaveStateMetadata(path);
    }

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
    /// Uses <see cref="GameSaveStateMetadata"/> to select the game file.
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

        GameSaveStateMetadata info = Game._saveStateMetadata;
        if (info != null)
        {
            if (SavedGameLoader.TryReadFromDisk(info.Path, out saveState))
            {
                _saveState = saveState;
                return true;
            }
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

        if (_saveStateMetadata == null && !string.IsNullOrEmpty(DefaultSaveGame))
        {
            GameLogger.Info("Using default save game '{0}'", DefaultSaveGame);
            SetGameSaveStateForReload(DefaultSaveGame);
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
        GameDataLoader<UIData>.SetGameData<UIManager>(ui, GetConfigPath(UIConfig));

        GameObject campus = UIFactory.GenerateEmpty("Campus", transform);
        GameDataLoader<CampusData>.SetGameData<CampusManager>(campus, GetConfigPath(CampusConfig));

        GameObject faculty = UIFactory.GenerateEmpty("Faculty", transform);
        GameDataLoader<FacultyData>.SetGameData<FacultyManager>(faculty, GetConfigPath(FacultyConfig));

        GameObject simulation = UIFactory.GenerateEmpty("Simulation", transform);
        GameDataLoader<SimulationData>.SetGameData<SimulationManager>(simulation, GetConfigPath(SimulationConfig));

        TooltipManager.Initialize(ui.gameObject.transform);
    }

    private string GetConfigPath(string configName) => Path.Combine(Application.streamingAssetsPath, ConfigFolderName, configName + ConfigFileExtension);
}
