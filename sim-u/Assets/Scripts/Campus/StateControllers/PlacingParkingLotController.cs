using Campus.GridTerrain;
using Common;
using GameData;
using System;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingParkingLot game state.
    /// </summary>
    internal class PlacingParkingLotController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private Point3 _startPoint;
        private Point3 _endPoint;

        private RectCursor _cursor;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public PlacingParkingLotController(GridMesh terrain)
        {
            _terrain = terrain;
            _cursor = new RectCursor(
                terrain,
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

            _startPoint = _endPoint = args.GridSelection;

            Game.Campus.IsValidForParkingLot(_startPoint, _endPoint, out bool[,] validGrids);
            _cursor.Place(_startPoint, _endPoint, validGrids);
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
                if (Game.Campus.IsValidForParkingLot(_startPoint, _endPoint, out bool[,] validGrids))
                {
                    Game.Campus.ConstructParkingLot(_startPoint, _endPoint);
                }

                Transition(GameState.SelectingParkingLot);
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
                _endPoint = args.GridSelection;
            }
            else
            {
                _endPoint = _startPoint;
            }

            Game.Campus.IsValidForParkingLot(_startPoint, _endPoint, out bool[,] validGrids);
            _cursor.Place(_startPoint, _endPoint, validGrids);
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
                Transition(GameState.SelectingPath);
            }
        }
    }
}
