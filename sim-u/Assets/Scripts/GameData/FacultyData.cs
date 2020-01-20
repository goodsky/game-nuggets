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
