using Campus.GridTerrain;
using Common;
using GameData;
using UI;
using UnityEngine;

namespace Campus
{
    public class PlacingPathContext
    {
        public TerrainClickedArgs ClickedArgs { get; set; }

        public PathsWindow Window { get; set; }
    }

    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    [StateController(HandledState = GameState.PlacingPath)]
    internal class PlacingPathController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private PathsWindow _window;
        private LineCursor _cursor;

        private AxisAlignedLine _line;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public PlacingPathController()
        {
            _terrain = Accessor.Terrain;
            _cursor = new LineCursor(
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
            var pathsContext = context as PlacingPathContext;
            if (pathsContext == null)
                GameLogger.FatalError("PlacingPathController was given unexpected context! Type = {0}", context?.GetType().Name ?? "null");

            var args = pathsContext.ClickedArgs;
            _window = pathsContext.Window;
            _line = new AxisAlignedLine(args.GridSelection);

            Accessor.CampusManager.IsValidForPath(_line, out bool[] validGrids);
            _cursor.Place(_line, validGrids);
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
                if (Accessor.CampusManager.IsValidForPath(_line, out bool[] _) &&
                    Accessor.Simulation.Purchase(CostOfPath()))
                {
                    Accessor.CampusManager.ConstructPath(_line);
                }

                Transition(
                    GameState.SelectingPath,
                    new SelectingPathContext
                    {
                        Window = _window,
                    });
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
                _line.UpdateEndPointAlongAxis(args.GridSelection);
            }
            else
            {
                // Snap to the extreme option if moused out of bounds
                if (_line.End.x < _line.Start.x)
                    _line.UpdateEndPointAlongAxis(new Point2(0, _line.End.z));
                else if (_line.End.x > _line.Start.x)
                    _line.UpdateEndPointAlongAxis(new Point2(_terrain.CountX - 1, _line.End.z));
                else if (_line.End.z < _line.Start.z)
                    _line.UpdateEndPointAlongAxis(new Point2(_line.End.x, 0));
                else if (_line.End.z > _line.Start.z)
                    _line.UpdateEndPointAlongAxis(new Point2(_line.End.x, _terrain.CountZ - 1));
            }

            int costOfPath = CostOfPath();
            _window.UpdateInfo(_line.Length, costOfPath);

            Accessor.CampusManager.IsValidForPath(_line, out bool[] validGrids);
            
            if (!Accessor.Simulation.CanPurchase(costOfPath))
            {
                for (int i = 0; i < validGrids.Length; ++i)
                    validGrids[i] = false;
            }

            _cursor.Place(_line, validGrids);
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
                    GameState.SelectingPath,
                    new SelectingPathContext
                    {
                        Window = _window,
                    });
            }
        }

        private int CostOfPath()
        {
            int squares = _line.Length;
            return Accessor.CampusManager.GetCostOfConstruction(CampusGridUse.Path, squares);
        }
    }
}
