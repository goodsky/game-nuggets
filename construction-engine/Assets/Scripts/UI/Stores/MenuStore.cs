using Common;
using GameData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// In-Memory database for Menu GameObjects loaded from Menu.xml.
    /// </summary>
    public static class MenuStore
    {
        private static MenuData Data = null;

        private static Dictionary<string, GameObject> ButtonGroups = new Dictionary<string, GameObject>();

        /// <summary>
        /// Constructs the store objects based on the Menu GameData.
        /// Will quit the application on a failure. (non-recoverable failure)
        /// </summary>
        /// <param name="configuration">Name of the Unity TextAsset to load from.</param>
        public static void Instantiate(string configuration)
        {
            try
            {
                Data = GameDataSerializer.Load<MenuData>("Menu");
            }
            catch (Exception ex)
            {
                GameLogger.Log(LogLevel.Error, "Failed to load Menu GameData. Ex = {0}", ex);
                Application.Quit();
            }

            // Create ButtonGroups
            if (Data.ButtonGroups != null)
            {
                foreach (var buttonGroup in Data.ButtonGroups)
                {
                    // where to get the colors from?                
                }
            }
        }

        /// <summary>
        /// Link actions that reference other GameObjects.
        /// All other GameObjects in dependent stores must already be Instantiated.
        /// </summary>
        public static void Link()
        {

        }

        /// <summary>
        /// Returns whether or not this store has been initialized.
        /// </summary>
        public static bool IsInitialized()
        {
            return Data != null;
        }

        public static List<GameData.ButtonGroup> GetButtonGroups()
        {
            return Data.ButtonGroups;
        }
    }
}
