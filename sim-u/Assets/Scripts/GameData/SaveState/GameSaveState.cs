using System;

namespace GameData
{
    /// <summary>
    /// Metadata to a <see cref="GameSaveState"/>.
    /// The path to the file to load from disk.
    /// </summary>
    public class GameSaveStateMetadata
    {
        public GameSaveStateMetadata(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }

    [Serializable]
    public class GameSaveState
    {
        public static readonly int CurrentVersion = 0;

        public int Version { get; set; }

        public CampusSaveState Campus { get; set; }

        public FacultySaveState Faculty { get; set; }

        public SimulationSaveState Simulation { get; set; }
    }
}
