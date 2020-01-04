using Campus;
using Common;
using GameData;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class ConstructionPlacingWindow : Window
    {
        /// <summary>
        /// The name of the building UI text element.
        /// </summary>
        public Text BuildingNameText;

        /// <summary>
        /// The cost of the building UI text element.
        /// </summary>
        public Text BuildingCostText;

        /// <summary>
        /// The window stop UI button.
        /// </summary>
        public Button StopButton;

        /// <summary>Gets the UI Buttons on this window.</summary>
        public override List<Button> Buttons
        {
            get { return new List<Button>() { StopButton }; }
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
                GameLogger.FatalError("ConstructionPlacingWindow was passed invalid data. Data = {0}", data?.GetType().Name ?? "null");
            }

            Accessor.CampusManager.SetTerrainSelectionParent(this);
            Accessor.StateMachine.StartDoing(
                GameState.PlacingConstruction,
                new PlacingConstructionContext()
                {
                    BuildingData = buildingData,
                    Window = this,
                });

            BuildingNameText.text = buildingData.Name;
            LayoutRebuilder.MarkLayoutForRebuild(BuildingNameText.rectTransform);

            UpdateInfo(buildingData.ConstructionCost);

            StopButton.OnSelect = () => { SelectionManager.UpdateSelection(SelectionParent.ToMainMenu()); };
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        public override void Close()
        {
            Accessor.CampusManager.SetTerrainSelectionParent(null);
            Accessor.StateMachine.StopDoing();
        }

        /// <summary>
        /// Update the window info.
        /// </summary>
        /// <param name="buildingCost">Current cost of the building.</param>
        public void UpdateInfo(int buildingCost)
        {
            string color = Accessor.Simulation.CanPurchase(buildingCost) ? "green" : "red";
            BuildingCostText.text = $"<color={color}>${buildingCost:n0}</color>";
            LayoutRebuilder.MarkLayoutForRebuild(BuildingCostText.rectTransform);
        }
    }
}
