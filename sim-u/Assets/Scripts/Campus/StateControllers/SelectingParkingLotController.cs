﻿using Campus.GridTerrain;
using Common;
using GameData;
using UI;
using UnityEngine;

namespace Campus
{
    public class SelectingParkingLotContext
    {
        public ParkingLotWindow Window { get; set; }
    }

    /// <summary>
    /// Game controller that runs during the SelectingParkingLot game state.
    /// </summary>
    [StateController(HandledState = GameState.SelectingParkingLot)]
    internal class SelectingParkingLotController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private ParkingLotWindow _window;

        private Material _validMaterial;
        private Material _invalidMaterial;
        private GridCursor _cursor;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public SelectingParkingLotController()
        {
            _terrain = Accessor.Terrain;

            _validMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid");
            _invalidMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid");
            _cursor = GridCursor.Create(_terrain, _validMaterial);

            OnTerrainGridSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        public override void TransitionIn(object context)
        {
            var parkingLotContext = context as SelectingParkingLotContext;
            if (parkingLotContext == null)
                GameLogger.FatalError("SelectingParkingLotController was given unexpected context! Type = {0}", context?.GetType().Name ?? "null");

            _window = parkingLotContext.Window;
            _window.UpdateInfo(0, 0, 0);

            _cursor.Activate();
            _cursor.Place(_cursor.Position);
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
        private void PlacementUpdate(object sender, TerrainGridUpdateArgs args)
        {
            if (args.GridSelection != Point3.Null)
            {
                if (!_cursor.IsActive)
                    _cursor.Activate();

                bool isValid = Accessor.CampusManager.IsValidForParkingLot(new Rectangle(args.GridSelection), out bool[,] _, ignoreSizeConstraint: true);
                _cursor.Place(args.GridSelection);
                _cursor.SetMaterial(
                    isValid ?
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
                if (Accessor.CampusManager.IsValidForParkingLot(new Rectangle(args.GridSelection), out bool[,] _, ignoreSizeConstraint: true))
                {
                    Transition(
                        GameState.PlacingParkingLot,
                        new PlacingParkingLotContext
                        {
                            ClickedArgs = args,
                            Window = _window,
                        });
                }
            }
        }
    }
}
