using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the New Construction UI window.
    /// </summary>
    public class ConstructionInfoWindow : Window
    {
        private BuildingData _bulidingData;
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

            _bulidingData = buildingData;
            Title = buildingData.Name;
            Description = WriteDescription(buildingData);
            ConstructionImage.sprite = buildingData.Icon;

            BuildButton.OnSelect = () => { Accessor.UiManager.OpenWindow("ConstructionPlacing", buildingData); };
            CancelButton.OnSelect = () => { SelectionManager.UpdateSelection(SelectionParent); };

            Accessor.Camera.FreezeCamera();
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        public override void Close()
        {
            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            Accessor.Camera?.UnfreezeCamera();
        }

        /// <summary>
        /// Unity update method
        /// </summary>
        protected override void Update()
        {
            // Make sure the building can be built
            if (Accessor.Simulation.CanPurchase(_bulidingData.ConstructionCost))
            {
                BuildButton.Enable();
            }
            else
            {
                BuildButton.Disable();
            }

            base.Update();
        }

        /// <summary>
        /// Write the building description to display.
        /// </summary>
        /// <param name="data">The game data.</param>
        /// <returns>A string summary of the building.</returns>
        private string WriteDescription(BuildingData data)
        {
            StringBuilder sb = new StringBuilder();

            string costColor = Accessor.Simulation.CanPurchase(data.ConstructionCost) ? "#323232" : "red";
            sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Cost:</b> <color={0}>{1:C0}</color>{2}", costColor, data.ConstructionCost, Environment.NewLine);

            sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Building Footprint:</b> {0}x{1}{2}", data.Footprint.GetLength(0), data.Footprint.GetLength(1), Environment.NewLine);

            if (data.UtilitiesPerQuarter != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Utilities:</b> {0:C0} per quarter{1}", data.UtilitiesPerQuarter, Environment.NewLine);

            if (data.SmallClassrooms != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Small Classrooms:</b> {0}{1}", data.SmallClassrooms, Environment.NewLine);

            if (data.MediumClassrooms != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Medium Classrooms:</b> {0}{1}", data.MediumClassrooms, Environment.NewLine);

            if (data.LargeClassrooms != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Large Classrooms:</b> {0}{1}", data.LargeClassrooms, Environment.NewLine);

            int classroomCapacity =
                (data.SmallClassrooms * Accessor.CampusManager.SmallClassroomCapacity) +
                (data.MediumClassrooms * Accessor.CampusManager.MediumClassroomCapacty) +
                (data.LargeClassrooms * Accessor.CampusManager.LargeClassroomCapacity);

            if (classroomCapacity != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Student Capacity:</b> {0}{1}", classroomCapacity, Environment.NewLine);

            if (data.Laboratories != 0)
                sb.AppendFormat(CultureInfo.CurrentCulture, "<b>Laboratories:</b> {0}{1}", data.Laboratories, Environment.NewLine);

            sb.AppendLine();

            sb.AppendLine(data.Description);

            return sb.ToString();
        }
    }
}
