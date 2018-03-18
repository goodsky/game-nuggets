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

        /// <summary>UI Status Bar</summary>
        public GameObject StatusBar { get; private set; }

        /// <summary>Main Toolbar</summary>
        public GameObject MainMenu { get; private set; }

        /// <summary>Main Toolbar selection pip</summary>
        public GameObject MainMenuPip { get; private set; }

        /// <summary>Sub Toolbarr</summary>
        public GameObject SubMenu { get; private set; }

        /// <summary>
        /// Load UI runtime instances.
        /// </summary>
        /// <param name="gameData">UI GameData</param>
        protected override void LoadData(UIData gameData)
        {
            // Fire and forget the selection root object.
            // This will catch click events on the screen that are not on a UI element.
            UIFactory.LoadSelectionRoot(gameObject);

            // Create the status bar on the top
            StatusBar = UIFactory.LoadStatusBar(gameObject, gameData.Config.HorizontalMargins, gameData.Config.MainMenuBackgroundColor.Value);

            // Create the Main Menu Bar on the bottom
            MainMenu = UIFactory.LoadToolbar(gameObject, "Main Toolbar", 0.0f, gameData.Config.MainMenuBackgroundColor.Value);
            MainMenuPip = UIFactory.LoadPip(MainMenu, gameData.Config.SubMenuBackgroundColor.Value);

            // Create the second layer menu
            float mainMenuHeight = MainMenu.GetComponent<RectTransform>().sizeDelta.y;
            SubMenu = UIFactory.LoadToolbar(gameObject, "Sub Toolbar", mainMenuHeight, gameData.Config.SubMenuBackgroundColor.Value);
            SubMenu.SetActive(false);

            // Link the main and sub menus
            var mainMenuToolbar = MainMenu.AddComponent<Toolbar>();
            mainMenuToolbar.SubMenu = SubMenu;
            mainMenuToolbar.Pip = MainMenuPip;

            // Load the Button Groups
            foreach (var buttonGroup in gameData.ButtonGroups)
            {
                _buttonGroups[buttonGroup.Name] = CreateButtonGroup(buttonGroup, gameData.Config);
            }
        }

        /// <summary>
        /// Link actions that reference other GameObjects.
        /// All other GameObjects in dependent stores must already be Instantiated.
        /// </summary>
        /// <param name="gameData">UI GameData</param>
        protected override void LinkData(UIData gameData)
        {
            var toolbar = MainMenu.GetComponent<Toolbar>();

            // Link button children and actions
            foreach (var buttonGroupData in gameData.ButtonGroups)
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
                        var openSubMenuButtons = _buttonGroups[openSubMenuAction.ButtonGroupName];
                        button.OnSelect = () => toolbar.OpenSubMenu(openSubMenuButtons);
                    }
                    else if (buttonData.OnSelect is OpenWindowAction)
                    {
                        // reference window
                    }

                    // Link OnDeselect Action -----------------------
                    if (buttonData.OnDeselect is CloseSubMenuAction)
                    {
                        button.OnDeselect = () => toolbar.CloseSubMenu();
                    }
                    else if (buttonData.OnDeselect is CloseWindowAction)
                    {
                        // reference window
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
                background = config.MainMenuBackgroundColor.Value;
                selected = config.MainMenuSelectedColor.Value;
                accent = config.MainMenuAccentColor.Value;
                active = true;
            }
            else if (buttonGroup is SubButtonGroupData)
            {
                menuTransform = SubMenu.transform;
                background = config.SubMenuBackgroundColor.Value;
                selected = config.SubMenuSelectedColor.Value;
                accent = config.SubMenuAccentColor.Value;
                active = false;
            }

            var buttonGroupObject = UIFactory.GenerateButtonGroup(
                menuTransform,
                new ButtonGroupArgs()
                {
                    Name = buttonGroup.Name,
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
                    IconImage = Resources.Load<Sprite>(button.IconImage),
                    // OnSelect and OnDeselect linked during second pass construction
                };
            }

            return buttonArgs;
        }
    }
}
