using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Toolbar01 : Toolbar
    {
        protected override void PopulateMenus()
        {
            var homeSubMenu = ToolbarFactory.GenerateButtonGroup(
                "HomeSubMenu",
                _subMenu.transform,
                SubMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                    }));


            var circleSubMenu = ToolbarFactory.GenerateButtonGroup(
                "CircleSubMenu",
                _subMenu.transform,
                SubMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuCircle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "SubMenuCircle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "SubMenuCircle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                    }));

            var diamondSubMenu = ToolbarFactory.GenerateButtonGroup(
                "DiamondSubMenu",
                _subMenu.transform,
                SubMenuGroup(
                    new ButtonArgs[]
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
                    }));

            homeSubMenu.SetActive(false);
            circleSubMenu.SetActive(false);
            diamondSubMenu.SetActive(false);

            var homeSubMenuButtons = homeSubMenu.GetComponentsInChildren<Button>();
            var circleSubMenuButtons = circleSubMenu.GetComponentsInChildren<Button>();
            var diamondSubMenuButtons = diamondSubMenu.GetComponentsInChildren<Button>();

            ToolbarFactory.GenerateButtonGroup(
                "MainMenu",
                gameObject.transform,
                MainMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "Home", Children = homeSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test1"), OnSelect = () => PopUpSubMenu(homeSubMenu), OnDeselect = () => PopDownSubMenu() },
                        new ButtonArgs() { Name = "Circle", Children = circleSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test2"), OnSelect = () => PopUpSubMenu(circleSubMenu), OnDeselect = () => PopDownSubMenu() },
                        new ButtonArgs() { Name = "Diamond", Children = diamondSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test3"), OnSelect = () => PopUpSubMenu(diamondSubMenu), OnDeselect = () => PopDownSubMenu() },
                    }));
        }
    }
}
