using Common;
using System;
using UI;
using UnityEngine;

public class Game : Singleton<Game>
{
    /// <summary>Main menu 'Default' color</summary>
    public Color MainMenuBackground = MyPalette.Gray.Darkest;

    /// <summary> Main menu 'Selected' color</summary>
    public Color MainMenuSelected = MyPalette.Gray.Darker;

    /// <summary>Sub menu 'Default' color</summary>
    public Color SubMenuBackground = MyPalette.Gray.Dark;

    /// <summary>Sub menu 'Selected' color</summary>
    public Color SubMenuSelected = MyPalette.Gray.Medium;

    /// <summary>Background Window color</summary>
    public Color PageBackground = MyPalette.Gray.Light;

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        InitializeLogging();

        GameLogger.Info("Game started at {0}", DateTime.Now);
    }

    /// <summary>
    /// Cleanpu game state and data.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

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
    /// Initialize the game object stores.
    /// </summary>
    private void InitializeStores()
    {
        MenuStore.Instantiate("Menu");
    }
}
