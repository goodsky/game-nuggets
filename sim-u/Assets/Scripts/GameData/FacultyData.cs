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
    }
}
