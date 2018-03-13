using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace GameData
{
    /// <summary>Root element for game menu data.</summary>
    [XmlRoot("MenuData")]
    public class MenuData
    {
        [XmlArray("ButtonGroups")]
        [XmlArrayItem("ButtonGroup")]
        public List<ButtonGroup> ButtonGroups { get; set; }
    }

    /// <summary>A scrollable group of buttons loaded into a toolbar.</summary>
    public class ButtonGroup
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("menu")]
        public string Menu { get; set; }

        [XmlArray("Buttons")]
        [XmlArrayItem("Button")]
        public List<Button> Buttons { get; set; }
    }

    /// <summary>A button that will be loaded into a Button Group.</summary>
    public class Button
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Tooltip")]
        public string Tooltip { get; set; }

        [XmlElement("IconImage")]
        public string IconImage { get; set; }

        [XmlElement("OnSelect")]
        public ButtonAction OnSelect { get; set; }

        [XmlElement("OnDeselect")]
        public ButtonAction OnDeselect { get; set; }

        [XmlElement("ChildButtonGroup")]
        public string ChildButtonGroup { get; set; }
    }

    /// <summary>Defines an Action that can be invoked by a Button.</summary>
    [XmlInclude(typeof(PopUpWindowAction))]
    [XmlInclude(typeof(PopDownWindowAction))]
    public abstract class ButtonAction { }

    public class PopUpWindowAction : ButtonAction
    {
        [XmlAttribute("windowName")]
        public string WindowName { get; set; }
    }

    public class PopDownWindowAction : ButtonAction
    {

    }
}
