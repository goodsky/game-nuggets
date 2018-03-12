using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Load UI Prefabs.
    /// Makes minor adjustments to the prefabs as necessary before returning them.
    /// </summary>
    public static partial class ToolbarFactory
    {
        private static readonly Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

        /// <summary>
        /// Load all UI Prefabs in the static ctor.
        /// </summary>
        static ToolbarFactory()
        {
            var uiPrefabs = Resources.LoadAll<GameObject>("UI");

            foreach (var prefab in uiPrefabs)
            {
                Prefabs[prefab.name] = prefab;
            }
        }

        /// <summary>
        /// Instantiate a prefab from the Prefabs/Resources/UI directory.
        /// Sets the object name and parent.
        /// </summary>
        /// <param name="prefabName">Filename of the prefab</param>
        /// <param name="name">Name of the new instance</param>
        /// <param name="parent">Parent transform of the prefab</param>
        /// <returns></returns>
        public static GameObject InstantiatePrefab(string prefabName, string name, Transform parent)
        {
            GameObject prefab;
            if (!Prefabs.TryGetValue(prefabName, out prefab))
            {
                throw new InvalidOperationException(string.Format("Could not find prefab '{0}'!", prefabName));
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.name = name;

            return instance;
        }

        /// <summary>
        /// Instantiates the screen-wide object that is the root of UI selections. 
        /// This means if you click on the non-UI screen then the selection manager will reset.
        /// </summary>
        /// <param name="parent">The Canvas parent</param>
        /// <returns>The selection root.</returns>
        public static GameObject LoadSelectionRoot(GameObject parent)
        {
            string SelectionRoot = "SelectionRoot";

            var selectionRoot = InstantiatePrefab(SelectionRoot, SelectionRoot, parent.transform);
            selectionRoot.transform.SetAsFirstSibling();

            return selectionRoot;
        }

        /// <summary>
        /// Instantiates the top status bar.
        /// </summary>
        /// <param name="parent">The status bar parent.</param>
        /// <param name="background">The sprite background color.</param>
        /// <returns>The status bar.</returns>
        public static GameObject LoadStatusBar(GameObject parent, Sprite background)
        {
            string StatusBar = "StatusBar";

            var statusBar = InstantiatePrefab(StatusBar, StatusBar, parent.transform);

            var image = statusBar.GetComponent<Image>();
            image.sprite = background;

            // TODO: wire up the status bar with game state.
            var statusBarInfo = statusBar.GetComponent<StatusBar>();
            statusBarInfo.CurrentFunds = 123456;
            statusBarInfo.CurrentDate = string.Format("{0}\n{1}", DateTime.Now.ToString("MM/dd/yyyy"), "Spring");

            return statusBar;
        }

        /// <summary>
        /// Instantiates a bottom menu bar.
        /// </summary>
        /// <param name="parent">The menu parent.</param>
        /// <param name="background">The sprite background color.</param>
        /// <returns>The menu bar.</returns>
        public static GameObject LoadMenuBar(GameObject parent, string name, float yOffset, Sprite background)
        {
            string MenuBar = "MenuBar";

            var menuBar = InstantiatePrefab(MenuBar, name, parent.transform);

            var rect = menuBar.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, yOffset);

            var image = menuBar.GetComponent<Image>();
            image.sprite = background;

            return menuBar;
        }

        /// <summary>
        /// Instantiates a the small UI 'pip' that is used entirely for looking pretty.
        /// Stop and smell the roses sometimes.
        /// </summary>
        /// <param name="parent">The parent of the pip.</param>
        /// <param name="color">The pip color.</param>
        /// <returns>The toolbar accent pip.</returns>
        public static GameObject LoadPip(GameObject parent, Color color)
        {
            string ToolbarPip = "ToolbarPip";

            var pip = InstantiatePrefab(ToolbarPip, ToolbarPip, parent.transform);

            var image = pip.GetComponent<Image>();
            image.color = color;

            pip.SetActive(false);

            return pip;
        }

        /// <summary>
        /// Instantiates a tooltip textbox.
        /// It pops up over buttons to lend helpful pointers.
        /// </summary>
        /// <param name="parent">The tooltip parent transform.</param>
        /// <returns>The tooltip.</returns>
        public static GameObject LoadTooltip(Transform parent)
        {
            string Tooltip = "Tooltip";

            return InstantiatePrefab(Tooltip, Tooltip, parent);
        }

        /// <summary>
        /// Instantiates the small UI divider between buttons.
        /// It's entirely for looking pretty.
        /// </summary>
        /// <param name="parent">The divider parent transform.</param>
        /// <param name="xPos">X Position for the divider.</param>
        /// <returns>The divider.</returns>
        public static GameObject LoadDivider(Transform parent, float xPos)
        {
            string Divider = "Divider";

            var divider = InstantiatePrefab(Divider, Divider, parent);

            var rect = divider.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(xPos, 0);

            return divider;
        }
    }
}
