using Common;
using System;
using UI;

namespace GameData
{
    /// <summary>
    /// Game data store locations.
    /// </summary>
    public enum GameDataType
    {
        ButtonGroup,
        Window,
        Building,
    }

    /// <summary>
    /// Global router of all Game Data and where it should be stored.
    /// Use it to get references to the runtime instances of game data.
    /// </summary>
    public class GameDataStore
    {
        /// <summary>
        /// Gets data from a well-known location.
        /// </summary>
        /// <param name="type">The data type.</param>
        /// <param name="dataName">Name of the data to load.</param>
        /// <returns>The game data, or null if not found.</returns>
        public object Get(GameDataType type, string dataName)
        {
            if (string.IsNullOrEmpty(dataName))
                throw new ArgumentNullException("dataName");

            object data = null;

            switch (type)
            {
                case GameDataType.ButtonGroup:
                    ButtonGroup buttonGroup;
                    if (Game.UI.TryGetButtonGroup(dataName, out buttonGroup))
                    {
                        data = buttonGroup;
                    }
                    break;

                case GameDataType.Window:
                    Window window;
                    if (Game.UI.TryGetWindow(dataName, out window))
                    {
                        data = window;
                    }
                    break;

                case GameDataType.Building:
                    BuildingData buildingData;
                    if (Game.Campus.TryGetBuildingData(dataName, out buildingData))
                    {
                        data = buildingData;
                    }
                    break;
            }

            return data;
        }

        /// <summary>
        /// Get data from a store and casts the type for you.
        /// This assumes you know what the type is.
        /// </summary>
        /// <typeparam name="T">The type to cast the data to.</typeparam>
        /// <param name="type">The game data type.</param>
        /// <param name="dataName">Name of the data to load.</param>
        /// <returns>The game data, or null if not found.</returns>
        public T Get<T>(GameDataType type, string dataName) where T : class
        {
            object data = Get(type, dataName);

            T dataCasted = data as T;
            if (dataCasted != null)
            {
                return dataCasted;
            }
            else if (data != null)
            {
                GameLogger.Error("Found game data '{0}' in {1} but could not load as type '{2}'", dataName, type.ToString(), typeof(T).Name);
            }
            else
            {
                GameLogger.Error("Failed to load game data from store. Store = '{0}'. Data = '{1}'.", type.ToString(), dataName);
            }

            return default(T);
        }
    }
}
