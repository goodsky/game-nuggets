using GameData;
using System;

namespace Simulation
{
    [Serializable]
    public class SimulationScore
    {
        public SimulationScore() { }

        public SimulationScore(SimulationData data)
        {
            AcademicScore = data.UniversityAcademicScore.DefaultValue;
            ResearchScore = data.UniversityResearchScore.DefaultValue;
            AcademicPrestige = data.ResearchPrestige.DefaultValue;
            ResearchPrestige = data.ResearchPrestige.DefaultValue;
            Popularity = data.Popularity.DefaultValue;
        }

        /// <summary>
        /// Sum of the academic points over the entire playthrough.
        /// </summary>
        public int AcademicScore { get; private set; }

        /// <summary>
        /// Sum of the research points over the entire playthrough.
        /// </summary>
        public int ResearchScore { get; private set; }

        /// <summary>
        /// Indicator of current academic performance.
        /// Value: [0, 100]
        /// </summary>
        public int AcademicPrestige { get; private set; }

        /// <summary>
        /// Indicator of current research performance.
        /// Value: [0, 100]
        /// </summary>
        public int ResearchPrestige { get; private set; }

        /// <summary>
        /// Indicator of current university popularity.
        /// Value: [0, 100]
        /// </summary>
        public int Popularity { get; private set; }
    }
}
