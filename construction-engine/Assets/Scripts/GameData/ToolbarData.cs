using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    /// <summary>Root element for toolbar game data.</summary>
    [XmlRoot("ToolbarData")]
    public class ToolbarData
    {
        [XmlElement("Configuration")]
        public ToolbarConfig Config { get; set; }

        [XmlArray("ButtonGroups")]
        [XmlArrayItem(ElementName = "MainButtonGroup", Type = typeof(MainButtonGroupData))]
        [XmlArrayItem(ElementName = "SubButtonGroup", Type = typeof(SubButtonGroupData))]
        public List<ButtonGroupData> ButtonGroups { get; set; }
    }

    /// <summary>Configuration values for the toolbar.</summary>
    public class ToolbarConfig
    {
        [XmlElement("HorizontalMargins")]
        public float HorizontalMargins { get; set; }

        [XmlElement("ButtonWidth")]
        public float ButtonWidth { get; set; }

        [XmlElement("ButtonHeight")]
        public float ButtonHeight { get; set; }

        [XmlElement("MainMenuBackground")]
        public string MainMenuBackground
        {
            get { return MainMenuBackgroundColor.Key; }
            set { MainMenuBackgroundColor = new KeyValuePair<string, Color>(value, MyPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> MainMenuBackgroundColor { get; set; }

        [XmlElement("MainMenuSelected")]
        public string MainMenuSelected
        {
            get { return MainMenuSelectedColor.Key; }
            set { MainMenuSelectedColor = new KeyValuePair<string, Color>(value, MyPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> MainMenuSelectedColor { get; set; }

        [XmlElement("MainMenuAccent")]
        public string MainMenuAccent
        {
            get { return MainMenuAccentColor.Key; }
            set { MainMenuAccentColor = new KeyValuePair<string, Color>(value, MyPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> MainMenuAccentColor { get; set; }

        [XmlElement("SubMenuBackground")]
        public string SubMenuBackground
        {
            get { return SubMenuBackgroundColor.Key; }
            set { SubMenuBackgroundColor = new KeyValuePair<string, Color>(value, MyPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> SubMenuBackgroundColor { get; set; }

        [XmlElement("SubMenuSelected")]
        public string SubMenuSelected
        {
            get { return SubMenuSelectedColor.Key; }
            set { SubMenuSelectedColor = new KeyValuePair<string, Color>(value, MyPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> SubMenuSelectedColor { get; set; }

        [XmlElement("SubMenuAccent")]
        public string SubMenuAccent
        {
            get { return SubMenuAccentColor.Key; }
            set { SubMenuAccentColor = new KeyValuePair<string, Color>(value, MyPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> SubMenuAccentColor { get; set; }
    }

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
    public class MainButtonGroupData : ButtonGroupData{ }

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
