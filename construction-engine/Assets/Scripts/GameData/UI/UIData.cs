using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    /// <summary>Root element for UI game data.</summary>
    [XmlRoot("UIData")]
    public class UIData
    {
        [XmlElement("Configuration")]
        public UIConfig Config { get; set; }

        [XmlArray("ButtonGroups")]
        [XmlArrayItem(ElementName = "MainButtonGroup", Type = typeof(MainButtonGroupData))]
        [XmlArrayItem(ElementName = "SubButtonGroup", Type = typeof(SubButtonGroupData))]
        public List<ButtonGroupData> ButtonGroups { get; set; }

        [XmlArray("Windows")]
        [XmlArrayItem("Window")]
        public List<WindowData> Windows { get; set; }
    }

    /// <summary>Configuration values for the UI.</summary>
    public class UIConfig
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
            set { MainMenuBackgroundColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> MainMenuBackgroundColor { get; set; }

        [XmlElement("MainMenuSelected")]
        public string MainMenuSelected
        {
            get { return MainMenuSelectedColor.Key; }
            set { MainMenuSelectedColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> MainMenuSelectedColor { get; set; }

        [XmlElement("MainMenuAccent")]
        public string MainMenuAccent
        {
            get { return MainMenuAccentColor.Key; }
            set { MainMenuAccentColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> MainMenuAccentColor { get; set; }

        [XmlElement("SubMenuBackground")]
        public string SubMenuBackground
        {
            get { return SubMenuBackgroundColor.Key; }
            set { SubMenuBackgroundColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> SubMenuBackgroundColor { get; set; }

        [XmlElement("SubMenuSelected")]
        public string SubMenuSelected
        {
            get { return SubMenuSelectedColor.Key; }
            set { SubMenuSelectedColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> SubMenuSelectedColor { get; set; }

        [XmlElement("SubMenuAccent")]
        public string SubMenuAccent
        {
            get { return SubMenuAccentColor.Key; }
            set { SubMenuAccentColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> SubMenuAccentColor { get; set; }

        [XmlElement("WindowBackground")]
        public string WindowBackground
        {
            get { return WindowBackgroundColor.Key; }
            set { WindowBackgroundColor = new KeyValuePair<string, Color>(value, ColorPalette.GetColor(value)); }
        }

        [XmlIgnore()]
        public KeyValuePair<string, Color> WindowBackgroundColor { get; set; }
    }
}
