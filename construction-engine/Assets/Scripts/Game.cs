using Common;
using GameData;
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
    public TextAsset UIConfiguration;

    /// <summary>Link to the game toolbar.</summary>
    public Toolbar Toolbar { get; private set; }

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected void Awake()
    {
        InitializeLogging();
        GameLogger.Info("Game started at {0}.", DateTime.Now);

        GameLogger.Info("Creating game objects.");
        InitializeGameObjects();

        GameLogger.Info("Loading game data.");
        InitializeStores();
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
    private void InitializeLogging()
    {
        if (Application.isEditor)
        {
            GameLogger.CreateUnityLogger(LogLevel.All);
        }

        GameLogger.CreateMyDocumentsStream("debug", LogLevel.All);
    }

    /// <summary>
    /// Initialize the game objects.
    /// </summary>
    private void InitializeGameObjects()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameLogger.Error("Scene requires a Canvas to load UI.");
            Application.Quit();
        }

        Toolbar = canvas.gameObject.AddComponent<Toolbar>();
    }

    /// <summary>
    /// Initialize the game object stores.
    /// </summary>
    private void InitializeStores()
    {
        if (Toolbar == null)
        {
            GameLogger.Warning("No toolbar found. Skipping initialization.");
        }
        else
        {
            UIData data = null;

            try
            {
                data = GameDataSerializer.Load<UIData>(UIConfiguration);

                if (data == null)
                    throw new ArgumentNullException("Toolbar GameData was null.");
            }
            catch (Exception e)
            {
                GameLogger.Error("Failed to load toolbar game data. Ex = {0}", e);
                Application.Quit();
            }

            Toolbar.InitializeStore(data);
        }

        if (Toolbar != null)
        {
            Toolbar.LinkStore();
        }
    }
}
