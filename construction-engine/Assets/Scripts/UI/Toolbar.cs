using Common;
using GameData;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The base class for the UI Toolbar.
    /// Extend this class to populate menus in the 'PopulateMenus' method.
    /// </summary>
    public class Toolbar : MonoBehaviour
    {
        /// <summary>Store for toolbar game objects</summary>
        private ToolbarStore _toolbarStore;

        /// <summary>Sub Menu buttons that are currently active.</summary>
        private GameObject _activeSubMenuButtons;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected void Start()
        {

        }

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Selectable.SelectionManager.UpdateSelection(null);
            }
        }

        /// <summary>
        /// Initialize the game data.
        /// </summary>
        /// <param name="toolbarData">Toolbar game data.</param>
        public void InitializeStore(ToolbarData toolbarData)
        {
            _toolbarStore = GetComponent<ToolbarStore>();
            if (_toolbarStore == null)
            {
                _toolbarStore = gameObject.AddComponent<ToolbarStore>();
            }

            _toolbarStore.Build(toolbarData);
        }

        /// <summary>
        /// Link references between game data.
        /// </summary>
        public void LinkStore()
        {
            if (_toolbarStore == null)
            {
                GameLogger.Error("Failed to Link ToolbarStore. Store does not exist.");
                Application.Quit();
            }

            _toolbarStore.Link();
        }

        /// <summary>
        /// Pop up a sub-menu.
        /// </summary>
        /// <param name="buttonGroup">The button group to populate with the sub-menu.</param>
        public void PopUpSubMenu(GameObject buttonGroup)
        {
            _toolbarStore.SubMenu.SetActive(true);

            var selected = Selectable.SelectionManager.Selected;
            if (selected != null)
            {
                _toolbarStore.MainMenuPip.transform.SetParent(selected.gameObject.transform, false);
                _toolbarStore.MainMenuPip.transform.SetAsFirstSibling();
                _toolbarStore.MainMenuPip.SetActive(true);
            }

            buttonGroup.SetActive(true);
            _activeSubMenuButtons = buttonGroup;
        }

        /// <summary>
        /// Pop down the sub-menu and its button group.
        /// </summary>
        public void PopDownSubMenu()
        {
            _toolbarStore.SubMenu.SetActive(false);
            _toolbarStore.MainMenuPip.SetActive(false);

            _activeSubMenuButtons.SetActive(false);
        }

        /// <summary>
        /// Pop up a window.
        /// </summary>
        /// <param name="windowContent"></param>
        public void PopUpWindow(GameObject windowContent)
        {
            // TODO: Window Manager
        }

        /// <summary>
        /// Pop down the window.
        /// </summary>
        public void PopDownWindow()
        {
            // TODO: Window Manager
        }
    }
}
