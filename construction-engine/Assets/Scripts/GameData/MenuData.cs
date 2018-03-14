using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameData
{
    /// <summary>Root element for game menu data.</summary>
    [XmlRoot("MenuData")]
    public class MenuData
    {
        [XmlElement("Configuration")]
        public MenuConfig Config { get; set; }

        [XmlArray("ButtonGroups")]
        [XmlArrayItem(ElementName = "MainButtonGroup", Type = typeof(MainButtonGroup))]
        [XmlArrayItem(ElementName = "SubButtonGroup", Type = typeof(SubButtonGroup))]
        public List<ButtonGroup> ButtonGroups { get; set; }
    }

    /// <summary>Configuration values for the menu.</summary>
    public class MenuConfig
    {
        [XmlElement("HorizonalMargins")]
        public float HorizontalMargins { get; set; }

        [XmlElement("ButtonWidth")]
        public float ButtonWidth { get; set; }

        [XmlElement("ButtonHeight")]
        public float ButtonHeight { get; set; }
    }

    /// <summary>A scrollable group of buttons loaded into a toolbar.</summary>
    public abstract class ButtonGroup
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("Buttons")]
        [XmlArrayItem("Button")]
        public List<Button> Buttons { get; set; }
    }

    /// <summary>Indicates a button group on the Main Menu</summary>
    public class MainButtonGroup { }

    /// <summary>Indicates a button group on the Sub Menu</summary>
    public class SubButtonGroup { }

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
    [XmlInclude(typeof(PopUpSubMenuAction))]
    [XmlInclude(typeof(PopDownSubMenuAction))]
    [XmlInclude(typeof(PopUpWindowAction))]
    [XmlInclude(typeof(PopDownWindowAction))]
    public abstract class ButtonAction { }

    /// <summary>Action to pop up the sub menu.</summary>
    public class PopUpSubMenuAction : ButtonAction
    {
        [XmlAttribute("buttonGroup")]
        public string ButtonGroupName { get; set; }
    }

    /// <summary>Action to pop down the active window.</summary>
    public class PopDownSubMenuAction : ButtonAction { }

    /// <summary>Action to pop up a window.</summary>
    public class PopUpWindowAction : ButtonAction
    {
        [XmlAttribute("windowName")]
        public string WindowName { get; set; }

        [XmlElement("Arguments")]
        public string Args { get; set; }
    }

    /// <summary>Action to pop down the active window.</summary>
    public class PopDownWindowAction : ButtonAction { }
}
