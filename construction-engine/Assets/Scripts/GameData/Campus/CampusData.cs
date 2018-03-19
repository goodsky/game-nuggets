using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameData
{
    /// <summary>Root element for Campus game data.</summary>
    [XmlRoot("CampusData")]
    public class CampusData
    {
        [XmlArray("Buildings")]
        [XmlArrayItem("Building")]
        public List<BuildingData> Buildings { get; set; }
    }
}
