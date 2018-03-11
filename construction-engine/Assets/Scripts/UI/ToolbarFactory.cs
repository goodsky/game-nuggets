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
    public static class ToolbarFactory
    {
        private readonly static float ButtonWidth = 75.0f;
        private readonly static float ButtonHeight = 75.0f;

        /// <summary>
        /// Instantiates a screen-wide object that is the root of UI selections. 
        /// This means if you click on the non-UI screen then the selection manager will reset.
        /// </summary>
        /// <param name="parent">The Canvas parent.</param>
        /// <returns>The selection root.</returns>
        public static GameObject InstantiateSelectionRoot(GameObject parent)
        {
            var selectionRoot = new GameObject("SelectionRoot");
            selectionRoot.transform.SetParent(parent.transform, false);
            selectionRoot.transform.SetAsFirstSibling();

            var rootRect = selectionRoot.AddComponent<RectTransform>();
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchorMin = new Vector2(0.0f, 0.0f);
            rootRect.anchorMax = new Vector2(1.0f, 1.0f);

            var rootImage = selectionRoot.AddComponent<Image>();
            rootImage.color = new Color(255.0f, 255.0f, 255.0f, 0.0f);
            rootImage.raycastTarget = true;

            var rootSelectable = selectionRoot.AddComponent<Selectable>();
            rootSelectable.IsEnabled = true;
            rootSelectable.Toggleable = true;

            return selectionRoot;
        }

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
        /// Instantiates a the small UI 'pip' that is used entirely for looking pretty.
        /// Stop and smell the roses sometimes.
        /// </summary>
        /// <param name="parent">The parent of the pip.</param>
        /// <param name="image">The pip image.</param>
        /// <returns></returns>
        public static GameObject InstantiatePip(GameObject parent, Sprite image)
        {
            var pip = new GameObject("Pip");
            pip.transform.SetParent(parent.transform, false);

            var pipRect = pip.AddComponent<RectTransform>();
            pipRect.pivot = new Vector2(0.5f, 1);
            pipRect.anchorMin = new Vector2(0.5f, 1);
            pipRect.anchorMax = new Vector2(0.5f, 1);
            pipRect.sizeDelta = new Vector2(image.rect.width, image.rect.height);
            pipRect.anchoredPosition = new Vector2(0, 0);

            var pipImage = pip.AddComponent<Image>();
            pipImage.sprite = image;
            pipImage.raycastTarget = false;

            pip.SetActive(false);

            return pip;
        }

        /// <summary>
        /// Instantiates a tooltip textbox.
        /// It pops up over buttons to lend helpful pointers.
        /// </summary>
        /// <param name="parent">The tooltip parent.</param>
        /// <returns></returns>
        public static GameObject InstantiateTooltip(Transform parent)
        {
            var tooltip = new GameObject("Tooltip");
            tooltip.transform.SetParent(parent.transform, false);

            var tooltipRect = tooltip.AddComponent<RectTransform>();
            tooltipRect.pivot = new Vector2(0, 1);
            tooltipRect.anchorMin = new Vector2(0, 1);
            tooltipRect.anchorMax = new Vector2(1, 0);

            var tooltipImage = tooltip.AddComponent<Image>();
            tooltipImage.color = Color.grey;
            tooltipImage.raycastTarget = false;

            var tooltipLayout = tooltip.AddComponent<HorizontalLayoutGroup>();
            tooltipLayout.padding = new RectOffset(5, 5, 5, 5);
            tooltipLayout.childForceExpandWidth = tooltipLayout.childForceExpandHeight = false;
            tooltipLayout.childControlWidth = tooltipLayout.childControlHeight = true;

            var tooltipSizeFitter = tooltip.AddComponent<ContentSizeFitter>();
            tooltipSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tooltipSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var text = new GameObject("Text");
            text.transform.SetParent(tooltip.transform, false);

            var textRect = text.AddComponent<RectTransform>();
            textRect.pivot = new Vector2(0.5f, 0.5f);

            var textText = text.AddComponent<Text>();
            textText.horizontalOverflow = HorizontalWrapMode.Wrap;
            textText.verticalOverflow = VerticalWrapMode.Truncate;
            textText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textText.color = Color.black;

            return tooltip;
        }

        /// <summary>
        /// Instantiates the small UI divider between buttons.
        /// It's entirely for looking pretty.
        /// </summary>
        /// <param name="parent">The parent for the divider.</param>
        /// <param name="xPos">X Position for the divider.</param>
        /// <returns></returns>
        public static GameObject InstantiateDivider(Transform parent, float xPos)
        {
            var divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);

            var rectSprite = Resources.Load<Sprite>("toolbar-divider");

            var rect = divider.AddComponent<RectTransform>();
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(rectSprite.rect.width, rectSprite.rect.height);
            rect.anchoredPosition = new Vector2(xPos, 0);

            var rectImage = divider.AddComponent<Image>();
            rectImage.sprite = rectSprite;
            rectImage.raycastTarget = false;

            return divider;
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
                    Toggleable = false,                    Position = new Vector2(0, 0),
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

            for (int i = 1; i < args.Buttons.Length; ++i)
            {
                // Little extra UI prettyness with the dividers
                InstantiateDivider(content.transform, i * ButtonWidth - 1);
            }

            var script = buttonGroup.AddComponent<ButtonGroup>();
            script.ScrollButtonLeft = leftArrow;
            script.ScrollButtonRight = rightArrow;
            script.Content = content;

            return buttonGroup;
        }

        /// <summary>
        /// Instantiates a window to show content in.
        /// </summary>
        /// <param name="parent">The UI parent.</param>
        /// <param name="args">The window arguments.</param>
        /// <returns>The window.</returns>
        public static GameObject InstantiateWindow(Transform parent, WindowArgs args)
        {
            var window = new GameObject(args.Name);
            window.transform.SetParent(parent, false);

            var parentRect = parent.gameObject.GetComponent<RectTransform>();
            var windowRect = window.AddComponent<RectTransform>();
            windowRect.pivot = new Vector2(0.5f, 0);
            windowRect.anchorMin = new Vector2(0.5f, 0);
            windowRect.anchorMax = new Vector2(0.5f, 0);
            windowRect.sizeDelta = new Vector2(500, 350);
            windowRect.anchoredPosition = new Vector2(args.PosX, parentRect.rect.height);

            var windowImage = window.AddComponent<Image>();
            windowImage.sprite = args.BackgroundImage;

            window.SetActive(false);

            return window;
        }
    }
}
