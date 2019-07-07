using Common;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace GameData
{
    /// <summary>
    /// Little hack to wire up saved game state into the GameData code path.
    /// </summary>
    public static class SavedGameLoader
    {
        public static void WriteToDisk(string path, GameSaveState state)
        {
            using (Stream fout = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fout, state);
            }
        }

        public static bool TryReadFromDisk(string path, out GameSaveState state)
        {
            state = null;

            if (!File.Exists(path))
            {
                GameLogger.Error("Tried to load game save from non-existant file. Path = {0}", path);
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
