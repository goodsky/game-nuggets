using Common;
using GridTerrain;
using System;
using System.Linq;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    internal class PlacingPathController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private LineCursor _cursors;

        private Point3 _pathStart;
        private Point3 _pathEnd;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public PlacingPathController(GridMesh terrain)
        {
            _terrain = terrain;
            _cursors = new LineCursor(
                terrain,
                Resources.Load<Material>("Terrain/cursor_valid"),
                Resources.Load<Material>("Terrain/cursor_invalid"));

            OnTerrainSelectionUpdate += PlacementUpdate;
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
                GameLogger.FatalError("EditingTerrainController was given incorrect context.");

            _pathStart = _pathEnd = args.ClickLocation;
            _cursors.Place(_pathStart, _pathEnd, IsValidTerrainAlongLine());
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            _cursors.Deactivate();
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                if (IsValidTerrainAlongLine().All(b => b))
                {
                    Game.Campus.Paths.BuildPath(_pathStart, _pathEnd);
                }

                Transition(GameState.SelectingPath);
                return;
            }
        }

        /// <summary>
        /// Event handler for selection updates on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain selection update args.</param>
        private void PlacementUpdate(object sender, TerrainSelectionUpdateArgs args)
        {
            if (args.SelectionLocation != Point3.Null)
            {
                int lengthX = Math.Abs(_pathStart.x - args.SelectionLocation.x);
                int lengthZ = Math.Abs(_pathStart.z - args.SelectionLocation.z);

                // I'm trying to force people to snap to the grid more often than not
                if (lengthX > lengthZ)
                {
                    _pathEnd = new Point3(args.SelectionLocation.x, args.SelectionLocation.y, _pathStart.z);
                }
                else
                {
                    _pathEnd = new Point3(_pathStart.x, args.SelectionLocation.y, args.SelectionLocation.z);
                }
            }
            else
            {
                // snap to the extreme option if moused out of bounds
                if (_pathEnd.x < _pathStart.x)
                    _pathEnd = new Point3(0, _pathEnd.y, _pathEnd.z);
                else if (_pathEnd.x > _pathStart.x)
                    _pathEnd = new Point3(_terrain.CountX - 1, _pathEnd.y, _pathEnd.z);
                else if (_pathEnd.y < _pathStart.y)
                    _pathEnd = new Point3(_pathEnd.x, _pathEnd.y, 0);
                else if (_pathEnd.y > _pathStart.y)
                    _pathEnd = new Point3(_pathEnd.x, _pathEnd.y, _terrain.CountZ - 1);

            }

            _cursors.Place(_pathStart, _pathEnd, IsValidTerrainAlongLine());
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

        /// <summary>
        /// Get a boolean array representing whether the grids selected are valid for path.
        /// </summary>
        /// <returns>A boolean array representing the valid terrain along the line.</returns>
        private bool[] IsValidTerrainAlongLine()
        {
            var gridcheck = _terrain.Editor.CheckSmoothAndFree(_pathStart.x, _pathStart.z, _pathEnd.x, _pathEnd.z);

            int dx = 0;
            int dz = 0;

            if (_pathStart.x == _pathEnd.x && _pathStart.z == _pathEnd.z)
            {
                // Case: Placing a single square
            }
            else if (_pathStart.x != _pathEnd.x)
            {
                // Case: Placing a line along the x-axis
                dx = _pathStart.x < _pathEnd.x ? 1 : -1;
            }
            else
            {
                // Case: Placing a line along the z-axis
                dz = _pathStart.z < _pathEnd.z ? 1 : -1;
            }

            for (int i = 0; i < gridcheck.Length; ++i)
            {
                if (!gridcheck[i])
                {
                    int checkx = _pathStart.x + i * dx;
                    int checkz = _pathStart.z + i * dz;

                    // We can build over existing path
                    gridcheck[i] = Game.Campus.Paths.IsPath(checkx, checkz);
                }
            }

            return gridcheck;
        }
    }
}
