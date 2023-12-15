using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameData
{
    /// <summary>
    /// Root element for Campus game data.
    /// </summary>
    [XmlRoot("CampusData")]
    public class CampusData
    {
        [SavedGameLoader]
        public GameSaveState SavedData { get; set; }

        [XmlElement("Terrain")]
        public TerrainData Terrain { get; set; }

        [XmlElement("Paths")]
        public PathsData Paths { get; set; }

        [XmlElement("Roads")]
        public RoadsData Roads { get; set; }

        [XmlElement("ParkingLots")]
        public ParkingLotData ParkingLots { get; set; }

        [XmlElement("SmallClassroomCapacity")]
        public int SmallClassroomCapacity { get; set; }

        [XmlElement("MediumClassroomCapacity")]
        public int MediumClassroomCapacity { get; set; }

        [XmlElement("LargeClassroomCapacity")]
        public int LargeClassroomCapacity { get; set; }

        [XmlArray("Buildings")]
        [XmlArrayItem("Building")]
        public List<BuildingData> Buildings { get; set; }
    }

    public class PathsData
    {
        [XmlElement("CostPerSquare")]
        public int CostPerSquare{ get; set; }
    }

    public class RoadsData
    {
        [XmlElement("CostPerSquare")]
        public int CostPerSquare { get; set; }
    }

    public class ParkingLotData
    {
        [XmlElement("CostPerSquare")]
        public int CostPerSquare { get; set; }
    }
}
