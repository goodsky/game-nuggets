using System;

namespace GameData
{
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
