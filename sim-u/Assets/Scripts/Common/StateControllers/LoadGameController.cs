using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    /// <summary>
    /// Game Controller that runs during the LoadGame state.
    /// </summary>
    internal class LoadGameController : GameStateMachine.Controller
    {
        public LoadGameController()
        {
        }

        /// <summary>
        /// Called by Game State Machine.
        /// </summary>
        /// <param name="_">Not used.</param>
        public override void TransitionIn(object _)
        {
            string saveGameDirectory = Path.Combine(Application.persistentDataPath, "Saved Games");
            Directory.CreateDirectory(saveGameDirectory);

            string path = EditorUtility.OpenFilePanelWithFilters("Save Game", saveGameDirectory, new[] { "Saved Game File", "svg" });

            if (!string.IsNullOrEmpty(path))
            {
                GameLogger.Info("Loading game from '{0}'.", path);
                Game.SavedGamePath = path;

                // Reload the game scene.
                SceneManager.LoadScene(Constant.GameSceneName);
            }

            Accessor.StateMachine.StopDoing();
        }

        /// <summary>
        /// Called by Game State Manager.
        /// </summary>
        public override void TransitionOut()
        {
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update()
        {
        }
    }
}
