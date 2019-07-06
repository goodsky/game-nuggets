using GameData;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// Game Controller that runs during the SaveGame state.
    /// </summary>
    internal class SaveGameController : GameStateMachine.Controller
    {
        public SaveGameController()
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

            string path = EditorUtility.SaveFilePanel("Save Game", saveGameDirectory, "saved-game", "svg");

            if (!string.IsNullOrEmpty(path))
            {
                GameLogger.Info("Saving game at '{0}'.", path);

                GameSaveState state = new GameSaveState
                {
                    Version = GameSaveState.CurrentVersion,
                    Campus = Accessor.CampusManager.SaveGameState(),
                };
                using (Stream fout = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(fout, state);
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
