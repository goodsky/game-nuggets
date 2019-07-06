using System;

namespace GameData
{
    [Serializable]
    public class GameSaveState
    {
        public static readonly int CurrentVersion = 0;

        public int Version { get; set; }

        public CampusSaveState Campus { get; set; }
    }
}
