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
    }
}
