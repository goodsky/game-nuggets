using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The base class for the UI Toolbar.
    /// Can be extended to create a specific style of toolbar.
    /// </summary>
    public class Toolbar : MonoBehaviour
    {
        protected static readonly float ButtonGroupMargins = 175.0f;

        /// <summary>
        /// Main menu 'Default' color
        /// </summary>
        public Sprite MainMenuBackground;

        /// <summary>
        /// Main menu 'Selected' color
        /// </summary>
        public Sprite MainMenuSelected;

        /// <summary>
        /// Main menu Pip Sprite
        /// </summary>
        public Sprite MainMenuPip;

        /// <summary>
        /// Sub menu 'Default' color
        /// </summary>
        public Sprite SubMenuBackground;

        /// <summary>
        /// Sub menu 'Selected' color
        /// </summary>
        public Sprite SubMenuSelected;

        /// <summary>
        /// Sub menu Pip Sprite
        /// </summary>
        public Sprite SubMenuPip;

        /// <summary>
        /// Background Window color
        /// (This may be removed)
        /// </summary>
        public Sprite PageBackground;

        protected GameObject _mainMenu;
        protected GameObject _subMenu;
        protected GameObject _subMenuButtons;

        private GameObject _window;
        private GameObject _windowContent;

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

            // Create the Main Menu Bar on the bottom
            _mainMenu = ToolbarFactory.LoadMenuBar(gameObject, "MainMenu", 0.0f, MainMenuBackground);

            // Creat the second layer menu
            float mainMenuHeight = _mainMenu.GetComponent<RectTransform>().sizeDelta.y;
            _subMenu = ToolbarFactory.LoadMenuBar(gameObject, "SubMenu", mainMenuHeight, SubMenuBackground);
            _subMenu.SetActive(false);

            _window = ToolbarFactory.GenerateWindow(_subMenu.transform,
                new WindowArgs()
                {
                    Name = "Window",
                    PosX = 0,
                    BackgroundImage = PageBackground
                });

            _mainMenuPip = ToolbarFactory.LoadPip(_mainMenu, MyColors.Gray.Dark);
            _subMenuPip = ToolbarFactory.LoadPip(_subMenu, MyColors.Gray.Light);

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

        public void PopUpWindow(GameObject windowContent)
        {
            // windowContent.SetActive(true);
            _window.SetActive(true);

            _windowContent = windowContent;

            var selected = Selectable.SelectionManager.Selected;
            if (selected != null)
            {
                _subMenuPip.transform.SetParent(selected.gameObject.transform, false);
                _subMenuPip.transform.SetAsFirstSibling();
                _subMenuPip.SetActive(true);
            }
        }

        public void PopDownWindow()
        {
            _window.SetActive(false);
            // _windowContent.SetActive(false);
            _subMenuPip.SetActive(false);
        }

        /// <summary>
        /// Called once during startup to create all the menus.
        /// </summary>
        protected virtual void PopulateMenus() { /* IMPLEMENT ME! :) */ }

        #region Helpful Factory Templates
        protected ButtonGroupArgs MainMenuGroup(ButtonArgs[] buttons)
        {
            return new ButtonGroupArgs()
            {
                Height = _mainMenu.GetComponent<RectTransform>().rect.height,
                PosY = 0,
                Left = ButtonGroupMargins,
                Right = ButtonGroupMargins,
                ButtonsDefaultImage = MainMenuBackground,
                ButtonsMouseOverImage = SubMenuBackground,
                ButtonsSelectedImage = MainMenuSelected,
                Buttons = buttons
            };
        }

        protected ButtonGroupArgs SubMenuGroup(ButtonArgs[] buttons)
        {
            return new ButtonGroupArgs()
                {
                    Height = _subMenu.GetComponent<RectTransform>().rect.height,
                    PosY = 0,
                    Left = ButtonGroupMargins,
                    Right = ButtonGroupMargins,
                    ButtonsDefaultImage = SubMenuBackground,
                    ButtonsMouseOverImage = PageBackground,
                    ButtonsSelectedImage = SubMenuSelected,
                    Buttons = buttons
                };
        }
        #endregion
    }
}
