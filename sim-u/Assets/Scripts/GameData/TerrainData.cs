using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    public class TerrainData
    {
        [XmlElement("TerrainMaterial")]
        public string TerrainMaterialName { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Terrain, nameof(TerrainMaterialName))]
        public Material TerrainMaterial { get; set; }

        [XmlElement("SubmaterialEmptyGrassIndex")]
        public int SubmaterialEmptyGrassIndex { get; set; }

        [XmlElement("SubmaterialInvalidIndex")]
        public int SubmaterialInvalidIndex { get; set; }

        [XmlElement("SubmaterialPathsIndex")]
        public int SubmaterialPathsIndex { get; set; }

        [XmlElement("SubmaterialRoadsIndex")]
        public int SubmaterialRoadsIndex { get; set; }

        [XmlElement("SubmaterialParkingLotsIndex")]
        public int SubmaterialParkingLotsIndex { get; set; }

        [XmlElement("DefaultGridCountX")]
        public int DefaultGridCountX { get; set; }

        [XmlElement("DefaultGridCountZ")]
        public int DefaultGridCountZ { get; set; }

        [XmlElement("DefaultGridCountY")]
        public int DefaultGridCountY { get; set; }

        [XmlElement("DefaultMaxDepth")]
        public int DefaultMaxDepth { get; set; }
    }
}
