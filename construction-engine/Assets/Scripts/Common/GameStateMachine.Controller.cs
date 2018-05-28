using GridTerrain;
using System;

namespace Common
{
    public partial class GameStateMachine
    {
        /// <summary>
        /// Base class for game state controllers.
        /// </summary>
        public abstract class Controller
        {
            /// <summary>
            /// Called each game step while this state is active.
            /// </summary>
            public abstract void Update();

            /// <summary>
            /// Called once as this state is newly started.
            /// </summary>
            /// <param name="context">Optionally arguments to give context to the new state.</param>
            public abstract void TransitionIn(object context);

            /// <summary>
            /// Called once as this state is ending.
            /// </summary>
            public abstract void TransitionOut();

            /// <summary>Event handler for terrain clicked events that don't transition the state.</summary>
            protected event EventHandler<TerrainClickedArgs> OnTerrainClicked;
            public void TerrainClicked(TerrainClickedArgs args)
            {
                if (OnTerrainClicked != null)
                {
                    OnTerrainClicked.Invoke(null, args);
                }
            }

            /// <summary>Event handler for terrain selection moved events that don't transition the state.</summary>
            protected event EventHandler<TerrainSelectionUpdateArgs> OnTerrainSelectionUpdate;
            public void TerrainSelectionUpdate(TerrainSelectionUpdateArgs args)
            {
                if (OnTerrainSelectionUpdate != null)
                {
                    OnTerrainSelectionUpdate.Invoke(null, args);
                }
            }

            /// <summary>
            /// Controllers can transition the game state.
            /// </summary>
            /// <param name="next">The state to transition to.</param>
            /// <param name="context">Optional context to pass.</param>
            protected void Transition(GameState next, object context = null)
            {
                Game.State.Transition(next, context);
            }
        }
    }
}
