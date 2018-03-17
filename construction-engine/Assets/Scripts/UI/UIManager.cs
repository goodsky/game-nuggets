using Common;
using GameData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private Dictionary<string, GameObject> _buttonGroups = new Dictionary<string, GameObject>();
        private UIData _data;

        /// <summary>The configuration to load</summary>
        public TextAsset Config { get; set; }

        /// <summary>UI Status Bar</summary>
        public GameObject StatusBar { get; private set; }

        /// <summary>Main Toolbar</summary>
        public GameObject MainMenu { get; private set; }

        /// <summary>Main Toolbar selection pip</summary>
        public GameObject MainMenuPip { get; private set; }

        /// <summary>Sub Toolbarr</summary>
        public GameObject SubMenu { get; private set; }

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected void Awake()
        {
            try
            {
                _data = GameDataSerializer.Load<UIData>(Config);
            }
            catch (Exception e)
            {
                GameLogger.FatalError("Failed to load toolbar game data. Ex = {0}", e);
            }

            // Fire and forget the selection root object.
            // This will catch click events on the screen that are not on a UI element.
            UIFactory.LoadSelectionRoot(gameObject);

            // Create the status bar on the top
            StatusBar = UIFactory.LoadStatusBar(gameObject, _data.Config.HorizontalMargins, _data.Config.MainMenuBackgroundColor.Value);

            // Create the Main Menu Bar on the bottom
            MainMenu = UIFactory.LoadToolbar(gameObject, "Main Toolbar", 0.0f, _data.Config.MainMenuBackgroundColor.Value);
            MainMenuPip = UIFactory.LoadPip(MainMenu, _data.Config.SubMenuBackgroundColor.Value);

            // Create the second layer menu
            float mainMenuHeight = MainMenu.GetComponent<RectTransform>().sizeDelta.y;
            SubMenu = UIFactory.LoadToolbar(gameObject, "Sub Toolbar", mainMenuHeight, _data.Config.SubMenuBackgroundColor.Value);
            SubMenu.SetActive(false);

            // Link the main and sub menus
            var mainMenuToolbar = MainMenu.AddComponent<Toolbar>();
            mainMenuToolbar.SubMenu = SubMenu;
            mainMenuToolbar.Pip = MainMenuPip;

            // Load the Button Groups
            foreach (var buttonGroup in _data.ButtonGroups)
            {
                _buttonGroups[buttonGroup.Name] = CreateButtonGroup(buttonGroup, _data.Config);
            }
        }

        /// <summary>
        /// Link actions that reference other GameObjects.
        /// All other GameObjects in dependent stores must already be Instantiated.
        /// </summary>
        /// <param name="data">Toolbar game data.</param>
        protected void Start()
        {
            var toolbar = MainMenu.GetComponent<Toolbar>();

            // Link button children and actions
            foreach (var buttonGroupData in _data.ButtonGroups)
            {
                var buttonGroupObject = _buttonGroups[buttonGroupData.Name];
                var buttonGroup = buttonGroupObject.GetComponent<ButtonGroup>();

                for (int i = 0; i < buttonGroupData.Buttons.Count; ++i)
                {
                    var buttonData = buttonGroupData.Buttons[i];
                    var button = buttonGroup.Buttons[i];

                    // Link OnSelect Action -----------------------
                    if (buttonData.OnSelect is PopUpSubMenuAction)
                    {
                        var popUpSubMenuAction = buttonData.OnSelect as PopUpSubMenuAction;
                        var popUpSubMenuObject = _buttonGroups[popUpSubMenuAction.ButtonGroupName];
                        button.OnSelect = () => toolbar.PopUpSubMenu(popUpSubMenuObject);
                    }
                    else if (buttonData.OnSelect is PopUpWindowAction)
                    {
                        // reference window
                    }

                    // Link OnDeselect Action -----------------------
                    if (buttonData.OnDeselect is PopDownSubMenuAction)
                    {
                        button.OnDeselect = () => toolbar.PopDownSubMenu();
                    }
                    else if (buttonData.OnDeselect is PopDownWindowAction)
                    {
                        // reference window
                    }

                    /// Link Child Selectables -----------------------
                    if (!string.IsNullOrEmpty(buttonData.ChildButtonGroup))
                    {
                        var childButtonGroupObject = _buttonGroups[buttonData.ChildButtonGroup];
                        var childButtonGroup = childButtonGroupObject.GetComponent<ButtonGroup>();

                        foreach (var childButton in childButtonGroup.Buttons)
                        {
                            childButton.SelectionParent = button;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Convert ButtonGroup GameData into a GameObject in the store.
        /// Uses the Factory methods to load the button group.
        /// </summary>
        private GameObject CreateButtonGroup(ButtonGroupData buttonGroup, UIConfig config)
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
                    Buttons = CreateButtons(buttonGroup.Buttons, config)
                });

            buttonGroupObject.SetActive(active);

            return buttonGroupObject;
        }

        /// <summary>
        /// Convert Button GameData into arguments for the Factory method.
        /// Called by CreateButtonGroup.
        /// </summary>
        private ButtonArgs[] CreateButtons(List<ButtonData> buttons, UIConfig config)
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
