using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingRoad game state.
    /// </summary>
    [StateController(HandledState = GameState.PlacingRoad)]
    internal class PlacingRoadController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private LineCursor _cursor1;
        private LineCursor _cursor2;

        private AxisAlignedLine _vertexLine;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public PlacingRoadController()
        {
            _terrain = Accessor.Terrain;
            _cursor1 = new LineCursor(
                _terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"));
            _cursor2 = new LineCursor(
                _terrain,
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
                GameLogger.FatalError("PlacingRoadController was given incorrect context.");

            _vertexLine = new AxisAlignedLine(args.VertexSelection);

            Accessor.CampusManager.IsValidForRoad(_vertexLine, out AxisAlignedLine[] lines, out bool[][] validGrids);
            _cursor1.Place(lines[0], validGrids[0]);
            _cursor2.Place(lines[1], validGrids[1]);
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
                if (Accessor.CampusManager.IsValidForRoad(_vertexLine, out AxisAlignedLine[] _, out bool[][] __))
                {
                    Accessor.CampusManager.ConstructRoad(_vertexLine);
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

                Accessor.CampusManager.IsValidForRoad(_vertexLine, out AxisAlignedLine[] lines, out bool[][] validGrids);
                _cursor1.Place(lines[0], validGrids[0]);
                _cursor2.Place(lines[1], validGrids[1]);
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
    }
}
