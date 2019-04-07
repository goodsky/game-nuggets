﻿using Common;
using GameData;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Root level object for the game UI.
    /// </summary>
    public class UIManager : GameDataLoader<UIData>
    {
        private Dictionary<string, ButtonGroup> _buttonGroups = new Dictionary<string, ButtonGroup>();

        /// <summary>UI Window Manager</summary>
        public WindowManager WindowManager { get; private set; }

        /// <summary>UI Status Bar</summary>
        public GameObject StatusBar { get; private set; }

        /// <summary>Main Toolbar</summary>
        public GameObject MainMenu { get; private set; }

        /// <summary>Main Toolbar selection pip</summary>
        public GameObject MainMenuPip { get; private set; }

        /// <summary>Sub Toolbarr</summary>
        public GameObject SubMenu { get; private set; }

        /// <summary>
        /// Try to load a button group from the store.
        /// </summary>
        /// <param name="name">Name of the button group to load.</param>
        /// <param name="buttonGroup">The button group.</param>
        /// <returns>True if the button group exists, false otherwise.</returns>
        public bool TryGetButtonGroup(string name, out ButtonGroup buttonGroup)
        {
            return _buttonGroups.TryGetValue(name, out buttonGroup);
        }

        /// <summary>
        /// Load UI runtime instances.
        /// </summary>
        /// <param name="data">UI GameData</param>
        protected override void LoadData(UIData data)
        {
            // Create the manager for opening and closing windows
            WindowManager = UIFactory.GenerateEmptyUI("Window Manager", transform).AddComponent<WindowManager>();
            WindowManager.LoadData(data);

            // Create the status bar on the top
            StatusBar = UIFactory.LoadStatusBar(gameObject, data.Config.HorizontalMargins, data.Config.MainMenuBackgroundColor);

            // Create the Main Menu Bar on the bottom
            MainMenu = UIFactory.LoadToolbar(gameObject, "Main Toolbar", 0.0f, data.Config.MainMenuBackgroundColor);
            MainMenuPip = UIFactory.LoadPip(MainMenu, data.Config.SubMenuBackgroundColor);

            // Create the second layer menu
            float mainMenuHeight = MainMenu.GetComponent<RectTransform>().sizeDelta.y;
            SubMenu = UIFactory.LoadToolbar(gameObject, "Sub Toolbar", mainMenuHeight, data.Config.SubMenuBackgroundColor);
            SubMenu.SetActive(false);

            // Link the main and sub menus
            var mainMenuToolbar = MainMenu.AddComponent<Toolbar>();
            mainMenuToolbar.SubMenu = SubMenu;
            mainMenuToolbar.Pip = MainMenuPip;

            // Load the Button Groups
            foreach (var buttonGroup in data.ButtonGroups)
            {
                _buttonGroups[buttonGroup.Name] = CreateButtonGroup(buttonGroup, data.Config);
            }
        }

        /// <summary>
        /// Link actions that reference other GameObjects.
        /// All other GameObjects in dependent stores must already be Instantiated.
        /// </summary>
        /// <param name="data">UI GameData</param>
        protected override void LinkData(UIData data)
        {
            var toolbar = MainMenu.GetComponent<Toolbar>();

            // Link button children and actions
            foreach (var buttonGroupData in data.ButtonGroups)
            {
                var buttonGroup = _buttonGroups[buttonGroupData.Name];

                for (int i = 0; i < buttonGroupData.Buttons.Count; ++i)
                {
                    var buttonData = buttonGroupData.Buttons[i];
                    var button = buttonGroup.Buttons[i];

                    // Link OnSelect Action -----------------------
                    if (buttonData.OnSelect is OpenSubMenuAction)
                    {
                        var openSubMenuAction = buttonData.OnSelect as OpenSubMenuAction;
                        var openSubMenuButtons = GameDataStore.Get<ButtonGroup>(GameDataType.ButtonGroup, openSubMenuAction.ButtonGroupName);
                        if (openSubMenuButtons == null)
                        {
                            GameLogger.FatalError("OpenSubMenuAction could not link to non-existant button group '{0}'", openSubMenuAction.ButtonGroupName);
                        }
                        button.OnSelect = () => toolbar.OpenSubMenu(openSubMenuButtons);
                    }
                    else if (buttonData.OnSelect is OpenWindowAction)
                    {
                        var openWindowAction = buttonData.OnSelect as OpenWindowAction;
                        if (!WindowManager.TryGetWindow(openWindowAction.WindowName, out Window _))
                        {
                            GameLogger.FatalError("OpenWindowAction could not link to non-existant window '{0}'", openWindowAction.WindowName);
                        }

                        button.OnSelect = () => WindowManager.OpenWindow(openWindowAction.WindowName, null);
                    }
                    else if (buttonData.OnSelect is OpenWindowWithDataAction)
                    {
                        var openWindowAction = buttonData.OnSelect as OpenWindowWithDataAction;
                        if (!WindowManager.TryGetWindow(openWindowAction.WindowName, out Window _))
                        {
                            GameLogger.FatalError("OpenWindowWithDataAction could not link to non-existant window '{0}'", openWindowAction.WindowName);
                        }

                        button.OnSelect = () => WindowManager.OpenWindow(openWindowAction.WindowName, openWindowAction.DataType, openWindowAction.DataName);
                    }

                    // Link OnDeselect Action -----------------------
                    if (buttonData.OnDeselect is CloseSubMenuAction)
                    {
                        button.OnDeselect = () => toolbar.CloseSubMenu();
                    }
                    else if (buttonData.OnDeselect is CloseWindowAction)
                    {
                        button.OnDeselect = () => WindowManager.CloseWindow();
                    }
                }
            }
        }

        /// <summary>
        /// Convert ButtonGroup GameData into a GameObject in the store.
        /// Uses the Factory methods to load the button group.
        /// </summary>
        private ButtonGroup CreateButtonGroup(ButtonGroupData buttonGroup, UIConfig config)
        {
            Transform menuTransform = null;
            Color background = Color.white;
            Color selected = Color.white;
            Color accent = Color.white;
            bool active = true;

            if (buttonGroup is MainButtonGroupData)
            {
                menuTransform = MainMenu.transform;
                background = config.MainMenuBackgroundColor;
                selected = config.MainMenuSelectedColor;
                accent = config.MainMenuAccentColor;
                active = true;
            }
            else if (buttonGroup is SubButtonGroupData)
            {
                menuTransform = SubMenu.transform;
                background = config.SubMenuBackgroundColor;
                selected = config.SubMenuSelectedColor;
                accent = config.SubMenuAccentColor;
                active = false;
            }

            var buttonGroupObject = UIFactory.GenerateButtonGroup(
                menuTransform,
                new ButtonGroupArgs()
                {
                    Name = buttonGroup.Name,
                    ArrowLeft = config.ArrowLeftSprite,
                    ArrowRight = config.ArrowRightSprite,
                    Height = MainMenu.GetComponent<RectTransform>().rect.height,
                    PosY = 0,
                    Left = config.HorizontalMargins,
                    Right = config.HorizontalMargins,
                    ButtonSize = new Vector2(config.ButtonWidth, config.ButtonHeight),
                    ButtonsDefaultColor = background,
                    ButtonsMouseOverColor = accent,
                    ButtonsSelectedColor = selected,
                    Buttons = CreateButtonArgs(buttonGroup.Buttons, config)
                });

            buttonGroupObject.SetActive(active);

            return buttonGroupObject.GetComponent<ButtonGroup>();
        }

        /// <summary>
        /// Convert Button GameData into arguments for the Factory method.
        /// Called by CreateButtonGroup.
        /// </summary>
        private ButtonArgs[] CreateButtonArgs(List<ButtonData> buttons, UIConfig config)
        {
            ButtonArgs[] buttonArgs = new ButtonArgs[buttons.Count];
            for (int i = 0; i < buttonArgs.Length; ++i)
            {
                ButtonData button = buttons[i];

                buttonArgs[i] = new ButtonArgs()
                {
                    Name = button.Name,
                    Tooltip = button.Tooltip,
                    IconImage = button.IconImage,
                    // OnSelect and OnDeselect linked during second pass construction
                };
            }

            return buttonArgs;
        }
    }
}
