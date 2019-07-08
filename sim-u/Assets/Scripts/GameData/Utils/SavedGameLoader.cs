using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// Little hack to wire up saved game state into the GameData code path.
    /// </summary>
    public static class SavedGameLoader
    {
        private static readonly string FileExtension = ".simu";
        private static readonly string SaveGameDirectory = Path.Combine(Application.persistentDataPath, "Saved Games");

        public static IEnumerable<string> GetSaveGames()
        {
            Directory.CreateDirectory(SaveGameDirectory);

            return Directory.GetFiles(SaveGameDirectory, "*" + FileExtension)
                .Select(fileName => Path.GetFileNameWithoutExtension(fileName));
        }

        public static void WriteToDisk(string fileName, GameSaveState state)
        {
            Directory.CreateDirectory(SaveGameDirectory);

            string path = Path.Combine(SaveGameDirectory, fileName + FileExtension);
            using (Stream fout = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fout, state);
            }
        }

        public static void DeleteFromDisk(string fileName)
        {
            Directory.CreateDirectory(SaveGameDirectory);

            string path = Path.Combine(SaveGameDirectory, fileName + FileExtension);
            if (File.Exists(path))
            {
                GameLogger.Info("Deleting save at path {0}.", path);
                File.Delete(path);
            }
        }

        public static bool TryReadFromDisk(string fileName, out GameSaveState state)
        {
            Directory.CreateDirectory(SaveGameDirectory);

            string path = Path.Combine(SaveGameDirectory, fileName + FileExtension);
            if (!File.Exists(path))
            {
                GameLogger.Error("Tried to load game save from non-existant file. Path = {0}", path);
                state = null;
                return false;
            }

            using (Stream fout = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bf = new BinaryFormatter();
                state = (GameSaveState)bf.Deserialize(fout);

                if (state.Version != GameSaveState.CurrentVersion)
                {
                    GameLogger.Error("Tried to load an invalid game save version. Version = {0}; Current Version = {1};", state.Version, GameSaveState.CurrentVersion);
                    return false;
                }

                GameLogger.Info("Successfully loaded game state from file '{0}'. Save Version = {1}.", path, state.Version);
            }

            return true;
        }
    }

    public class SavedGameLoaderAttribute : XmlIgnoreAttribute
    {
    }
}
