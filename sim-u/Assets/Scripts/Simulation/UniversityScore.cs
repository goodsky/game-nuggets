using GameData;
using System;

namespace Simulation
{
    public enum ScoreTrend
    {
        Neutral,
        Up,
        Down
    }

    [Serializable]
    public class UniversityScore
    {
        public UniversityScore() { }

        public UniversityScore(SimulationData data)
        {
            AcademicScore = data.UniversityAcademicScore.DefaultValue;
            ResearchScore = data.UniversityResearchScore.DefaultValue;
            AcademicPrestige = data.ResearchPrestige.DefaultValue;
            ResearchPrestige = data.ResearchPrestige.DefaultValue;
            Popularity = data.Popularity.DefaultValue;
            Money = data.StartingMoney;
        }

        /// <summary>
        /// Sum of the academic points over the entire playthrough.
        /// </summary>
        public int AcademicScore { get; set; }

        /// <summary>
        /// Sum of the research points over the entire playthrough.
        /// </summary>
        public int ResearchScore { get; set; }

        /// <summary>
        /// Indicator of current academic performance.
        /// Value: [0, 100]
        /// </summary>
        public int AcademicPrestige { get; set; }

        /// <summary>
        /// Direction that AP is trending
        /// </summary>
        public ScoreTrend AcademicPrestigeTrend { get; set; }

        /// <summary>
        /// Indicator of current research performance.
        /// Value: [0, 100]
        /// </summary>
        public double ResearchPrestige { get; set; }

        /// <summary>
        /// Direction that RP is trending.
        /// </summary>
        public ScoreTrend ResearchPrestigeTrend { get; set; }

        /// <summary>
        /// Indicator of current university popularity.
        /// Value: [0, 100]
        /// </summary>
        public double Popularity { get; set; }

        /// <summary>
        /// Direction that popularity is trending.
        /// </summary>
        public ScoreTrend PopularityTrend { get; set; }

        /// <summary>
        /// Amount of money the University currently has.
        /// </summary>
        public int Money { get; set; }
    }
}
