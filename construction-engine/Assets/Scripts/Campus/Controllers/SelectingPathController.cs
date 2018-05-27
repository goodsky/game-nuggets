using Common;
using GridTerrain;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    class SelectingPathController : GameStateMachine.Controller
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

            _validMaterial = Resources.Load<Material>("Terrain/cursor_valid");
            _invalidMaterial = Resources.Load<Material>("Terrain/cursor_invalid");
            _cursor = GridCursor.Create(terrain, _validMaterial);

            OnTerrainSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="context">The construction to place.</param>
        public override void TransitionIn(object context)
        {
            _cursor.Activate();
            _cursor.Place(_cursor.Position.x, _cursor.Position.y);
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            if (_cursor != null)
            {
                _cursor.Deactivate();
            }
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
                    _terrain.Editor.CheckSmoothAndFree(_cursor.Position.x, _cursor.Position.y, _cursor.Position.x, _cursor.Position.y)[0] ?
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
                if (_terrain.Editor.CheckSmoothAndFree(_cursor.Position.x, _cursor.Position.y, _cursor.Position.x, _cursor.Position.y)[0])
                {
                    Transition(GameState.PlacingPath, args);
                }
            }
        }
    }
}
