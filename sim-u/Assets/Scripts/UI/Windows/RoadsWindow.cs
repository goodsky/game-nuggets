using Common;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class RoadsWindow : Window
    {
        /// <summary>
        /// The window Title UI text element.
        /// </summary>
        public Text TitleText;

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
            Accessor.StateMachine.StartDoing(GameState.SelectingRoad, data);

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
    }
}
