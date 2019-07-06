using Campus.GridTerrain;
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
            /// Accessor for game components.
            /// </summary>
            protected GameAccessor Accessor = new GameAccessor();

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

            /// <summary>Event handler for terrain grid selection moved events that don't transition the state.</summary>
            protected event EventHandler<TerrainGridUpdateArgs> OnTerrainGridSelectionUpdate;
            public void TerrainGridSelectionUpdate(TerrainGridUpdateArgs args)
            {
                if (OnTerrainGridSelectionUpdate != null)
                {
                    OnTerrainGridSelectionUpdate.Invoke(null, args);
                }
            }

            /// <summary>Event handler for terrain vertex selection moved events that don't transition the state.</summary>
            protected event EventHandler<TerrainVertexUpdateArgs> OnTerrainVertexSelectionUpdate;
            public void TerrainVertexSelectionUpdate(TerrainVertexUpdateArgs args)
            {
                if (OnTerrainVertexSelectionUpdate != null)
                {
                    OnTerrainVertexSelectionUpdate.Invoke(null, args);
                }
            }

            /// <summary>
            /// Controllers can transition the game state.
            /// </summary>
            /// <param name="next">The state to transition to.</param>
            /// <param name="context">Optional context to pass.</param>
            protected void Transition(GameState next, object context = null)
            {
                Accessor.StateMachine.Transition(next, context);
            }
        }
    }
}
