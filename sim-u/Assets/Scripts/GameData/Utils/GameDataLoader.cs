using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// Base class for Unity behaviours that will load game data from configuration.
    /// Uses the Unity Awake() method to build data. Then Unity Start() to link.
    /// </summary>
    public abstract class GameDataLoader<T> : MonoBehaviour where T : class
    {
        /// <summary>
        /// Add a <see cref="GameDataLoader{T}"/> instance to the game object.
        /// This helper is required to ensure the data is set safely.
        /// </summary>
        /// <typeparam name="U">Type of the GameDataLoader.</typeparam>
        /// <param name="gameObject">The game object to add the data loader to.</param>
        /// <param name="config">The configuration object to parse into a GameData object.</param>
        /// <returns>The GameDataLoader.</returns>
        public static U SetGameData<U>(GameObject gameObject, TextAsset config) where U : GameDataLoader<T>
        {
            gameObject.SetActive(false);
            U dataLoader = gameObject.AddComponent<U>();
            dataLoader.SetConfig(config);
            gameObject.SetActive(true);
            return dataLoader;
        }

        /// <summary>Game manager accessor. Fake DI pipeline.</summary>
        protected GameAccessor Accessor = new GameAccessor();

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
                GameLogger.FatalError("Failed to load game data for {0}. Ex = {1}", GetType().Name, e);
                return;
            }

            if (GameData == null)
            {
                GameLogger.FatalError("Null game data for {0}.", GetType().Name);
                return;
            }

            LoadDataInternal(GameData);
            LoadData(GameData);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected virtual void Start()
        {
            if (GameData != null)
            {
                LinkData(GameData);
            }
        }

        /// <summary>
        /// You must call this method on a GameDataLoader before it starts!
        /// The GameObject must be disabled while doing this step otherwise
        /// the Awake and Start methods will not be called correctly.
        /// </summary>
        /// <param name="config"></param>
        protected void SetConfig(TextAsset config)
        {
            if (gameObject.activeSelf)
            {
                GameLogger.FatalError("Setting game data configuration on an active GameObject!");
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

        /// <summary>
        /// Use reflection to load resources that have the resource attributes.
        /// </summary>
        /// <param name="gameData">The game data to load data on.</param>
        private void LoadDataInternal(object gameData)
        {
            PropertyInfo[] properties = gameData.GetType().GetProperties();
            Dictionary<string, PropertyInfo> propertiesDictionary = properties.ToDictionary(info => info.Name);
            foreach (var property in properties)
            {
                var colorPalette = property.GetCustomAttribute<ColorPaletteAttribute>();
                var resourceLoader = property.GetCustomAttribute<ResourceLoaderAttribute>();
                var saveGameLoader = property.GetCustomAttribute<SavedGameLoaderAttribute>();

                // Case 1) The property has a ColorPaletteAttribute. Load the Color.
                if (colorPalette != null)
                {
                    string colorName = string.Empty;
                    if (!propertiesDictionary.TryGetValue(colorPalette.PropertyName, out PropertyInfo colorNameProperty))
                    {
                        GameLogger.FatalError("Could not link ColorPaletteAttribute property name '{0}' to a PropertyInfo.", colorPalette.PropertyName);
                        continue;
                    }
                    else
                    {
                        colorName = colorNameProperty.GetValue(gameData)?.ToString();
                        if (string.IsNullOrEmpty(colorName))
                        {
                            GameLogger.FatalError("Could not load a value from ColorPaletteAttribute property '{0}'.", colorPalette.PropertyName);
                            continue;
                        }
                    }

                    Color color = ColorPalette.GetColor(colorName);
                    property.SetValue(gameData, color);
                }
                // Case 2) The property has a ResourceLoaderAttribute. Load the Resource.
                else if (resourceLoader != null)
                {
                    string resourceName = resourceLoader.ResourceName;
                    if (string.IsNullOrEmpty(resourceName))
                    {
                        if (!propertiesDictionary.TryGetValue(resourceLoader.PropertyName, out PropertyInfo resourceNameProperty))
                        {
                            GameLogger.FatalError("Could not link ResourceLoaderAttribute property name '{0}' to a PropertyInfo.", resourceLoader.PropertyName);
                            continue;
                        }
                        else
                        {
                            resourceName = resourceNameProperty.GetValue(gameData)?.ToString();
                            if (string.IsNullOrEmpty(resourceName))
                            {
                                GameLogger.FatalError("Could not load a value from ResourceLoaderAttribute property '{0}'.", resourceLoader.PropertyName);
                                continue;
                            }
                        }
                    }

                    UnityEngine.Object resource = ResourceLoader.Load(resourceLoader.Type, resourceLoader.Category, resourceName, property.PropertyType);
                    property.SetValue(gameData, resource);
                }
                // Case 3) The property has a SavedGameLoader. Try to load from the saved game state.
                else if (saveGameLoader != null)
                {
                    if (Accessor.Game.TryLoadSavedGame(out GameSaveState save))
                    {
                        property.SetValue(gameData, save);
                    }
                    else
                    {
                        GameLogger.Warning("Did not load any save game.");
                    }
                }
                // Case 4) The type is from the GameData namespace. Recursively attempt to load data.
                else if (property.GetValue(gameData) != null && 
                    property.PropertyType.FullName.StartsWith("GameData"))
                {
                    object propertyValue = property.GetValue(gameData);
                    LoadDataInternal(propertyValue);
                }
                // Case 5) The type is an IEnumerable. Unspool the objects and recursively load data.
                else if (property.GetValue(gameData) != null &&
                    property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    foreach (object item in (IEnumerable)property.GetValue(gameData, null))
                    {
                        if (item.GetType().FullName.StartsWith("GameData"))
                        {
                            LoadDataInternal(item);
                        }
                    }
                }
            }
        }
    }
}
