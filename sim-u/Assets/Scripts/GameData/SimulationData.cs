using Simulation;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameData
{
    /// <summary>
    /// I've made an effort to comment some of the more obtuse factors here...
    /// but it's very possible that not all of these variables are intuitive.
    /// Oh well. Do your best? Sorry.
    /// </summary>
    [XmlRoot("SimulationData")]
    public class SimulationData
    {
        [SavedGameLoader]
        public GameSaveState SavedData { get; set; }

        [XmlElement("TickRateInSeconds")]
        public float TickRateInSeconds { get; set; }

        [XmlElement("NormalTicksPerWeek")]
        public int NormalTicksPerWeek { get; set; }

        [XmlElement("FastSpeedTicksPerWeek")]
        public int FastSpeedTicksPerWeek { get; set; }

        [XmlElement("UniversityAcademicScore")]
        public ScoreDefinition<int> UniversityAcademicScore { get; set; }

        [XmlElement("UniversityResearchScore")]
        public ScoreDefinition<int> UniversityResearchScore { get; set; }

        [XmlElement("AcademicPrestige")]
        public ScoreDefinition<int> AcademicPrestige { get; set; }

        [XmlElement("ResearchPrestige")]
        public ScoreDefinition<int> ResearchPrestige { get; set; }

        [XmlElement("Popularity")]
        public ScoreDefinition<int> Popularity { get; set; }

        [XmlElement("StartingMoney")]
        public int StartingMoney { get; set; }

        /// <summary>
        /// The graduation rate buckets. Split by graduation year and academic scores.
        /// </summary>
        [XmlElement("GraduationRateBucket")]
        public List<GraduationRateBucket> GraduationRateBuckets { get; set; }

        /// <summary>
        /// Range of possible tuition values you can set.
        /// </summary>
        [XmlElement("TuitionRange")]
        public ScoreDefinition<int> TuitionRange { get; set; }

        /// <summary>
        /// Exponent in target tuition calculation.
        /// </summary>
        [XmlElement("TuitionRangeExponentialFactor")]
        public double TuitionRangeExponentialFactor { get; set; }

        /// <summary>
        /// The bonus based on how cheap (or expensive) your tuition is set.
        /// </summary>
        [XmlElement("TuitionBonus")]
        public ScoreDefinition<int> TuitionBonus { get; set; }

        /// <summary>
        /// A sigmoid dampener used to calculate positive tuition bonus on popularity.
        /// Used while calculating enrollment population size.
        /// </summary>
        [XmlElement("TuitionBonus90PercentileRange")]
        public double TuitionBonus90PercentileRange { get; set; }

        /// <summary>
        /// A linear penalty of 1 popularity point per $X over-setting tuition.
        /// Used while calculating enrollment population size.
        /// </summary>
        [XmlElement("TuitionBonusLinearFactor")]
        public double TuitionBonusLinearFactor { get; set; }

        /// <summary>
        /// The size of the population generated each academic year.
        /// </summary>
        [XmlElement("EnrollingPopulationSize")]
        public ScoreDefinition<int> EnrollingPopulationSize { get; set; }

        /// <summary>
        /// Exponent in enrolling student population size calculation.
        /// </summary>
        [XmlElement("EnrollingPopulationSizeExponentialFactor")]
        public double EnrollingPopulationSizeExponentialFactor { get; set; }

        /// <summary>
        /// Standard Deviation of normally distributed academic scores of enrolling students.
        /// Remember: 68% of all the population will be within 1 standard deviation of the mean.
        /// Linearly maps to SAT scores of the students.
        /// </summary>
        [XmlElement("EnrollingPopulationAcademicScoreStdDev")]
        public double EnrollingPopulationAcademicScoreStdDev { get; set; }

        /// <summary>
        /// Individual academic score of students.
        /// </summary>
        [XmlElement("StudentAcademicScore")]
        public ScoreDefinition<int> StudentAcademicScore { get; set; }

        /// <summary>
        /// Students' academic score range is larger than the max SAT score.
        /// This sets the maximum academic score of a student while enrolling.
        /// </summary>
        [XmlElement("MaxStudentAcademicScoreDuringEnrollment")]
        public int MaxStudentAcademicScoreDuringEnrollment { get; set; }

        /// <summary>
        /// Mapping individual academic scores to SAT scores.
        /// Currently just used to filter based on academics during enrollment.
        /// </summary>
        [XmlElement("StudentSATScore")]
        public ScoreDefinition<int> StudentSATScore { get; set; }

        /// <summary>
        /// The minimum number of years to look back while calculating academic prestige.
        /// </summary>
        [XmlElement("AcademicPrestigeLookBackYears")]
        public int AcademicPrestigeLookBackYears { get; set; }

        /// <summary>
        /// The minimum number of students to look back while calculating academic prestige.
        /// </summary>
        [XmlElement("AcademicPrestigeLookBackStudentCount")]
        public int AcademicPrestigeLookBackStudentCount { get; set; }
    }

    /// <summary>
    /// Represents the graduation or drop out rate for a bucket of students.
    /// E.G. For students with academic scores between [LowerBound, UpperBound):
    ///     Total * Rate will graduate/dropout at the end of a year.
    /// </summary>
    public class GraduationRateBucket
    {
        [XmlAttribute("year")]
        public StudentBodyYear Year { get; set; }

        [XmlAttribute("lowScore")]
        public int LowerBoundAcademicScore { get; set; }

        [XmlAttribute("highScore")]
        public int UpperBoundAcademicScore { get; set; }

        [XmlAttribute("graduationRate")]
        public double GraduationRate { get; set; }

        [XmlAttribute("dropoutRate")]
        public double DropoutRate { get; set; }
    }
}
