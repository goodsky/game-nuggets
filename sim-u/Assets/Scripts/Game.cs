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

    /// <summary>
    /// This string will survive scene transitions.
    /// Set it before loading the game scene to request a particular save game to be loaded.
    /// </summary>
    public static string SavedGameName { get; set; } = null;

    private static object _singletonLock = new object();
    private static Game _singleton = null;

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
        
        TooltipManager.Initialize(ui.gameObject.transform);
    }
}
