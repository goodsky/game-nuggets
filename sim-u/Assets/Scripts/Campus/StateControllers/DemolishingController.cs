using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game Controller that runs during the Demolishing State.
    /// </summary>
    internal class DemolishingController : GameStateMachine.Controller
    {
        private GridMesh _terrain;

        private Material _validMaterial;
        private Material _invalidMaterial;
        private GridCursor _cursor;

        private Point2 _lastDemolishedGrid = Point2.Null;

        public DemolishingController(GridMesh terrain)
        {
            _terrain = terrain;
            _validMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid");
            _invalidMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid");
            _cursor = GridCursor.Create(terrain, _validMaterial);

            OnTerrainGridSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// Called by Game State Machine as the Demolishing state is transitioned in.
        /// </summary>
        /// <param name="_">Not used.</param>
        public override void TransitionIn(object _)
        {
            _cursor.Activate();
            _cursor.Place(_cursor.Position);
        }

        /// <summary>
        /// Called by Game State Manager as the Demolishing state is transitioned out.
        /// </summary>
        public override void TransitionOut()
        {
            _cursor.Deactivate();
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (_cursor.Position != _lastDemolishedGrid && HasDemolishableTile())
                {
                    Accessor.CampusManager.DestroyAt(_cursor.Position);
                    _cursor.SetMaterial(_invalidMaterial);

                    // Only one demolish per mouse down (don't delete multiple improvements at once).
                    _lastDemolishedGrid = _cursor.Position;
                }
            }
            else
            {
                _lastDemolishedGrid = Point2.Null;
            }
        }

        /// <summary>
        /// Called when the Terrain is clicked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="args">Mouse click arguments</param>
        private void Clicked(object sender, TerrainClickedArgs args)
        {
            // DEBUGGING:
            if (args.Button == MouseButton.Right)
            {
                GameLogger.Info("HasDemolishableTile: {0};", HasDemolishableTile());
            }
        }

        /// <summary>
        /// Called when the current mouse grid selection changes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="args">Mouse movement arguments.</param>
        private void PlacementUpdate(object sender, TerrainGridUpdateArgs args)
        {
            if (args.GridSelection != Point3.Null)
            {
                if (!_cursor.IsActive)
                    _cursor.Activate();

                _cursor.Place(args.GridSelection);
                _cursor.SetMaterial(
                    HasDemolishableTile() ?
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
        /// Checks if a demolishable tile is under the mouse.
        /// </summary>
        /// <returns>True if a demolishable tile exists under the mouse, otherwise false.</returns>
        private bool HasDemolishableTile()
        {
            return Accessor.CampusManager.GetGridUse(_cursor.Position) != CampusGridUse.Empty;
        }
    }
}
