using System;

namespace Simulation
{
    [Serializable]
    public class GraduationResults
    {
        public SimulationDate GraduationDate { get; set; }

        public StudentHistogram GraduatedStudents { get; set; }

        public StudentHistogram DropOuts { get; set; }
    }
}
