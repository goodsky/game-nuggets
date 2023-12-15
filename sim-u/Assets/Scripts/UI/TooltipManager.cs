﻿using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// In charge of placing the tooltip around on the screen.
    /// </summary>
    public static class TooltipManager
    {
        private static GameObject Tooltip;

        /// <summary>
        /// Initialize the tooltip manager
        /// </summary>
        /// <param name="canvas"></param>
        public static void Initialize(Transform canvas)
        {
            Tooltip = UIFactory.LoadTooltip(canvas);
            Tooltip.SetActive(false);
        }

        /// <summary>
        /// Pop up the tooltip at the mouse's location.
        /// </summary>
        /// <param name="text">Text to display.</param>
        public static void PopUp(string text)
        {
            if (Tooltip == null)
                return;

            var textComponent = Tooltip.GetComponentInChildren<Text>();
            textComponent.text = text;

            var rectComponent = Tooltip.GetComponent<RectTransform>();

            int deltaY;
            if (Input.mousePosition.y < Screen.height / 2)
            {
                deltaY = 25;
            }
            else
            {
                deltaY = -20;
            }

            rectComponent.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y + deltaY);

            Tooltip.transform.SetAsLastSibling();

            Tooltip.SetActive(true);
        }

        /// <summary>
        /// Clear the tooltip.
        /// </summary>
        public static void PopDown()
        {
            if (Tooltip == null)
                return;

            Tooltip.SetActive(false);
        }
    }
}
