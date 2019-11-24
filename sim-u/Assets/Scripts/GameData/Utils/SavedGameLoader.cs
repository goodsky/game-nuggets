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

        public static IEnumerable<SavedGame> GetSaveGames()
        {
            Directory.CreateDirectory(SaveGameDirectory);

            return Directory.GetFiles(SaveGameDirectory, "*" + FileExtension)
                .Select(filePath => new SavedGame
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    SavePath = filePath,
                })
                .ToList();
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

        public static void DeleteFromDisk(string path)
        {
            if (File.Exists(path))
            {
                GameLogger.Info("Deleting save at path {0}.", path);
                File.Delete(path);
            }
            else
            {
                GameLogger.Warning("Could not delete save {0}.", path);
            }
        }

        public static bool TryReadFromDisk(string path, out GameSaveState state)
        {
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

        public static bool TryReadFromResources(string path, out GameSaveState state)
        {
            state = null;
            return false;
        }
    }

    public class SavedGame
    {
        public string Name { get; set; }

        public string SavePath { get; set; }
    }

    public class SavedGameLoaderAttribute : XmlIgnoreAttribute
    {
    }
}
