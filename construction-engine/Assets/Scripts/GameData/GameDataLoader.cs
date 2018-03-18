using Common;
using System;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// Base class for Unity behaviours that will load game data from configuration.
    /// Uses the Unity Awake() method to build data. Then Unity Start() to link.
    /// </summary>
    public abstract class GameDataLoader<T> : MonoBehaviour where T : class
    {
        /// <summary>Link to the GameData configuration asset.</summary>
        protected TextAsset Config;

        /// <summary>Parsed and loaded GameData.</summary>
        protected T GameData;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected virtual void Awake()
        {
            try
            {
                GameData = GameDataSerializer.Load<T>(Config);
            }
            catch (Exception e)
            {
                GameLogger.FatalError("Failed to load toolbar game data. Ex = {0}", e);
            }

            LoadData(GameData);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected virtual void Start()
        {
            LinkData(GameData);
        }

        /// <summary>
        /// You must call this method on a GameDataLoader before it starts!
        /// The GameObject must be disabled while doing this step otherwise
        /// the Awake and Start methods will not be called correctly.
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(TextAsset config)
        {
            if (gameObject.activeSelf)
            {
                GameLogger.FatalError("Setting configuration on an active GameObject!");
            }

            Config = config;
        }

        /// <summary>
        /// Load the runtime instances from GameData.
        /// </summary>
        /// <param name="gameData">The game data to load.</param>
        protected abstract void LoadData(T gameData);

        /// <summary>
        /// Link references between runtime instances of GameData.
        /// e.g. the OnSelect action from a button referencing a window.
        /// </summary>
        /// <param name="gameData">The game data to link.</param>
        protected abstract void LinkData(T gameData);
    }
}
