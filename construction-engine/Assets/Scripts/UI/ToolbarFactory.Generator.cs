using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI
{
    public class ButtonArgs
    {
        // These properties must always be set
        public string Name = "default";
        public string Tooltip = null;
        public bool Toggleable = true;
        public Sprite IconImage = null; // can be null
        public Action OnSelect = null; // can be null
        public Action OnDeselect = null; // can be null
        public Button[] Children = null;

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

    public class WindowArgs
    {
        public string Name = "default";
        public float PosX;

        public Sprite BackgroundImage;
    }

    /// <summary>
    /// Class with factory methods to generate UI components.
    /// A testiment to how uncomfortable I am with using editor tools and prefabs.
    /// </summary>
    public static partial class ToolbarFactory
    {
        private static readonly float ButtonWidth = 75.0f;
        private static readonly float ButtonHeight = 75.0f;

        /// <summary>
        /// Instantiates a button.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="parent">The parent of the new button.</param>
        /// <param name="args">Instantiation arguments.</param>
        /// <returns>The button.</returns>
        public static GameObject GenerateButton(Transform parent, ButtonArgs args)
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
            script.OnDeselect = args.OnDeselect;
            script.Tooltip = args.Tooltip;

            foreach (var child in args.Children ?? new Button[0])
            {
                child.SelectionParent = script;
            }

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
        public static GameObject GenerateButtonGroup(string name, Transform parent, ButtonGroupArgs args)
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
            var leftArrow = GenerateButton(
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
            var rightArrow = GenerateButton(
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

                GenerateButton(content.transform, button);
            }

            for (int i = 1; i < args.Buttons.Length; ++i)
            {
                // Little extra UI prettyness with the dividers
                LoadDivider(content.transform, i * ButtonWidth - 1);
            }

            var script = buttonGroup.AddComponent<ButtonGroup>();
            script.ScrollButtonLeft = leftArrow;
            script.ScrollButtonRight = rightArrow;
            script.Content = content;

            return buttonGroup;
        }
    }
}
