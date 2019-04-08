using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Arguments to create a Button via the factory.
    /// </summary>
    public class ButtonArgs
    {
        // These properties must always be set
        public string Name = "default";
        public string Tooltip = null;
        public bool Toggleable = true;
        public Sprite IconImage = null; // can be null
        public Action OnSelect = null; // can be null
        public Action OnDeselect = null; // can be null
        public Button[] Children = null; // can be null

        // These properties will be set by a ButtonGroup
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Pivot;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;

        public Color DefaultColor;
        public Color MouseOverColor;
        public Color SelectedColor;
    }

    /// <summary>
    /// Arguments to create a ButtonGroup via the factory.
    /// </summary>
    public class ButtonGroupArgs
    {
        public string Name = "default";
        public Sprite ArrowLeft;
        public Sprite ArrowRight;

        // The buttons to put in a scrollable group.
        // The factory will overwrite positions & images to conform with the group
        public ButtonArgs[] Buttons;

        public Vector2 ButtonSize;

        public Color ButtonsDefaultColor;
        public Color ButtonsMouseOverColor;
        public Color ButtonsSelectedColor;

        public float Left;
        public float Right;
        public float PosY;
        public float Height;
    }

    /// <summary>
    /// Class with factory methods to generate UI components.
    /// A testiment to how uncomfortable I am with using editor tools and prefabs.
    /// </summary>
    public static partial class UIFactory
    {
        /// <summary>
        /// Instantiates an empty game object.
        /// </summary>
        /// <param name="name">Name of the empty game object.</param>
        /// <param name="parent">Parent of the empty game object.</param>
        /// <returns>An empty game object.</returns>
        public static GameObject GenerateEmpty(string name, Transform parent)
        {
            var empty = new GameObject(name);
            empty.transform.SetParent(parent, false);

            return empty;
        }

        /// <summary>
        /// Instantiates an empty UI game object with a full canvas.
        /// </summary>
        /// <param name="name">Name of the empty game object.</param>
        /// <param name="parent">Parent of the empty game object.</param>
        /// <returns>An empty UI game object.</returns>
        public static GameObject GenerateEmptyUI(string name, Transform parent)
        {
            var empty = GenerateEmpty(name, parent);

            var rect = empty.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, 0f);

            return empty;
        }

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
            script.DefaultColor = args.DefaultColor;
            script.MouseOverColor = args.MouseOverColor;
            script.SelectedColor = args.SelectedColor;
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
        public static GameObject GenerateButtonGroup(Transform parent, ButtonGroupArgs args)
        {
            var buttonGroup = new GameObject(args.Name);
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
                    Size = args.ButtonSize,
                    Pivot = new Vector2(1, 0.5f),
                    AnchorMax = new Vector2(0, 0.5f),
                    AnchorMin = new Vector2(0, 0.5f),
                    DefaultColor = args.ButtonsDefaultColor,
                    MouseOverColor = args.ButtonsMouseOverColor,
                    SelectedColor = args.ButtonsSelectedColor,
                    IconImage = args.ArrowLeft,
                });

            // Instantiate the Right Arrow
            var rightArrow = GenerateButton(
                buttonGroup.transform,
                new ButtonArgs()
                {
                    Name = "RightButton",
                    Toggleable = false,
                    Position = new Vector2(0, 0),
                    Size = args.ButtonSize,
                    Pivot = new Vector2(0, 0.5f),
                    AnchorMax = new Vector2(1, 0.5f),
                    AnchorMin = new Vector2(1, 0.5f),
                    DefaultColor = args.ButtonsDefaultColor,
                    MouseOverColor = args.ButtonsMouseOverColor,
                    SelectedColor = args.ButtonsSelectedColor,
                    IconImage = args.ArrowRight,
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
            contentRect.sizeDelta = new Vector2(args.Buttons.Length * args.ButtonSize.x, args.ButtonSize.y);
            contentRect.anchoredPosition = new Vector2(0, 0);

            var buttons = new List<Button>();

            for (int i = 0; i < args.Buttons.Length; ++i)
            {
                var button = args.Buttons[i];

                Assert.IsNotNull(button.Name, "Button in ButtonGroup requires a name!");

                button.Position = new Vector2(i * args.ButtonSize.x, 0);
                button.Size = args.ButtonSize;
                button.Pivot = new Vector2(0, 0.5f);
                button.AnchorMin = new Vector2(0, 0.5f); // Left-Align
                button.AnchorMax = new Vector2(0, 0.5f);

                button.DefaultColor = args.ButtonsDefaultColor;
                button.MouseOverColor = args.ButtonsMouseOverColor;
                button.SelectedColor = args.ButtonsSelectedColor;

                var buttonObject = GenerateButton(content.transform, button);
                buttons.Add(buttonObject.GetComponent<Button>());
            }

            for (int i = 1; i < args.Buttons.Length; ++i)
            {
                // Little extra UI prettyness with the dividers
                LoadDivider(content.transform, i * args.ButtonSize.x - 1);
            }

            var script = buttonGroup.AddComponent<ButtonGroup>();
            script.ScrollButtonLeft = leftArrow;
            script.ScrollButtonRight = rightArrow;
            script.Content = content;
            script.Buttons = buttons;

            return buttonGroup;
        }
    }
}
