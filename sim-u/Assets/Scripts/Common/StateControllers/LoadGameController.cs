using GameData;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

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

                using (Stream fout = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var bf = new BinaryFormatter();
                    GameSaveState state = (GameSaveState)bf.Deserialize(fout);

                    // Accessor.Game.LoadGame(state);
                }
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
