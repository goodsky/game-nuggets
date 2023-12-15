using Campus;
using Common;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class RoadsWindow : Window
    {
        /// <summary>
        /// The length of the road UI text element.
        /// </summary>
        public Text RoadLengthText;

        /// <summary>
        /// The cost of the road UI text element.
        /// </summary>
        public Text RoadCostText;

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
            Accessor.CampusManager.SetTerrainSelectionParent(this);
            Accessor.StateMachine.StartDoing(
                GameState.SelectingRoad,
                new SelectingRoadContext
                {
                    Window = this,
                });

            StopButton.OnSelect = () => { SelectionManager.UpdateSelection(null); };
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
        /// Update the window info texts.
        /// </summary>
        /// <param name="roadLength">The length of the road being built.</param>
        /// <param name="roadCost">The cost of the road being built.</param>
        public void UpdateInfo(int roadLength, int roadCost)
        {
            RoadLengthText.text = roadLength.ToString();
            LayoutRebuilder.MarkLayoutForRebuild(RoadLengthText.rectTransform);

            string color = Accessor.Simulation.CanPurchase(roadCost) ? "green" : "red";
            RoadCostText.text = $"<color={color}>${roadCost:n0}</color>";
            LayoutRebuilder.MarkLayoutForRebuild(RoadCostText.rectTransform);
        }
    }
}
