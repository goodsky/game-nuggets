using Campus;
using Campus.GridTerrain;
using Common;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class PathsWindow : Window
    {
        /// <summary>
        /// The length of the path UI text element.
        /// </summary>
        public Text PathLengthText;

        /// <summary>
        /// The cost of the path UI text element.
        /// </summary>
        public Text PathCostText;

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
                GameState.SelectingPath,
                new SelectingPathContext
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
        /// <param name="pathLength">The length of the path being built.</param>
        /// <param name="pathCost">The cost of the path being built.</param>
        public void UpdateInfo(int pathLength, int pathCost)
        {
            PathLengthText.text = pathLength.ToString();
            LayoutRebuilder.MarkLayoutForRebuild(PathLengthText.rectTransform);

            string color = Accessor.Simulation.CanPurchase(pathCost) ? "green" : "red";
            PathCostText.text = $"<color={color}>${pathCost:n0}</color>";
            LayoutRebuilder.MarkLayoutForRebuild(PathCostText.rectTransform);
        }
    }
}
