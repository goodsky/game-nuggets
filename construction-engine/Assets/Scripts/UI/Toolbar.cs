using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Toolbar : MonoBehaviour
    {
        public Sprite MainMenuBackground;
        public Sprite MainMenuHover;
        public Sprite SubMenuBackground;
        public Sprite SubMenuHover;

        private GameObject _subMenu;
        private GameObject _subMenuButtons;

        void Start()
        {
            var image = gameObject.AddComponent<Image>();
            image.sprite = MainMenuBackground;

            _subMenu = ToolbarFactory.InstantiateSubMenu(gameObject, SubMenuBackground);
            PopulateMenus();
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

            ToolbarFactory.InstantiateButtonGroup(
                "MainButtonGroup", 
                gameObject.transform, 
                new ButtonGroupArgs()
                {
                    Height = mainMenuRect.rect.height,
                    PosY = 0,
                    Left = 175f,
                    Right = 175f,
                    ButtonsDefaultImage = MainMenuBackground,
                    ButtonsMouseOverImage = MainMenuHover,
                    ButtonsSelectedImage = SubMenuBackground,
                    Buttons = new ButtonArgs[]
                    {
                        new ButtonArgs() { Name = "Home", IconImage = Resources.Load<Sprite>("toolbar-icon-test1") },
                        new ButtonArgs() { Name = "Circle", IconImage = Resources.Load<Sprite>("toolbar-icon-test2") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                        new ButtonArgs() { Name = "Diamond", IconImage = Resources.Load<Sprite>("toolbar-icon-test3") },
                    }
                });
        }
    }
}
