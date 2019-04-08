using System.Collections.Generic;
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

        [XmlElement("Classrooms")]
        public int Classrooms { get; set; }

        [XmlElement("Icon")]
        public string IconName { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Buildings, nameof(IconName))]
        public Sprite Icon { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Mesh")]
        public string MeshName { get; set; }

        [ResourceLoader(ResourceType.Models, ResourceCategory.Buildings, nameof(MeshName))]
        public Mesh Mesh { get; set; }

        [XmlElement("Material")]
        public string MaterialName { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Buildings, nameof(MaterialName))]
        public Material Material { get; set; }

        [XmlIgnore]
        public bool[,] Footprint { get; set; }
    }
}
