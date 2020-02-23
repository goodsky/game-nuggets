using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    [XmlRoot("FacultyData")]
    public class FacultyData
    {
        [SavedGameLoader]
        public GameSaveState SavedData { get; set; }

        [XmlElement("DefaultHeadshot")]
        public string DefaultHeadshotName { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Faculty, nameof(DefaultHeadshotName))]
        public Sprite DefaultHeadshot { get; set; }

        [XmlElement("TeachingScore")]
        public ScoreDefinition<int> TeachingScore { get; set; }

        [XmlElement("ResearchScore")]
        public ScoreDefinition<int> ResearchScore { get; set; }

        [XmlElement("FacultySlots")]
        public ScoreDefinition<int> FacultySlots { get; set; }

        [XmlElement("MinimumBaseSalary")]
        public int MinimumBaseSalary { get; set; }

        [XmlElement("AverageDollarsPerTeachingPoint")]
        public double AverageDollarsPerTeachingPoint { get; set; }

        [XmlElement("AverageDollarsPerResearchPoint")]
        public double AverageDollarsPerResearchPoint { get; set; }

        [XmlElement("TeachingScoreStdDev")]
        public double TeachingScoreStdDev { get; set; }

        [XmlElement("ResearchScoreStdDev")]
        public double ResearchScoreStdDev { get; set; }

        [XmlElement("MinDefaultTeachingScore")]
        public int MinDefaultTeachingScore { get; set; }

        [XmlElement("MinDefaultResearchScore")]
        public int MinDefaultResearchScore { get; set; }

        [XmlElement("FacultyOverTheMeanPremiumMaxDollars")]
        public int FacultyOverTheMeanPremiumMaxDollars { get; set; }

        [XmlElement("FacultyOverTheMeanPremiumExponentialFactor")]
        public double FacultyOverTheMeanPremiumExponentialFactor { get; set; }

        [XmlElement("ResearchPrestigeImpactOnTeachingScore")]
        public double ResearchPrestigeImpactOnTeachingScore { get; set; }

        [XmlElement("TeachingPrestigeImpactOnResearchScore")]
        public double TeachingPrestigeImpactOnResearchScore { get; set; }

        /// <summary>
        /// This factor says how much better good researchers are than average researchers.
        /// </summary>
        [XmlElement("ResearchExponentialFactor")]
        public double ResearchExponentialFactor { get; set; }

        [XmlElement("AvailableFacultyCount")]
        public int AvailableFacultyCount { get; set; }

        [XmlElement("FirstNamesMen")]
        public NameList FirstNamesMen { get; set; }

        [XmlElement("FirstNamesWomen")]
        public NameList FirstNamesWomen { get; set; }

        [XmlElement("LastNames")]
        public NameList LastNames { get; set; }
    }

    public class NameList
    {
        [XmlElement("Name")]
        public List<string> Names { get; set; }
    }
}
