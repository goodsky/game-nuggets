using Common;
using GridTerrain;
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
            // Set the terrain to be the child of this window.
            EditableTerrain.Singleton.SelectionParent = this;

            // Set the game state.
            GameStateMachine.SetState(GameState.EditingTerrain);

            StopButton.OnSelect = () => { SelectionManager.UpdateSelection(SelectionParent.SelectionParent); };
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        public override void Close()
        {
            EditableTerrain.Singleton.SelectionParent = null;
            GameStateMachine.SetState(GameState.Selecting);
        }
    }
}
