using Common;
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
        private GameAccessor _accessor = new GameAccessor();
        private Dictionary<string, ButtonGroup> _buttonGroups = new Dictionary<string, ButtonGroup>();
        private WindowManager _windowManager;
        private GameObject _statusBar;
        private GameObject _mainMenu;
        private GameObject _mainMenuPip;
        private GameObject _subMenu;

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
        /// Try to load a window from the store.
        /// </summary>
        /// <param name="name">Name of the window</param>
        /// <param name="window">The window.</param>
        /// <returns>True if the window exists, false otherwise.</returns>
        public bool TryGetWindow(string name, out Window window)
        {
            return _windowManager.TryGetWindow(name, out window);
        }

        /// <summary>
        /// Opens a window on screen using GameDataStore data.
        /// </summary>
        /// <param name="name">Name of the window to open.</param>
        /// <param name="type">The type of game data to pass to the window.</param>
        /// <param name="dataName">Name of the data to pass to the window.</param>
        public void OpenWindow(string name, GameDataType type, string dataName)
        {
            _windowManager.OpenWindow(name, type, dataName);
        }

        /// <summary>
        /// Opens a window on the screen with custom (possibly null) data.
        /// </summary>
        /// <param name="name">Name of the window to open.</param>
        /// <param name="data">The data to pass to the window.</param>
        public void OpenWindow(string name, object data)
        {
            _windowManager.OpenWindow(name, data);
        }

        /// <summary>
        /// Load UI runtime instances.
        /// </summary>
        /// <param name="data">UI GameData</param>
        protected override void LoadData(UIData data)
        {
            // Create the manager for opening and closing windows
            _windowManager = UIFactory.GenerateEmptyUI("Window Manager", transform).AddComponent<WindowManager>();
            _windowManager.LoadData(data);

            // Create the status bar on the top
            _statusBar = UIFactory.LoadStatusBar(gameObject, data.Config.HorizontalMargins, data.Config);

            // Create the Main Menu Bar on the bottom
            _mainMenu = UIFactory.LoadToolbar(gameObject, "Main Toolbar", 0.0f, data.Config.MainMenuBackgroundColor);
            _mainMenuPip = UIFactory.LoadPip(_mainMenu, data.Config.SubMenuBackgroundColor);

            // Create the second layer menu
            float mainMenuHeight = _mainMenu.GetComponent<RectTransform>().sizeDelta.y;
            _subMenu = UIFactory.LoadToolbar(gameObject, "Sub Toolbar", mainMenuHeight, data.Config.SubMenuBackgroundColor);
            _subMenu.SetActive(false);

            // Link the main and sub menus
            var mainMenuToolbar = _mainMenu.AddComponent<Toolbar>();
            mainMenuToolbar.SubMenu = _subMenu;
            mainMenuToolbar.Pip = _mainMenuPip;

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
            var statusbar = _statusBar.GetComponent<Statusbar>();
            var toolbar = _mainMenu.GetComponent<Toolbar>();

            // Link the StatusBar update into the Simulation Manager 
            _accessor.Simulation.RegisterSimulationUpdateCallback(
                nameof(Statusbar),
                statusbar.SimulationUpdateCallback,
                Simulation.UpdateType.Tick);

            // Link button children and actions
            foreach (var buttonGroupData in data.ButtonGroups)
            {
                var buttonGroup = _buttonGroups[buttonGroupData.Name];

                for (int i = 0; i < buttonGroupData.Buttons.Count; ++i)
                {
                    var buttonData = buttonGroupData.Buttons[i];
                    var button = buttonGroup.Buttons[i];

                    // Link OnSelect Action -----------------------
                    if (buttonData.OnSelect is TransitionGameStateAction)
                    {
                        var transitionGameAction = buttonData.OnSelect as TransitionGameStateAction;
                        button.OnSelect = () => _accessor.StateMachine.StartDoing(transitionGameAction.State);
                    }
                    else if (buttonData.OnSelect is OpenSubMenuAction)
                    {
                        var openSubMenuAction = buttonData.OnSelect as OpenSubMenuAction;
                        var openSubMenuButtons = _accessor.GameData.Get<ButtonGroup>(GameDataType.ButtonGroup, openSubMenuAction.ButtonGroupName);
                        if (openSubMenuButtons == null)
                        {
                            GameLogger.FatalError("OpenSubMenuAction could not link to non-existant button group '{0}'", openSubMenuAction.ButtonGroupName);
                        }
                        button.OnSelect = () => toolbar.OpenSubMenu(openSubMenuButtons);
                    }
                    else if (buttonData.OnSelect is OpenWindowAction)
                    {
                        var openWindowAction = buttonData.OnSelect as OpenWindowAction;
                        if (!_windowManager.TryGetWindow(openWindowAction.WindowName, out Window _))
                        {
                            GameLogger.FatalError("OpenWindowAction could not link to non-existant window '{0}'", openWindowAction.WindowName);
                        }

                        button.OnSelect = () => _windowManager.OpenWindow(openWindowAction.WindowName, null);
                    }
                    else if (buttonData.OnSelect is OpenWindowWithDataAction)
                    {
                        var openWindowAction = buttonData.OnSelect as OpenWindowWithDataAction;
                        if (!_windowManager.TryGetWindow(openWindowAction.WindowName, out Window _))
                        {
                            GameLogger.FatalError("OpenWindowWithDataAction could not link to non-existant window '{0}'", openWindowAction.WindowName);
                        }

                        button.OnSelect = () => _windowManager.OpenWindow(openWindowAction.WindowName, openWindowAction.DataType, openWindowAction.DataName);
                    }

                    // Link OnDeselect Action -----------------------
                    if (buttonData.OnDeselect is TransitionGameStateAction)
                    {
                        var transitionGameAction = buttonData.OnDeselect as TransitionGameStateAction;
                        button.OnDeselect = () => _accessor.StateMachine.StartDoing(transitionGameAction.State);
                    }
                    else if (buttonData.OnDeselect is CloseSubMenuAction)
                    {
                        button.OnDeselect = () => toolbar.CloseSubMenu();
                    }
                    else if (buttonData.OnDeselect is CloseWindowAction)
                    {
                        button.OnDeselect = () => _windowManager.CloseWindow();
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
                menuTransform = _mainMenu.transform;
                background = config.MainMenuBackgroundColor;
                selected = config.MainMenuSelectedColor;
                accent = config.MainMenuAccentColor;
                active = true;
            }
            else if (buttonGroup is SubButtonGroupData)
            {
                menuTransform = _subMenu.transform;
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
                    Height = _mainMenu.GetComponent<RectTransform>().rect.height,
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
