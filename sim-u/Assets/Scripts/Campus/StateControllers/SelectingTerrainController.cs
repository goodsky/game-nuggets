using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the SelectingTerrain game state.
    /// </summary>
    internal class SelectingTerrainController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private GridCursor _cursor;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to edit.</param>
        public SelectingTerrainController(GridMesh terrain)
        {
            _terrain = terrain;
            _cursor = GridCursor.Create(terrain, ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_terrain"));

            OnTerrainGridSelectionUpdate += SelectionUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// Transition to SelectingTerrain state.
        /// </summary>
        /// <param name="context">Not used.</param>
        public override void TransitionIn(object context)
        {
            _cursor.Activate();
            _cursor.Place(_cursor.Position);
        }

        /// <summary>
        /// Transition out of SelectingTerrain state.
        /// </summary>
        public override void TransitionOut()
        {
            if (_cursor != null)
            {
                _cursor.Deactivate();
            }
        }

        /// <summary>
        /// Update during the SelectingTerrain state.
        /// </summary>
        public override void Update() { }

        /// <summary>
        /// Update the cursor selection.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="args">The terrain selection arguments.</param>
        private void SelectionUpdate(object sender, TerrainGridUpdateArgs args)
        {
            if (args.GridSelection != Point3.Null)
            {
                if (!_cursor.IsActive)
                    _cursor.Activate();

                _cursor.Place(args.GridSelection);
            }
            else
            {
                if (_cursor.IsActive)
                    _cursor.Deactivate();
            }
        }

        /// <summary>
        /// Handle a click event on the terrain.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="args">The terrain click arguments.</param>
        private void Clicked(object sender, TerrainClickedArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                if (_cursor.IsActive)
                {
                    Transition(GameState.EditingTerrain, args);
                }
            }

            // DEBUGGING:
            if (args.Button == MouseButton.Right && args.GridSelection != Point3.Null)
            {
                GameLogger.Info("Selected ({0}); Point Heights ({1}, {2}, {3}, {4}); Anchored ({5}, {6}, {7}, {8})",
                       args.GridSelection,
                       _terrain.GetVertexHeight(args.GridSelection.x + 1, args.GridSelection.z + 1),
                       _terrain.GetVertexHeight(args.GridSelection.x + 1, args.GridSelection.z),
                       _terrain.GetVertexHeight(args.GridSelection.x, args.GridSelection.z),
                       _terrain.GetVertexHeight(args.GridSelection.x, args.GridSelection.z + 1),
                       _terrain.Editor.IsVertexAnchored(args.GridSelection.x + 1, args.GridSelection.z + 1),
                       _terrain.Editor.IsVertexAnchored(args.GridSelection.x + 1, args.GridSelection.z),
                       _terrain.Editor.IsVertexAnchored(args.GridSelection.x, args.GridSelection.z),
                       _terrain.Editor.IsVertexAnchored(args.GridSelection.x, args.GridSelection.z + 1));
            }
        }
    }
}
