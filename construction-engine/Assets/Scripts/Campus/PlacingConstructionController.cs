using Common;
using GameData;
using GridTerrain;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingContruction game state.
    /// </summary>
    public class PlacingConstructionController : GameStateController
    {
        private GridMesh _terrain;

        private BuildingData _building;
        private FootprintCursor _cursors;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public PlacingConstructionController(GridMesh terrain)
        {
            _terrain = terrain;
            _building = null;
            _cursors = new FootprintCursor(
                terrain,
                Resources.Load<Material>("Terrain/cursor_valid"),
                Resources.Load<Material>("Terrain/cursor_invalid"));

            OnTerrainSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Build;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="context">The construction to place.</param>
        public override void TransitionIn(object context)
        {
            _building = context as BuildingData;
            if (_building == null)
                GameLogger.FatalError("PlacingConstructionController was not given a building data!");

            _cursors.Create(_building.Footprint);
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            _cursors.Destroy();
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
                _cursors.Place(args.SelectionLocation);
            }
            else
            {
                _cursors.Deactivate();
            }
        }

        /// <summary>
        /// Event handler for clicks on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain click arguments.</param>
        private void Build(object sender, TerrainClickedArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                CampusFactory.GenerateBuilding(
                    _building, 
                    Game.Campus.transform, 
                    _terrain.Convert.GridToWorld(args.ClickLocation) + new Vector3(0f, 0.01f, 0f) /* Place just above the grass*/, 
                    Quaternion.identity);

                SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
            }
        }
    }
}
