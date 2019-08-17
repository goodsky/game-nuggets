using Simulation;
using System;

namespace GameData
{
    [Serializable]
    public class SimulationSaveState
    {
        public SimulationSpeed SavedSpeed { get; set; }

        public SimulationDate SavedDate { get; set; }
    }
}
