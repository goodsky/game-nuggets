using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingParkingLot game state.
    /// </summary>
    [StateController(HandledState = GameState.PlacingParkingLot)]
    internal class PlacingParkingLotController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private Rectangle _rect;

        private RectangleCursor _cursor;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public PlacingParkingLotController()
        {
            _terrain = Accessor.Terrain;
            _cursor = new RectangleCursor(
                _terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"));

            OnTerrainGridSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="context">The construction to place.</param>
        public override void TransitionIn(object context)
        {
            var args = context as TerrainClickedArgs;
            if (args == null)
                GameLogger.FatalError("PlacingPathController was given incorrect context.");

            _rect = new Rectangle(args.GridSelection);

            Accessor.CampusManager.IsValidForParkingLot(_rect, out bool[,] validGrids);
            _cursor.Place(_rect, validGrids);
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
        public override void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                if (Accessor.CampusManager.IsValidForParkingLot(_rect, out bool[,] validGrids))
                {
                    Accessor.CampusManager.ConstructParkingLot(_rect);
                    SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
                }
                else
                {
                    Transition(GameState.SelectingParkingLot);
                }

                return;
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
                _rect.UpdateEndPoint(args.GridSelection);
            }
            else
            {
                _rect.UpdateEndPoint(_rect.Start);
            }

            Accessor.CampusManager.IsValidForParkingLot(_rect, out bool[,] validGrids);
            _cursor.Place(_rect, validGrids);
        }

        /// <summary>
        /// Handle a click event on the terrain.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="args">The terrain click arguments.</param>
        private void Clicked(object sender, TerrainClickedArgs args)
        {
            if (args.Button == MouseButton.Right)
            {
                // Cancel placing path.
                Transition(GameState.SelectingParkingLot);
            }
        }
    }
}
