using Campus.GridTerrain;
using Common;
using GameData;
using UI;
using UnityEngine;

namespace Campus
{
    public class PlacingParkingLotContext
    {
        public TerrainClickedArgs ClickedArgs { get; set; }

        public ParkingLotWindow Window { get; set; }
    }

    /// <summary>
    /// Game controller that runs during the PlacingParkingLot game state.
    /// </summary>
    [StateController(HandledState = GameState.PlacingParkingLot)]
    internal class PlacingParkingLotController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private ParkingLotWindow _window;
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
        public override void TransitionIn(object context)
        {
            var pathsContext = context as PlacingParkingLotContext;
            if (pathsContext == null)
                GameLogger.FatalError("PlacingParkingLotController was given unexpected context! Type = {0}", context?.GetType().Name ?? "null");

            var args = pathsContext.ClickedArgs;
            _window = pathsContext.Window;
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
                if (Accessor.CampusManager.IsValidForParkingLot(_rect, out bool[,] validGrids) &&
                    Accessor.Simulation.Purchase(CostOfParkingLot()))
                {
                    Accessor.CampusManager.ConstructParkingLot(_rect);
                    SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
                }
                else
                {
                    Transition(
                        GameState.SelectingParkingLot,
                        new SelectingParkingLotContext
                        {
                            Window = _window,
                        });
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

            int costOfParkingLot = CostOfParkingLot();
            _window.UpdateInfo(_rect.SizeX, _rect.SizeZ, costOfParkingLot);

            Accessor.CampusManager.IsValidForParkingLot(_rect, out bool[,] validGrids);

            if (!Accessor.Simulation.CanPurchase(costOfParkingLot))
            {
                for (int i = 0; i < validGrids.GetLength(0); ++i)
                    for (int j = 0; j < validGrids.GetLength(1); ++j)
                        validGrids[i, j] = false;
            }

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
                Transition(
                    GameState.SelectingParkingLot,
                    new SelectingParkingLotContext
                    {
                        Window = _window,
                    });
            }
        }

        private int CostOfParkingLot()
        {
            int squares = _rect.SizeX * _rect.SizeZ;
            return Accessor.CampusManager.GetCostOfConstruction(CampusGridUse.ParkingLot, squares);
        }
    }
}
