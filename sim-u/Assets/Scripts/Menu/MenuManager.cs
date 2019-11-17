using Common;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        private readonly Color[] Colors = new Color[]
        {
            Color.blue,
            Color.red,
            Color.green,
            Color.yellow,
            Color.cyan,
            Color.magenta,
        };

        private int _colorIndex = 0;

        public Image ChromaticFunImage;
        public UnityEngine.UI.Button NewGameButton;
        public UnityEngine.UI.Button LoadGameButton;
        public UnityEngine.UI.Button ExitButton;

        public GameObject NewGameMenu;
        public NewGameBox NewGameBox;

        public GameObject LoadGameMenu;
        public LoadGameBox LoadGameBox;

        public void OnClickNewGame()
        {
            NewGameMenu.SetActive(true);
            NewGameBox.Initialize(() => { NewGameMenu.SetActive(false); });
        }

        public void OnClickLoadGame()
        {
            LoadGameMenu.SetActive(true);
            LoadGameBox.Initialize(() => { LoadGameMenu.SetActive(false); });
        }

        public void OnClickExit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Unity start method
        /// </summary>
        protected void Start()
        {
            GameLogger.EnsureSingletonExists();
            GameLogger.Info("Menu loaded.");

            NewGameButton.onClick.AddListener(OnClickNewGame);
            LoadGameButton.onClick.AddListener(OnClickLoadGame);
            ExitButton.onClick.AddListener(OnClickExit);
        }

        /// <summary>
        /// Unity update method
        /// </summary>
        protected void Update()
        {
            // I just want something to be moving on this screen.
            Color current = ChromaticFunImage.color;
            Color desired = Colors[_colorIndex];

            ColorBlock buttonColors = new ColorBlock
            {
                colorMultiplier = 1,
                fadeDuration = 0.1f,
                highlightedColor = current
            };

            NewGameButton.colors = buttonColors;
            LoadGameButton.colors = buttonColors;
            ExitButton.colors = buttonColors;

            const float eps = 0.05f;
            const float t = 0.01f;
            if (Mathf.Abs(current.r - desired.r) < eps &&
                Mathf.Abs(current.g - desired.g) < eps &&
                Mathf.Abs(current.b - desired.b) < eps)
            {
                _colorIndex = (_colorIndex + 1) % Colors.Length;
            }
            else
            {
                ChromaticFunImage.color = new Color(
                    Mathf.Lerp(current.r, desired.r, t),
                    Mathf.Lerp(current.g, desired.g, t),
                    Mathf.Lerp(current.b, desired.b, t));
            }
        }
    }
}
