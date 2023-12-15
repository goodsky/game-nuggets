﻿using Common;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// Companion behavior for the New Construction UI window.
    /// </summary>
    public class TerrainEditingWindow : Window
    {
        /// <summary>
        /// The build button.
        /// </summary>
        public Button StopButton;

        /// <summary>Gets the UI Buttons on this window.</summary>
        public override List<Button> Buttons
        {
            get { return new List<Button>() { StopButton }; }
        }

        /// <summary>
        /// Open the window.
        /// </summary>
        /// <param name="data">not used.</param>
        public override void Open(object data)
        {
            Accessor.CampusManager.SetTerrainSelectionParent(this);
            Accessor.StateMachine.StartDoing(GameState.SelectingTerrain);

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
