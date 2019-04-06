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
            Game.Campus.Terrain.Selectable.SelectionParent = this;
            Game.State.StartDoing(GameState.SelectingTerrain);

            StopButton.OnSelect = () => { SelectionManager.UpdateSelection(SelectionParent.ToMainMenu()); };
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
