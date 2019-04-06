using Campus;
using Common;
using GameData;
using UI;
using UnityEngine;

/// <summary>
/// Main entry point for the game state and data initialization.
/// </summary>
public class Game : MonoBehaviour
{
    [Header("UI Configuration")]
    public TextAsset UIConfig;

    [Header("Campus Configuration")]
    public TextAsset CampusConfig;

    /// <summary>The global game state machine</summary>
    public static GameStateMachine State { get; private set; }

    /// <summary>The User Interface Manager</summary>
    public static UIManager UI { get; private set; }

    /// <summary>The Campus Manager</summary>
    public static CampusManager Campus { get; private set; }

    private static object _singletonLock = new object();
    private static Game _singleton = null;

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected void Awake()
    {
        InitLogging();
        GameLogger.Info("Game started.");

        lock (_singletonLock)
        {
            if (_singleton != null)
            {
                GameLogger.FatalError("It appears there are multiple root Game objects.");
            }

            _singleton = this;
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
        GameLogger.Close();
    }

    /// <summary>
    /// Set up the game session loggers.
    /// </summary>
    private void InitLogging()
    {
        if (Application.isEditor)
        {
            GameLogger.CreateUnityLogger(LogLevel.Info);
        }

        GameLogger.CreateMyDocumentsStream("debug", LogLevel.Info);
    }

    /// <summary>
    /// Initialize the game objects.
    /// </summary>
    private void InitGameObjects()
    {
        UIFactory.LoadEventSystem(gameObject);

        State = gameObject.AddComponent<GameStateMachine>();

        var ui = UIFactory.LoadUICanvas(gameObject);
        UI = GameDataLoader<UIData>.SetGameData<UIManager>(ui, UIConfig);

        var campus = UIFactory.GenerateEmpty("Campus", transform);
        Campus = GameDataLoader<CampusData>.SetGameData<CampusManager>(campus, CampusConfig);

        TooltipManager.Initialize(ui.gameObject.transform);
    }
}
