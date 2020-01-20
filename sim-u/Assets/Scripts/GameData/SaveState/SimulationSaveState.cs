using Simulation;
using System;
using System.Collections.Generic;

namespace GameData
{
    [Serializable]
    public class SimulationSaveState
    {
        public SimulationSpeed SavedSpeed { get; set; }

        public bool SavedIsFrozen { get; set; }

        public SimulationDate SavedDate { get; set; }

        public UniversityScore Score { get; set; }

        public UniversityVariables Variables { get; set; }

        public StudentBodySaveState StudentBody { get; set; }
    }

    [Serializable]
    public class StudentBodySaveState
    {
        public StudentHistogram[] ActiveStudents { get; set; }

        public List<GraduationResults> GraduatedStudents { get; set; }
    }
}
