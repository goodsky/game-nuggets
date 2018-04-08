using System.Xml.Serialization;

namespace GameData
{
    public class TerrainData
    {
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
