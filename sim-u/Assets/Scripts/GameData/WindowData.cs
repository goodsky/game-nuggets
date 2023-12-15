using System.Xml.Serialization;

namespace GameData
{
    /// <summary>Windows.</summary>
    public class WindowData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("prefab")]
        public string PrefabName { get; set; }

        [XmlAttribute("fullScreen")]
        public bool FullScreen { get; set; }
    }
}
