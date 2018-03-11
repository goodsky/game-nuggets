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

        public Sprite MainMenuBackground;
        public Sprite MainMenuSelected;
        public Sprite MainMenuPip;
        public Sprite SubMenuBackground;
        public Sprite SubMenuSelected;
        public Sprite SubMenuPip;
        public Sprite PageBackground;

        protected GameObject _subMenu;
        protected GameObject _subMenuButtons;

        private GameObject _window;
        private GameObject _windowContent;

        private GameObject _mainMenuPip;
        private GameObject _subMenuPip;

        void Start()
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            TooltipManager.Initialize(canvas.gameObject.transform);

            var image = gameObject.AddComponent<Image>();
            image.sprite = MainMenuBackground;

            _subMenu = ToolbarFactory.InstantiateSubMenu(gameObject, SubMenuBackground);
            _window = ToolbarFactory.InstantiateWindow(_subMenu.transform,
                new WindowArgs()
                {
                    Name = "Window",
                    PosX = 0,
                    BackgroundImage = PageBackground
                });

            _mainMenuPip = ToolbarFactory.InstantiatePip(gameObject, MainMenuPip);
            _subMenuPip = ToolbarFactory.InstantiatePip(_subMenu, SubMenuPip);

            PopulateMenus();
        }

        void Update()
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
                Height = GetComponent<RectTransform>().rect.height,
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
