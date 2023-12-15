﻿using Campus.GridTerrain;
using Common;
using GameData;
using UI;
using UnityEngine;

namespace Campus
{
    public class SelectingRoadContext
    {
        public RoadsWindow Window { get; set; }
    }

    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    [StateController(HandledState = GameState.SelectingRoad)]
    internal class SelectingRoadController : GameStateMachine.Controller
    {
        private GridMesh _terrain;
        private RoadsWindow _window;

        private LineCursor _cursor1;
        private LineCursor _cursor2;

        private AxisAlignedLine _vertexLine;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public SelectingRoadController()
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
        public override void TransitionIn(object context)
        {
            var roadsContext = context as SelectingRoadContext;
            if (roadsContext == null)
                GameLogger.FatalError("SelectingRoadController was given unexpected context! Type = {0}", context?.GetType().Name ?? "null");

            _window = roadsContext.Window;
            _window.UpdateInfo(0, 0);
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
        public override void Update() { }

        /// <summary>
        /// Event handler for selection updates on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain selection update args.</param>
        private void PlacementUpdate(object sender, TerrainVertexUpdateArgs args)
        {
            if (args.VertexSelection != Point2.Null)
            {
                _vertexLine = new AxisAlignedLine(args.VertexSelection);

                Accessor.CampusManager.IsValidForRoad(_vertexLine, out AxisAlignedLine[] lines, out bool[][] validGrids);
                _cursor1.Place(lines[0], validGrids[0]);
                _cursor2.Place(lines[1], validGrids[1]);
            }
            else
            {
                _cursor1.Deactivate();
                _cursor2.Deactivate();
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
                if (Accessor.CampusManager.IsValidForRoad(_vertexLine, out AxisAlignedLine[] _, out bool[][] __))
                {
                    Transition(
                        GameState.PlacingRoad,
                        new PlacingRoadContext
                        {
                            ClickedArgs = args,
                            Window = _window,
                        });
                }
            }
            if (args.Button == MouseButton.Right)
            {
                GameLogger.Info("Checking Vertex Use: {0} = {1}", _vertexLine.Start, Accessor.CampusManager.GetVertexUse(_vertexLine.Start));
            }
        }
    }
}
