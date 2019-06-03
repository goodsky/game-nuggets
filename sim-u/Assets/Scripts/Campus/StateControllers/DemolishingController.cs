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

        public DemolishingController(GridMesh terrain)
        {
            _terrain = terrain;
            _validMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid");
            _invalidMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid");
            _cursor = GridCursor.Create(terrain, _validMaterial);

            OnTerrainSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// Called by Game State Machine as the Demolishing state is transitioned in.
        /// </summary>
        /// <param name="_">Not used.</param>
        public override void TransitionIn(object _)
        {
            _cursor.Activate();
            _cursor.Place(_cursor.Position.x, _cursor.Position.y);
        }

        /// <summary>
        /// Called by Game State Manager as the Demolishing state is transitioned out.
        /// </summary>
        public override void TransitionOut()
        {
            _cursor.Deactivate();
        }

        public override void Update() { }

        /// <summary>
        /// Called when the Terrain is clicked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="args">Mouse click arguments</param>
        private void Clicked(object sender, TerrainClickedArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                if (HasDemolishableTile())
                {
                    Game.Campus.DestroyAt(_cursor.Position);
                    _cursor.SetMaterial(_invalidMaterial);
                }
            }

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
        private void PlacementUpdate(object sender, TerrainSelectionUpdateArgs args)
        {
            if (args.SelectionLocation != Point3.Null)
            {
                if (!_cursor.IsActive)
                    _cursor.Activate();

                _cursor.Place(args.SelectionLocation.x, args.SelectionLocation.z);
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
            return Game.Campus.GetGridUse(_cursor.Position) != CampusGridUse.Empty;
        }
    }
}
