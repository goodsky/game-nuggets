using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the New Construction UI window.
    /// </summary>
    public class ConstructionInfoWindow : Window
    {
        private string _title;
        private string _description;

        /// <summary>
        /// The building title text.
        /// </summary>
        public Text TitleText;

        /// <summary>
        /// The building image.
        /// </summary>
        public Image ConstructionImage;

        /// <summary>
        /// The building description text.
        /// </summary>
        public Text DescriptionText;

        /// <summary>
        /// The build button.
        /// </summary>
        public Button BuildButton;

        /// <summary>
        /// The build button.
        /// </summary>
        public Button CancelButton;

        /// <summary>
        /// A view on the construction window title text.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                TitleText.text = _title;
            }
        }

        /// <summary>
        /// A view on the construction window description text.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                DescriptionText.text = _description;
            }
        }

        /// <summary>Gets the UI Buttons on this window.</summary>
        public override List<Button> Buttons
        {
            get { return new List<Button>() { BuildButton, CancelButton }; }
        }

        /// <summary>
        /// Open the window to display the game data.
        /// </summary>
        /// <param name="data">The game data</param>
        public override void Open(object data)
        {
            var buildingData = data as BuildingData;
            if (buildingData == null)
            {
                GameLogger.FatalError("ConstructionInfoWindow was passed invalid data. Data = {0}", data == null ? "null" : data.GetType().Name);
            }

            Title = buildingData.Name;
            Description = WriteDescription(buildingData);
            ConstructionImage.sprite = buildingData.Icon;

            BuildButton.OnSelect = () => { Accessor.UiManager.OpenWindow("ConstructionPlacing", buildingData); };
            CancelButton.OnSelect = () => { SelectionManager.UpdateSelection(SelectionParent); };
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        public override void Close()
        {
            
        }

        /// <summary>
        /// Write the building description to display.
        /// </summary>
        /// <param name="data">The game data.</param>
        /// <returns>A string summary of the building.</returns>
        private string WriteDescription(BuildingData data)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Cost:</b> {0:C0}{1}", data.ConstructionCost, Environment.NewLine);

            if (data.MaintenanceCost != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Utilities:</b> {0:C0} per year{1}", data.MaintenanceCost, Environment.NewLine);

            if (data.Classrooms != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Classrooms:</b> {0}{1}", data.Classrooms, Environment.NewLine);

            sb.AppendLine();

            sb.AppendLine(data.Description);

            return sb.ToString();
        }
    }
}
