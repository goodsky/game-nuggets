using Common;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The base class for the UI Toolbar.
    /// Extend this class to populate menus in the 'PopulateMenus' method.
    /// </summary>
    public class Toolbar : Singleton<Toolbar>
    {
        protected GameObject _statusBar;
        protected GameObject _mainMenu;
        protected GameObject _subMenu;
        protected GameObject _subMenuButtons;

        private GameObject _mainMenuPip;
        private GameObject _subMenuPip;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected void Start()
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            TooltipManager.Initialize(canvas.gameObject.transform);

            // Fire and forget the selection root object.
            // This will catch click events on the screen that are not on a UI element.
            ToolbarFactory.LoadSelectionRoot(canvas.gameObject);

            // Create the status bar on the top
            _statusBar = ToolbarFactory.LoadStatusBar(gameObject, MainMenuBackground);

            // Create the Main Menu Bar on the bottom
            _mainMenu = ToolbarFactory.LoadMenuBar(gameObject, "MainMenu", 0.0f, MainMenuBackground);
            _mainMenuPip = ToolbarFactory.LoadPip(_mainMenu, MyPalette.Gray.Dark);

            // Creat the second layer menu
            float mainMenuHeight = _mainMenu.GetComponent<RectTransform>().sizeDelta.y;
            _subMenu = ToolbarFactory.LoadMenuBar(gameObject, "SubMenu", mainMenuHeight, SubMenuBackground);
            _subMenuPip = null; // not using sub menu pip
            _subMenu.SetActive(false);

            PopulateMenus();
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
        /// Pop up a sub-menu.
        /// </summary>
        /// <param name="buttonGroup">The button group to populate with the sub-menu.</param>
        public void PopUpSubMenu(GameObject buttonGroup)
        {
            buttonGroup.SetActive(true);
            _subMenu.SetActive(true);

            _subMenuButtons = buttonGroup;

            var selected = Selectable.SelectionManager.Selected;
            if (selected != null)
            {
                _mainMenuPip.transform.SetParent(selected.gameObject.transform, false);
                _mainMenuPip.transform.SetAsFirstSibling();
                _mainMenuPip.SetActive(true);
            }
        }

        /// <summary>
        /// Pop down the sub-menu and its button group.
        /// </summary>
        public void PopDownSubMenu()
        {
            _subMenu.SetActive(false);
            _subMenuButtons.SetActive(false);
            _mainMenuPip.SetActive(false);
        }

        /// <summary>
        /// Pop up a window.
        /// </summary>
        /// <param name="windowContent"></param>
        public void PopUpWindow(GameObject windowContent)
        {
            // TODO: Window Manager

            var selected = Selectable.SelectionManager.Selected;
            if (selected != null && _subMenuPip != null)
            {
                _subMenuPip.transform.SetParent(selected.gameObject.transform, false);
                _subMenuPip.transform.SetAsFirstSibling();
                _subMenuPip.SetActive(true);
            }
        }

        /// <summary>
        /// Pop down the window.
        /// </summary>
        public void PopDownWindow()
        {
            // TODO: Window Manager

            if (_subMenuPip != null)
            {
                _subMenuPip.SetActive(false);
            }
        }

        /// <summary>
        /// Called once during startup to create all the menus.
        /// </summary>
        protected virtual void PopulateMenus() { /* IMPLEMENT ME! :) */ }

        #region Helpful Argument Factories

        /// <summary>
        /// Helper method to generate a Main Menu button group.
        /// </summary>
        /// <param name="name">The name of the main menu.</param>
        /// <param name="buttons">The buttons to put in the main menu.</param>
        /// <returns>A populated button group argument.</returns>
        protected ButtonGroupArgs MainMenuButtonGroup(string name, ButtonArgs[] buttons)
        {
            return new ButtonGroupArgs()
            {
                Name = name,
                Height = _mainMenu.GetComponent<RectTransform>().rect.height,
                PosY = 0,
                Left = ToolbarConstants.HorizontalMargins,
                Right = ToolbarConstants.HorizontalMargins,
                ButtonsDefaultColor = MainMenuBackground,
                ButtonsMouseOverColor = SubMenuBackground,
                ButtonsSelectedColor = MainMenuSelected,
                Buttons = buttons
            };
        }

        /// <summary>
        /// Helper method to generate a Sub Menu button group.
        /// </summary>
        /// <param name="name">The name of the sub menu.</param>
        /// <param name="buttons">The buttons to put in the sub menu.</param>
        /// <returns>A populated button group argument.</returns>
        protected ButtonGroupArgs SubMenuButtonGroup(string name, ButtonArgs[] buttons)
        {
            return new ButtonGroupArgs()
                {
                    Name = name,
                    Height = _subMenu.GetComponent<RectTransform>().rect.height,
                    PosY = 0,
                    Left = ToolbarConstants.HorizontalMargins,
                    Right = ToolbarConstants.HorizontalMargins,
                    ButtonsDefaultColor = SubMenuBackground,
                    ButtonsMouseOverColor = PageBackground,
                    ButtonsSelectedColor = SubMenuSelected,
                    Buttons = buttons
                };
        }

        #endregion
    }
}
