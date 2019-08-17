using System.Xml.Serialization;

namespace GameData
{
    [XmlRoot("SimulationData")]
    public class SimulationData
    {
        [SavedGameLoader]
        public GameSaveState SavedData { get; set; }

        [XmlElement("TickRateInSeconds")]
        public float TickRateInSeconds { get; set; }

        [XmlElement("NormalTicksPerWeek")]
        public int NormalTicksPerWeek { get; set; }

        [XmlElement("FastSpeedTicksPerWeek")]
        public int FastSpeedTicksPerWeek { get; set; }
    }
}
