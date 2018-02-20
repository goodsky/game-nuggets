using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public static class TooltipManager
    {
        private static GameObject Tooltip;

        public static void Initialize(Transform canvas)
        {
            Tooltip = ToolbarFactory.InstantiateTooltip(canvas);
            Tooltip.SetActive(false);
        }

        public static void PopUp(string text)
        {
            var textComponent = Tooltip.GetComponentInChildren<Text>();
            textComponent.text = text;

            var rectComponent = Tooltip.GetComponent<RectTransform>();
            rectComponent.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y - 20);

            Tooltip.SetActive(true);
        }

        public static void PopDown()
        {
            Tooltip.SetActive(false);
        }
    }
}
