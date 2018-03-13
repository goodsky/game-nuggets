using Common;
using System;
using UnityEngine;

/// <summary>
/// Bootstrapping behavior for the game.
/// </summary>
public class Game : Singleton<Game>
{
    protected void Start()
    {
        InitializeLogging();

        GameLogger.Info("Game started at {0}", DateTime.Now);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        GameLogger.Info("Game exiting at {0}", DateTime.Now);
        GameLogger.Close();
    }

    /// <summary>
    /// Set up the global static logger.
    /// </summary>
    private void InitializeLogging()
    {
        if (Application.isEditor)
        {
            GameLogger.CreateUnityLogger(LogLevel.All);
        }

        GameLogger.CreateMyDocumentsStream("debug", LogLevel.All);
    }
}
