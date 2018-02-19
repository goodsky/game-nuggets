using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI
{
    public class ButtonArgs
    {
        // These properties must always be set
        public string Name;
        public bool Toggleable = true;
        public Sprite IconImage; // can be null
        public Action OnSelect; // can be null

        // These properties will be set by a ButtonGroup
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Pivot;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;

        public Sprite DefaultImage;
        public Sprite MouseOverImage;
        public Sprite SelectedImage;
    }

    public class ButtonGroupArgs
    {
        public float Left;
        public float Right;
        public float PosY;
        public float Height;

        public Sprite ButtonsDefaultImage;
        public Sprite ButtonsMouseOverImage;
        public Sprite ButtonsSelectedImage;

        // The Factory will overwrite positions & images to conform with the group
        public ButtonArgs[] Buttons;
    }

    public static class ToolbarFactory
    {
        private readonly static float ButtonWidth = 75.0f;
        private readonly static float ButtonHeight = 75.0f;

        /// <summary>
        /// Instantiates the SubMenu game object as a copy of the main toolbar with a different background.
        /// Starts its life as non-active.
        /// </summary>
        /// <param name="parent">The toolbar to copy.</param>
        /// <param name="background">The sprite image to use.</param>
        /// <returns>The submenu.</returns>
        public static GameObject InstantiateSubMenu(GameObject parent, Sprite background)
        {
            var subMenu = new GameObject("SubToolbar");
            subMenu.transform.SetParent(parent.transform, false);

            var rect = parent.GetComponent<RectTransform>();
            var subRect = subMenu.AddComponent<RectTransform>();
            subRect.pivot = rect.pivot;
            subRect.anchorMin = rect.anchorMin;
            subRect.anchorMax = rect.anchorMax;
            subRect.sizeDelta = rect.sizeDelta;
            subRect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + rect.rect.height);

            var subImage = subMenu.AddComponent<Image>();
            subImage.sprite = background;

            subMenu.SetActive(false);

            return subMenu;
        }

        /// <summary>
        /// Instantiates a button.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="parent">The parent of the new button.</param>
        /// <param name="args">Instantiation arguments.</param>
        /// <returns>The button.</returns>
        public static GameObject InstantiateButton(Transform parent, ButtonArgs args)
        {
            var button = new GameObject(args.Name);
            button.transform.SetParent(parent, false);

            var rect = button.AddComponent<RectTransform>();
            rect.pivot = args.Pivot;
            rect.anchorMin = args.AnchorMin;
            rect.anchorMax = args.AnchorMax;
            rect.sizeDelta = args.Size;
            rect.anchoredPosition = args.Position;

            var script = button.AddComponent<Button>();
            script.Toggleable = args.Toggleable;
            script.DefaultImage = args.DefaultImage;
            script.MouseOverImage = args.MouseOverImage;
            script.SelectedImage = args.SelectedImage;
            script.OnSelect = args.OnSelect;

            if (args.IconImage != null)
            {
                var icon = new GameObject("icon");
                icon.transform.SetParent(button.transform, false);

                var iconRect = icon.AddComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(args.IconImage.rect.width, args.IconImage.rect.height);

                var iconImage = icon.AddComponent<Image>();
                iconImage.sprite = args.IconImage;
                iconImage.raycastTarget = false;

                script.IconImage = iconImage;
            }

            return button;
        }

        /// <summary>
        /// Instantiates a scrollable group of buttons.
        /// </summary>
        /// <param name="name">Name of the button group.</param>
        /// <param name="parent">Parent transform to create the button group on.</param>
        /// <param name="args">Instantiation arguments.</param>
        /// <returns>The button group.</returns>
        public static GameObject InstantiateButtonGroup(string name, Transform parent, ButtonGroupArgs args)
        {
            var buttonGroup = new GameObject(name);
            buttonGroup.transform.SetParent(parent, false);

            var rect = buttonGroup.AddComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.offsetMin = new Vector2(args.Left, 0);
            rect.offsetMax = new Vector2(-args.Right, args.Height);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, args.PosY);

            // Instantiate the Left Arrow
            var leftArrow = InstantiateButton(
                buttonGroup.transform,
                new ButtonArgs()
                {
                    Name = "LeftButton",
                    Toggleable = false,
                    Position = new Vector2(0, 0),
                    Size = new Vector2(ButtonWidth, ButtonHeight),
                    Pivot = new Vector2(1, 0.5f),
                    AnchorMax = new Vector2(0, 0.5f), // 
                    AnchorMin = new Vector2(0, 0.5f),
                    DefaultImage = args.ButtonsDefaultImage,
                    MouseOverImage = args.ButtonsMouseOverImage,
                    SelectedImage = args.ButtonsSelectedImage,
                    IconImage = Resources.Load<Sprite>("toolbar-arrow-left"),
                });

            // Instantiate the Right Arrow
            var rightArrow = InstantiateButton(
                buttonGroup.transform,
                new ButtonArgs()
                {
                    Name = "RightButton",
                    Toggleable = false,
                    Position = new Vector2(0, 0),
                    Size = new Vector2(ButtonWidth, ButtonHeight),
                    Pivot = new Vector2(0, 0.5f),
                    AnchorMax = new Vector2(1, 0.5f),
                    AnchorMin = new Vector2(1, 0.5f),
                    DefaultImage = args.ButtonsDefaultImage,
                    MouseOverImage = args.ButtonsMouseOverImage,
                    SelectedImage = args.ButtonsSelectedImage,
                    IconImage = Resources.Load<Sprite>("toolbar-arrow-right"),
                });

            // Instantiate ButtonGroup's Content Mask
            var mask = new GameObject("Mask");
            mask.transform.SetParent(buttonGroup.transform, false);

            var maskRect = mask.AddComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0, 0); // Fill Parent
            maskRect.anchorMax = new Vector2(1, 1);
            maskRect.sizeDelta = new Vector2(0, 0);

            mask.AddComponent<Image>().raycastTarget = false;
            mask.AddComponent<Mask>().showMaskGraphic = false;

            // Instantiate ButtonGroup's Content Root
            var content = new GameObject("Content");
            content.transform.SetParent(mask.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.pivot = new Vector2(0, 0.5f);
            contentRect.anchorMin = new Vector2(0, 0.5f); // Left-Align
            contentRect.anchorMax = new Vector2(0, 0.5f);
            contentRect.sizeDelta = new Vector2(args.Buttons.Length * ButtonWidth, ButtonHeight);
            contentRect.anchoredPosition = new Vector2(0, 0);

            for (int i = 0; i < args.Buttons.Length; ++i)
            {
                var button = args.Buttons[i];

                Assert.IsNotNull(button.Name, "Button in ButtonGroup requires a name!");

                button.Position = new Vector2(i * ButtonWidth, 0);
                button.Size = new Vector2(ButtonWidth, ButtonHeight);
                button.Pivot = new Vector2(0, 0.5f);
                button.AnchorMin = new Vector2(0, 0.5f); // Left-Align
                button.AnchorMax = new Vector2(0, 0.5f);

                button.DefaultImage = args.ButtonsDefaultImage;
                button.MouseOverImage = args.ButtonsMouseOverImage;
                button.SelectedImage = args.ButtonsSelectedImage;

                InstantiateButton(content.transform, button);
            }

            var script = buttonGroup.AddComponent<ButtonGroup>();
            script.ScrollButtonLeft = leftArrow;
            script.ScrollButtonRight = rightArrow;
            script.Content = content;

            return buttonGroup;
        }
    }
}
