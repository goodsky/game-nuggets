using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Toolbar : MonoBehaviour
    {
        private static readonly float ButtonGroupMargins = 175.0f;

        public Sprite MainMenuBackground;
        public Sprite MainMenuHover;
        public Sprite SubMenuBackground;
        public Sprite SubMenuHover;
        public Sprite PageBackground;

        private GameObject _subMenu;
        private GameObject _subMenuButtons;

        void Start()
        {
            var image = gameObject.AddComponent<Image>();
            image.sprite = MainMenuBackground;

            _subMenu = ToolbarFactory.InstantiateSubMenu(gameObject, SubMenuBackground);
            PopulateMenus();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Selectable.SelectionManager.UpdateSelection(null);
            }
        }

        public void PopUpMenu(GameObject buttonGroup)
        {
            var buttonsRect = buttonGroup.GetComponent<RectTransform>();
            if (buttonsRect == null)
                throw new InvalidOperationException("Attempted to Pop Up Menu with an invalid UI element.");

            buttonGroup.SetActive(true);
            _subMenu.SetActive(true);

            _subMenuButtons = buttonGroup;
        }

        public void PopDownMenu()
        {
            _subMenu.SetActive(false);
            _subMenuButtons.SetActive(false);
        }

        private void PopulateMenus()
        {
            var mainMenuRect = GetComponent<RectTransform>();
            var subMenuRect = _subMenu.GetComponent<RectTransform>();

            var homeSubMenu = ToolbarFactory.InstantiateButtonGroup(
                "HomeSubMenu",
                _subMenu.transform,
                new ButtonGroupArgs()
                {
                    Height = subMenuRect.rect.height,
                    PosY = 0,
                    Left = ButtonGroupMargins,
                    Right = ButtonGroupMargins,
                    ButtonsDefaultImage = SubMenuBackground,
                    ButtonsMouseOverImage = SubMenuHover,
                    ButtonsSelectedImage = PageBackground,
                    Buttons = new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                    }
                });

            var circleSubMenu = ToolbarFactory.InstantiateButtonGroup(
                "CircleSubMenu",
                _subMenu.transform,
                new ButtonGroupArgs()
                {
                    Height = subMenuRect.rect.height,
                    PosY = 0,
                    Left = ButtonGroupMargins,
                    Right = ButtonGroupMargins,
                    ButtonsDefaultImage = SubMenuBackground,
                    ButtonsMouseOverImage = SubMenuHover,
                    ButtonsSelectedImage = PageBackground,
                    Buttons = new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuCircle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "SubMenuCircle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "SubMenuCircle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                    }
                });

            var diamondSubMenu = ToolbarFactory.InstantiateButtonGroup(
                "DiamondSubMenu",
                _subMenu.transform,
                new ButtonGroupArgs()
                {
                    Height = subMenuRect.rect.height,
                    PosY = 0,
                    Left = ButtonGroupMargins,
                    Right = ButtonGroupMargins,
                    ButtonsDefaultImage = SubMenuBackground,
                    ButtonsMouseOverImage = SubMenuHover,
                    ButtonsSelectedImage = PageBackground,
                    Buttons = new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                    }
                });

            homeSubMenu.SetActive(false);
            circleSubMenu.SetActive(false);
            diamondSubMenu.SetActive(false);

            var homeSubMenuButtons = homeSubMenu.GetComponentsInChildren<Button>();
            var circleSubMenuButtons = circleSubMenu.GetComponentsInChildren<Button>();
            var diamondSubMenuButtons = diamondSubMenu.GetComponentsInChildren<Button>();

            ToolbarFactory.InstantiateButtonGroup(
                "MainMenu",
                gameObject.transform,
                new ButtonGroupArgs()
                {
                    Height = mainMenuRect.rect.height,
                    PosY = 0,
                    Left = ButtonGroupMargins,
                    Right = ButtonGroupMargins,
                    ButtonsDefaultImage = MainMenuBackground,
                    ButtonsMouseOverImage = MainMenuHover,
                    ButtonsSelectedImage = SubMenuBackground,
                    Buttons = new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "Home", Children = homeSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test1"), OnSelect = () => PopUpMenu(homeSubMenu), OnDeselect = () => PopDownMenu() },
                        new ButtonArgs() { Name = "Circle", Children = circleSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test2"), OnSelect = () => PopUpMenu(circleSubMenu), OnDeselect = () => PopDownMenu() },
                        new ButtonArgs() { Name = "Diamond", Children = diamondSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test3"), OnSelect = () => PopUpMenu(diamondSubMenu), OnDeselect = () => PopDownMenu() },
                    }
                });

        }
    }
}
