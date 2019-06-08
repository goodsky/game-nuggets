using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    internal class SelectingPathController : GameStateMachine.Controller
    {
        private GridMesh _terrain;

        private Material _validMaterial;
        private Material _invalidMaterial;
        private GridCursor _cursor;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public SelectingPathController(GridMesh terrain)
        {
            _terrain = terrain;

            _validMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid");
            _invalidMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid");
            _cursor = GridCursor.Create(terrain, _validMaterial);

            OnTerrainSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="_">Not used.</param>
        public override void TransitionIn(object _)
        {
            _cursor.Activate();
            _cursor.Place(_cursor.Position.x, _cursor.Position.z);
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            _cursor.Deactivate();
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update() { }

        /// <summary>
        /// Event handler for selection updates on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain selection update args.</param>
        private void PlacementUpdate(object sender, TerrainSelectionUpdateArgs args)
        {
            if (args.SelectionLocation != Point3.Null)
            {
                if (!_cursor.IsActive)
                    _cursor.Activate();

                _cursor.Place(args.SelectionLocation.x, args.SelectionLocation.z);
                _cursor.SetMaterial(
                    IsValidTerrain() ?
                        _validMaterial :
                        _invalidMaterial);
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
                if (IsValidTerrain())
                {
                    Transition(GameState.PlacingPath, args);
                }
            }

            // DEBUGGING:
            if (args.Button == MouseButton.Right)
            {
                GameLogger.Info("IsValidTerrain: {0}; IsSmoothAndFree: {1}; Grid Use: {2}", 
                    IsValidTerrain(), 
                    Game.Campus.CheckLineSmoothAndFree(new AxisAlignedLine(_cursor.Position))[0],
                    Game.Campus.GetGridUse(_cursor.Position));
            }
        }

        /// <summary>
        /// Gets a value representing whether or not the grid under the cursor is valid for path.
        /// </summary>
        /// <returns>True if the grid is valid, false otherwise.</returns>
        private bool IsValidTerrain()
        {
            return
                Game.Campus.CheckLineSmoothAndFree(new AxisAlignedLine(_cursor.Position))[0] ||
                Game.Campus.GetGridUse(_cursor.Position) == CampusGridUse.Path;
        }
    }
}
