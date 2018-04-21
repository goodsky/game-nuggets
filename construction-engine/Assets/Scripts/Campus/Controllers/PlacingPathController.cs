using Common;
using GridTerrain;
using System;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    class PlacingPathController : GameStateMachine.Controller
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
            _cursors.Place(_pathStart, _pathEnd, GetValidTerrainAlongLine());
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

                _cursors.Place(_pathStart, _pathEnd, GetValidTerrainAlongLine());
            }
            else
            {
                int length = Math.Abs(_pathStart.x - _pathEnd.x) + Math.Abs(_pathStart.z - _pathEnd.z);
                bool[] allInvalid = new bool[length];
                _cursors.Place(_pathStart, _pathEnd, allInvalid);
            }
        }

        /// <summary>
        /// Get a boolean array representing valid construction terrain along the line.
        /// </summary>
        /// <returns>A boolean array representing the valid terrain along the line.</returns>
        private bool[] GetValidTerrainAlongLine()
        {
            return _terrain.Editor.CheckSmoothAndFree(_pathStart.x, _pathStart.z, _pathEnd.x, _pathEnd.z);
        }
    }
}
