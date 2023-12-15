using System.Xml.Serialization;

namespace GameData
{
    public class ScoreDefinition<T>
    {
        [XmlAttribute("min")]
        public T MinValue { get; set; }

        [XmlAttribute("max")]
        public T MaxValue { get; set; }

        [XmlAttribute("default")]
        public T DefaultValue { get; set; }
    }
}
