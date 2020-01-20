using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    public class BuildingData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("ConstructionCost")]
        public int ConstructionCost { get; set; }

        [XmlElement("MaintenanceCost")]
        public int MaintenanceCost { get; set; }

        [XmlElement("SmallClassrooms")]
        public int SmallClassrooms { get; set; }

        [XmlElement("MediumClassrooms")]
        public int MediumClassrooms { get; set; }

        [XmlElement("LargeClassrooms")]
        public int LargeClassrooms { get; set; }

        [XmlElement("Laboratories")]
        public int Laboratories { get; set; }

        [XmlElement("Icon")]
        public string IconName { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Buildings, nameof(IconName))]
        public Sprite Icon { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Model")]
        public string ModelName { get; set; }

        [ResourceLoader(ResourceType.Models, ResourceCategory.Buildings, nameof(ModelName))]
        public GameObject Model { get; set; }

        [XmlIgnore]
        public bool[,] Footprint { get; set; }
    }
}
