using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

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

        [XmlIgnore]
        public KeyValuePair<string, Sprite> Icon { get; set; }

        [XmlElement("Icon")]
        public string IconName
        {
            get { return Icon.Key; }
            set { Icon = new KeyValuePair<string, Sprite>(value, Resources.Load<Sprite>(value)); }
        }

        [XmlElement("Description")]
        public string Description { get; set; }
    }
}
