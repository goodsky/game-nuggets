using Common;
using System;
using UI;
using UnityEngine;

/// <summary>
/// Main entry point for the game state and data initialization.
/// </summary>
public class Game : MonoBehaviour
{
    /// <summary>Singleton reference to the root Game object.</summary>
    public static Game Instance { get; private set; }

    [Header("UI Configuration")]
    public TextAsset UIConfig;

    /// <summary>The User Interface Manager</summary>
    public UIManager UI { get; private set; }

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected void Awake()
    {
        InitLogging();
        GameLogger.Info("Game started at {0}.", DateTime.Now);

        GameLogger.Info("Creating game objects.");
        InitGameObjects();
    }

    /// <summary>
    /// Cleanup game state and data.
    /// </summary>
    protected void OnDestroy()
    {
        GameLogger.Info("Game exiting at {0}", DateTime.Now);
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
        UIFactory.LoadUIEventSystem(gameObject);

        var canvas = UIFactory.LoadUICanvas(gameObject);
        canvas.SetActive(false);

        UI = canvas.AddComponent<UIManager>();
        UI.SetConfig(UIConfig);

        TooltipManager.Initialize(canvas.gameObject.transform);

        canvas.SetActive(true);
    }
}
