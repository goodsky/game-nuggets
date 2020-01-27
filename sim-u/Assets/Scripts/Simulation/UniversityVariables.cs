using GameData;
using System;

namespace Simulation
{
    [Serializable]
    public class UniversityVariables
    {
        public UniversityVariables() { }

        /// <summary>
        /// The current tuition of the university.
        /// </summary>
        public int TuitionPerQuarter { get; set; }

        /// <summary>
        /// The current SAT cutoff of the university.
        /// </summary>
        public int SatCutoff { get; set; }
    }
}
