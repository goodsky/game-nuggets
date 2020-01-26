using Campus.GridTerrain;
using Common;
using GameData;
using UI;
using UnityEngine;

namespace Campus
{
    public class PlacingConstructionContext
    {
        public BuildingData BuildingData { get; set; }

        public ConstructionPlacingWindow Window { get; set; }
    }

    /// <summary>
    /// Game controller that runs during the PlacingContruction game state.
    /// </summary>
    [StateController(HandledState = GameState.PlacingConstruction)]
    internal class PlacingConstructionController : GameStateMachine.Controller
    {
        private GridMesh _terrain;

        private BuildingData _building;
        private ConstructionPlacingWindow _window;
        private FootprintCursor _cursors;
        private BuildingRotation _rotation = BuildingRotation.deg0;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public PlacingConstructionController()
        {
            _terrain = Accessor.Terrain;
            _building = null;
            _cursors = new FootprintCursor(
                _terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_entry_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_entry_invalid"));

            OnTerrainGridSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Build;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="context">The construction to place.</param>
        public override void TransitionIn(object context)
        {
            var constructionContext = context as PlacingConstructionContext;
            if (constructionContext == null)
                GameLogger.FatalError("PlacingConstructionController was given unexpected context! Type = {0}", context?.GetType().Name ?? "null");

            _building = constructionContext.BuildingData;
            _window = constructionContext.Window;

            _rotation = BuildingRotation.deg0;
            _window.RotateClockwiseButton.OnSelect = RotateClockwise;
            _window.RotateCounterClockwiseButton.OnSelect = RotateCounterClockwise;

            _cursors.Create(_building);
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            _building = null;
            _window = null;
            _cursors.Destroy();
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update()
        {
            if (_building != null)
            {
                // Rotate by pressing '>'
                if (Input.GetKeyDown(KeyCode.Period))
                {
                    RotateClockwise();
                }

                // Rotate by pressing '<'
                if (Input.GetKeyDown(KeyCode.Comma))
                {
                    RotateCounterClockwise();
                }

                // Note: this call is needed to update the color of the cost text.
                _window.UpdateInfo(_building.ConstructionCost);
            }
        }

        /// <summary>
        /// Event handler for selection updates on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain selection update args.</param>
        private void PlacementUpdate(object sender, TerrainGridUpdateArgs args)
        {
            if (args.GridSelection != Point3.Null)
            {
                Accessor.CampusManager.IsValidForBuilding(_building, args.GridSelection, _rotation, out bool[,] validGrids);

                _cursors.Place(args.GridSelection, _rotation);
                _cursors.SetMaterials(validGrids, _rotation);
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
                if (Accessor.CampusManager.IsValidForBuilding(_building, args.GridSelection, _rotation, out bool[,] _) &&
                    Accessor.Simulation.Purchase(_building.ConstructionCost))
                {
                    Accessor.CampusManager.ConstructBuilding(_building, args.GridSelection, _rotation);
                    SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
                }
            }
        }

        /// <summary>
        /// Rotate the building CCW
        /// </summary>
        private void RotateClockwise()
        {
            int rInt = (int)_rotation;
            rInt += 1;
            if (rInt >= 4) rInt = 0;
            _rotation = (BuildingRotation)rInt;

            // Resend update even though the grid has not changed.
            Accessor.StateMachine.PumpTerrainGridSelection();
        }

        /// <summary>
        /// Rotate the building CW
        /// </summary>
        private void RotateCounterClockwise()
        {
            int rInt = (int)_rotation;
            rInt -= 1;
            if (rInt < 0) rInt = 3;
            _rotation = (BuildingRotation)rInt;

            // Resend update even though the grid has not changed.
            Accessor.StateMachine.PumpTerrainGridSelection();
        }
    }
}
