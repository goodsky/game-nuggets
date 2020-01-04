using Campus;
using Common;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class ParkingLotWindow : Window
    {
        /// <summary>
        /// The size of the parking lot UI text element.
        /// </summary>
        public Text ParkingLotSizeText;

        /// <summary>
        /// The cost of the parking lot UI text element.
        /// </summary>
        public Text ParkingLotCostText;

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
        public override void Open(object _)
        {
            Accessor.CampusManager.SetTerrainSelectionParent(this);
            Accessor.StateMachine.StartDoing(
                GameState.SelectingParkingLot,
                new SelectingParkingLotContext
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
        /// <param name="lotLength">The length of the parking lot being built.</param>
        /// <param name="lotWidth">The length of the parking lot being built.</param>
        /// <param name="lotCost">The cost of the parking lot being built.</param>
        public void UpdateInfo(int lotLength, int lotWidth, int lotCost)
        {
            ParkingLotSizeText.text = $"{lotLength}x{lotWidth}";
            LayoutRebuilder.MarkLayoutForRebuild(ParkingLotSizeText.rectTransform);

            string color = Accessor.Simulation.CanPurchase(lotCost) ? "green" : "red";
            ParkingLotCostText.text = $"<color={color}>${lotCost:n0}</color>";
            LayoutRebuilder.MarkLayoutForRebuild(ParkingLotCostText.rectTransform);
        }
    }
}
