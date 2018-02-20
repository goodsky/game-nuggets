using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Toolbar02 : Toolbar
    {
        protected override void PopulateMenus()
        {
            var homeSubMenu = ToolbarFactory.InstantiateButtonGroup(
                "HomeSubMenu",
                _subMenu.transform,
                SubMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuHome", Tooltip = "Another Home", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", Tooltip = "Another Home", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", Tooltip = "Another Home", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "SubMenuHome", Tooltip = "Another Home", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                    }));


            var circleSubMenu = ToolbarFactory.InstantiateButtonGroup(
                "CircleSubMenu",
                _subMenu.transform,
                SubMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuCircle", Tooltip = "Another Circle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "SubMenuCircle", Tooltip = "Another Circle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "SubMenuCircle", Tooltip = "Another Circle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                    }));

            var diamondSubMenu = ToolbarFactory.InstantiateButtonGroup(
                "DiamondSubMenu",
                _subMenu.transform,
                SubMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "SubMenuDiamond", Tooltip = "Another Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                    }));

            homeSubMenu.SetActive(false);
            circleSubMenu.SetActive(false);
            diamondSubMenu.SetActive(false);

            var homeSubMenuButtons = homeSubMenu.GetComponentsInChildren<Button>();
            var circleSubMenuButtons = circleSubMenu.GetComponentsInChildren<Button>();
            var diamondSubMenuButtons = diamondSubMenu.GetComponentsInChildren<Button>();

            ToolbarFactory.InstantiateButtonGroup(
                "MainMenu",
                gameObject.transform,
                MainMenuGroup(
                    new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "Home", Tooltip = "Home", Children = homeSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test1"), OnSelect = () => PopUpSubMenu(homeSubMenu), OnDeselect = () => PopDownSubMenu() },
                        new ButtonArgs() { Name = "Circle", Tooltip = "Circle", Children = circleSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test2"), OnSelect = () => PopUpSubMenu(circleSubMenu), OnDeselect = () => PopDownSubMenu() },
                        new ButtonArgs() { Name = "Diamond", Tooltip = "Diamond", Children = diamondSubMenuButtons, IconImage = Resources.Load<Sprite>("toolbar-icon-test3"), OnSelect = () => PopUpSubMenu(diamondSubMenu), OnDeselect = () => PopDownSubMenu() },
                    }));
        }
    }
}
