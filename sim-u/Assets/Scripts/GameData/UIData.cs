using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// Root element for UI game data.
    /// </summary>
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

    /// <summary>
    /// Configuration values for the UI.
    /// </summary>
    public class UIConfig
    {
        [ResourceLoader(ResourceType.Materials, ResourceCategory.Toolbar, resourceName: "arrow-left")]
        public Sprite ArrowLeftSprite { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Toolbar, resourceName: "arrow-right")]
        public Sprite ArrowRightSprite { get; set; }

        [XmlElement("HorizontalMargins")]
        public float HorizontalMargins { get; set; }

        [XmlElement("ButtonWidth")]
        public float ButtonWidth { get; set; }

        [XmlElement("ButtonHeight")]
        public float ButtonHeight { get; set; }

        [XmlElement("MainMenuBackground")]
        public string MainMenuBackground { get; set; }

        [ColorPalette(nameof(MainMenuBackground))]
        public Color MainMenuBackgroundColor { get; set; }

        [XmlElement("MainMenuSelected")]
        public string MainMenuSelected { get; set; }

        [ColorPalette(nameof(MainMenuSelected))]
        public Color MainMenuSelectedColor { get; set; }

        [XmlElement("MainMenuAccent")]
        public string MainMenuAccent { get; set; }

        [ColorPalette(nameof(MainMenuAccent))]
        public Color MainMenuAccentColor { get; set; }

        [XmlElement("SubMenuBackground")]
        public string SubMenuBackground { get; set; }

        [ColorPalette(nameof(SubMenuBackground))]
        public Color SubMenuBackgroundColor { get; set; }

        [XmlElement("SubMenuSelected")]
        public string SubMenuSelected { get; set; }

        [ColorPalette(nameof(SubMenuSelected))]
        public Color SubMenuSelectedColor { get; set; }

        [XmlElement("SubMenuAccent")]
        public string SubMenuAccent { get; set; }

        [ColorPalette(nameof(SubMenuAccent))]
        public Color SubMenuAccentColor { get; set; }

        [XmlElement("WindowBackground")]
        public string WindowBackground { get; set; }

        [ColorPalette(nameof(WindowBackground))]
        public Color WindowBackgroundColor { get; set; }
    }
}
