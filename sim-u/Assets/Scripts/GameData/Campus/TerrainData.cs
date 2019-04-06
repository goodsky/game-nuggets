using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    public class TerrainData
    {
        [XmlIgnore]
        public KeyValuePair<string, Material> TerrainMaterial { get; set; }

        [XmlElement("TerrainMaterial")]
        public string TerrainMaterialName
        {
            get { return TerrainMaterial.Key; }
            set { TerrainMaterial = new KeyValuePair<string, Material>(value, Resources.Load<Material>(value)); }
        }

        [XmlElement("GridCountX")]
        public int GridCountX { get; set; }

        [XmlElement("GridCountZ")]
        public int GridCountZ { get; set; }

        [XmlElement("GridCountY")]
        public int GridCountY { get; set; }

        [XmlElement("StartingHeight")]
        public int StartingHeight { get; set; }
    }
}
