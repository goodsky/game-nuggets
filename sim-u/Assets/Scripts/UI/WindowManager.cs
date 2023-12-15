﻿using Common;
using GameData;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Class to manage active windows. This was pulled out of the UIManager because of anticipated feature creep here.
    /// Some day... we may want fancy lerp animations to pop windows in.
    /// Today is not that day.
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        private GameAccessor _accessor = new GameAccessor();
        private Dictionary<string, Window> _windows = new Dictionary<string, Window>();

        /// <summary>The active window.</summary>
        public Window CurrentOpenWindow { get; private set; }

        /// <summary>
        /// Try to load a window from the store.
        /// </summary>
        /// <param name="name">Name of the window</param>
        /// <param name="window">The window.</param>
        /// <returns>True if the window exists, false otherwise.</returns>
        public bool TryGetWindow(string name, out Window window)
        {
            return _windows.TryGetValue(name, out window);
        }

        /// <summary>
        /// Load the window game objects from the UI game data.
        /// </summary>
        /// <param name="data">The UI game data.</param>
        public void LoadData(UIData data)
        {
            foreach (var window in data.Windows)
            {
                var windowObject = UIFactory.LoadWindow(window.PrefabName, window.Name, window.FullScreen, transform, data.Config);
                _windows[window.Name] = windowObject.GetComponent<Window>();
            }
        }

        /// <summary>
        /// Opens a window on screen using GameDataStore data.
        /// </summary>
        /// <param name="name">Name of the window to open.</param>
        /// <param name="type">The type of game data to pass to the window.</param>
        /// <param name="dataName">Name of the data to pass to the window.</param>
        public void OpenWindow(string name, GameDataType type, string dataName)
        {
            object data = _accessor.GameData.Get(type, dataName);
            OpenWindow(name, data);
        }

        /// <summary>
        /// Opens a window on the screen with custom (possibly null) data.
        /// </summary>
        /// <param name="name">Name of the window to open.</param>
        /// <param name="data">The data to pass to the window.</param>
        public void OpenWindow(string name, object data)
        {
            Window window = null;
            if (!_windows.TryGetValue(name, out window))
            {
                GameLogger.FatalError("Attempted to open non-existant window '{0}'", name);
                return;
            }

            if (window == CurrentOpenWindow)
            {
                // If you open the same window again, I assume you want to close it.
                CloseWindow();
                return;
            }

            CloseWindow();

            var selected = SelectionManager.Selected;
            if (selected != null)
            {
                window.SelectionParent = selected;
            }

            CurrentOpenWindow = window;

            window.Open(data);
            window.gameObject.SetActive(true);
        }

        /// <summary>
        /// Close all windows on screen.
        /// </summary>
        public void CloseWindow()
        {
            if (CurrentOpenWindow != null)
            {
                CurrentOpenWindow.Close();
                CurrentOpenWindow.gameObject.SetActive(false);
                CurrentOpenWindow = null;
            }
        }
    }
}
