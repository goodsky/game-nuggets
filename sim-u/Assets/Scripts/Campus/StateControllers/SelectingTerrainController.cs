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

            OnTerrainSelectionUpdate += SelectionUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// Transition to SelectingTerrain state.
        /// </summary>
        /// <param name="context">Not used.</param>
        public override void TransitionIn(object context)
        {
            _cursor.Activate();
            _cursor.Place(_cursor.Position.x, _cursor.Position.y);
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
        private void SelectionUpdate(object sender, TerrainSelectionUpdateArgs args)
        {
            if (args.SelectionLocation != Point3.Null)
            {
                if (!_cursor.IsActive)
                    _cursor.Activate();

                _cursor.Place(args.SelectionLocation.x, args.SelectionLocation.z);
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
                Transition(GameState.EditingTerrain, args);
            }

            if (Application.isEditor)
            {
                GameLogger.Info("Selected ({0}); Point Heights ({1}, {2}, {3}, {4})",
                       args.ClickLocation,
                       _terrain.GetVertexHeight(args.ClickLocation.x, args.ClickLocation.z),
                       _terrain.GetVertexHeight(args.ClickLocation.x + 1, args.ClickLocation.z),
                       _terrain.GetVertexHeight(args.ClickLocation.x, args.ClickLocation.z + 1),
                       _terrain.GetVertexHeight(args.ClickLocation.x + 1, args.ClickLocation.z + 1));

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    int submaterial = _terrain.GetSubmaterial(args.ClickLocation.x, args.ClickLocation.z);
                    _terrain.SetSubmaterial(args.ClickLocation.x, args.ClickLocation.z, (submaterial + 1) % _terrain.SubmaterialCount, Rotation.deg270);
                }
            }
        }
    }
}
