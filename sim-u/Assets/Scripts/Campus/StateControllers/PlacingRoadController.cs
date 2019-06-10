using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Linq;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingRoad game state.
    /// </summary>
    internal class PlacingRoadController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private LineCursor _cursor1;
        private LineCursor _cursor2;

        private AxisAlignedLine _vertexLine;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public PlacingRoadController(GridMesh terrain)
        {
            _terrain = terrain;
            _cursor1 = new LineCursor(
                terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"));
            _cursor2 = new LineCursor(
                terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"));

            OnTerrainVertexSelectionUpdate += PlacementUpdate;
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

            _vertexLine = new AxisAlignedLine(args.VertexSelection);

            (AxisAlignedLine line1, AxisAlignedLine line2) = GetRoadGridLines();
            _cursor1.Place(line1, IsValidForRoadAlongLine(line1));
            _cursor2.Place(line2, IsValidForRoadAlongLine(line2));
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            _cursor1.Deactivate();
            _cursor2.Deactivate();
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                (AxisAlignedLine line1, AxisAlignedLine line2) = GetRoadGridLines();
                if (IsValidForRoadAlongLine(line1).All(x => x) &&
                    IsValidForRoadAlongLine(line2).All(x => x))
                {
                    Game.Campus.ConstructRoad(_vertexLine);
                }

                Transition(GameState.SelectingRoad);
                return;
            }
        }

        /// <summary>
        /// Event handler for selection updates on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain selection update args.</param>
        private void PlacementUpdate(object sender, TerrainVertexUpdateArgs args)
        {
            if (args.VertexSelection != Point2.Null)
            {
                _vertexLine.UpdateEndPointAlongAxis(args.VertexSelection);

                (AxisAlignedLine line1, AxisAlignedLine line2) = GetRoadGridLines();
                _cursor1.Place(line1, IsValidForRoadAlongLine(line1));
                _cursor2.Place(line2, IsValidForRoadAlongLine(line2));
            }
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
                Transition(GameState.SelectingRoad);
            }
        }

        /// <summary>
        /// Get a boolean array representing whether the grids selected are valid for path.
        /// </summary>
        /// <returns>A boolean array representing the valid terrain along the line.</returns>
        private bool[] IsValidForRoadAlongLine(AxisAlignedLine line)
        {
            bool[] gridcheck = Game.Campus.CheckLineSmoothAndFree(line);
            foreach ((int lineIndex, Point2 point) in line.PointsAlongLine())
            {
                if (!gridcheck[lineIndex])
                {
                    // We can build over existing road
                    // NB: Assumes currently built roads are smooth
                    gridcheck[lineIndex] = Game.Campus.GetGridUse(point) == CampusGridUse.Road;
                }

                if (gridcheck[lineIndex])
                {
                    // Rule: You can't make a tight turn with roads. It messes up my lanes. And it's ugly.
                    //       Don't do it!

                }
            }

            return gridcheck;
        }

        /// <summary>
        /// Converts the single Vertex Line into two Grid Lines for checking terrain and placing the cursor.
        /// </summary>
        /// <returns>Two grid lines.</returns>
        private (AxisAlignedLine, AxisAlignedLine) GetRoadGridLines()
        {
            int minX = Math.Min(_vertexLine.Start.x, _vertexLine.End.x);
            int maxX = Math.Max(_vertexLine.Start.x, _vertexLine.End.x);
            int minZ = Math.Min(_vertexLine.Start.z, _vertexLine.End.z);
            int maxZ = Math.Max(_vertexLine.Start.z, _vertexLine.End.z);

            if (_vertexLine.Alignment == AxisAlignment.ZAxis)
            {
                var start1 = ClampPoint(minX - 1, minZ - 1);
                var end1 = ClampPoint(maxX - 1, maxZ);

                var start2 = ClampPoint(minX, minZ - 1);
                var end2 = ClampPoint(maxX, maxZ);

                return (new AxisAlignedLine(start1, end1), new AxisAlignedLine(start2, end2));
            }
            else
            {
                var start1 = ClampPoint(minX - 1, minZ - 1);
                var end1 = ClampPoint(maxX, maxZ - 1);

                var start2 = ClampPoint(minX - 1, minZ);
                var end2 = ClampPoint(maxX, maxZ);

                return (new AxisAlignedLine(start1, end1), new AxisAlignedLine(start2, end2));
            }
        }

        private Point2 ClampPoint(int x, int z)
        {
            return new Point2(
                Math.Min(_terrain.CountX - 1, Math.Max(0, x)),
                Math.Min(_terrain.CountZ - 1, Math.Max(0, z)));
        }
    }
}
