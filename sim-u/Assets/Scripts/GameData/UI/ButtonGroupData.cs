using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    /// <summary>A scrollable group of buttons loaded into a toolbar.</summary>
    public abstract class ButtonGroupData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("Buttons")]
        [XmlArrayItem("Button")]
        public List<ButtonData> Buttons { get; set; }
    }

    /// <summary>Indicates a button group on the Main Toolbar</summary>
    public class MainButtonGroupData : ButtonGroupData { }

    /// <summary>Indicates a button group on the Sub Toolbar</summary>
    public class SubButtonGroupData : ButtonGroupData { }

    /// <summary>A button that will be loaded into a Button Group.</summary>
    public class ButtonData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Tooltip")]
        public string Tooltip { get; set; }

        [XmlElement("IconImage")]
        public string IconImageName { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Toolbar, nameof(IconImageName))]
        public Sprite IconImage { get; set; }

        [XmlElement("OnSelect")]
        public ButtonAction OnSelect { get; set; }

        [XmlElement("OnDeselect")]
        public ButtonAction OnDeselect { get; set; }
    }

    /// <summary>Defines an Action that can be invoked by a Button.</summary>
    [XmlInclude(typeof(OpenSubMenuAction))]
    [XmlInclude(typeof(CloseSubMenuAction))]
    [XmlInclude(typeof(OpenWindowAction))]
    [XmlInclude(typeof(OpenWindowWithDataAction))]
    [XmlInclude(typeof(CloseWindowAction))]
    public abstract class ButtonAction { }

    /// <summary>Action to pop up the sub menu.</summary>
    public class OpenSubMenuAction : ButtonAction
    {
        [XmlAttribute("buttonGroupName")]
        public string ButtonGroupName { get; set; }
    }

    /// <summary>Action to pop down the active window.</summary>
    public class CloseSubMenuAction : ButtonAction { }

    /// <summary>Action to pop up a window.</summary>
    public class OpenWindowAction : ButtonAction
    {
        /// <summary>Name of the window to open.</summary>
        [XmlAttribute("windowName")]
        public string WindowName { get; set; }
    }

    /// <summary>Action to pop up a window.</summary>
    public class OpenWindowWithDataAction : ButtonAction
    {
        /// <summary>Name of the window to open.</summary>
        [XmlAttribute("windowName")]
        public string WindowName { get; set; }

        /// <summary>Type of the game data to pass to the window.</summary>
        [XmlAttribute("dataType")]
        public GameDataType DataType { get; set; }

        /// <summary>Name of the data to pass to the window.</summary>
        [XmlAttribute("data")]
        public string DataName { get; set; }
    }

    /// <summary>Action to pop down the active window.</summary>
    public class CloseWindowAction : ButtonAction { }
}
