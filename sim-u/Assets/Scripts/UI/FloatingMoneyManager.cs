using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// In charge of placing the floating money UI effects on the screen.
    /// </summary>
    public static class FloatingMoneyManager
    {
        private static Transform Canvas;

        /// <summary>
        /// Initialize the floating money manager
        /// </summary>
        /// <param name="canvas"></param>
        public static void Initialize(Transform canvas)
        {
            Canvas = canvas;
        }

        /// <summary>
        /// Spawn a new floating money at the mouse's location.
        /// </summary>
        /// <param name="cost">Amount of money spent.</param>
        public static void Spawn(int cost)
        {
            var floatingMoney = UIFactory.LoadFloatingMoney(Canvas);

            var textComponent = floatingMoney.GetComponentInChildren<Text>();
            textComponent.text = string.Format(CultureInfo.CurrentCulture, "${0:n0}", -cost);

            var rectComponent = floatingMoney.GetComponent<RectTransform>();
            rectComponent.anchoredPosition = new Vector2(
                Input.mousePosition.x - (Screen.width / 2),
                Input.mousePosition.y - (Screen.height / 2));

            floatingMoney.transform.SetAsLastSibling();
            floatingMoney.SetActive(true);
        }
    }
}
