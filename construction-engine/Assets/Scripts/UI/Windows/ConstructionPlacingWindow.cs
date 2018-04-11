using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace UI
{
    public class ConstructionPlacingWindow : Window
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
            var buildingData = data as BuildingData;
            if (buildingData == null)
            {
                GameLogger.FatalError("ConstructionPlacingWindow was passed invalid data. Data = {0}", data == null ? "null" : data.GetType().Name);
            }

            Game.Campus.Terrain.Selectable.SelectionParent = this;
            Game.State.StartDoing(GameState.PlacingConstruction);

            StopButton.OnSelect = () => { SelectionManager.UpdateSelection(SelectionParent.SelectionParent); };
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        public override void Close()
        {
            Game.Campus.Terrain.Selectable.SelectionParent = null;
            Game.State.StopDoing();
        }
    }
}
