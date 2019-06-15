using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingPath game state.
    /// </summary>
    internal class SelectingRoadController : GameStateMachine.Controller
    {
        private GridMesh _terrain;

        private Material _validMaterial;
        private Material _invalidMaterial;
        private GridCursor[] _cursors;

        // Convert vertex to the indices of the IsValidCheck bool[,]
        // This is a special encoding to match the CheckFlatAndFree call.
        private readonly int[] dxCheck = new int[] { 0, 0, 1, 1 };
        private readonly int[] dzCheck = new int[] { 0, 1, 0, 1 };

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public SelectingRoadController(GridMesh terrain)
        {
            _terrain = terrain;

            _validMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid");
            _invalidMaterial = ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid");

            _cursors = new GridCursor[4];
            for (int i = 0; i < _cursors.Length; ++i)
                _cursors[i] = GridCursor.Create(terrain, _validMaterial);

            OnTerrainVertexSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Clicked;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="_">Not used.</param>
        public override void TransitionIn(object _)
        {
            ActivateCursors();
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            DeactivateCursors();
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
                ActivateCursors();

                bool[,] isValidTerrain = IsValidTerrain(args.VertexSelection);
                for (int i = 0; i < 4; ++i)
                {
                    int cursorX = args.VertexSelection.x + GridConverter.VertexToGridDx[i];
                    int cursorZ = args.VertexSelection.z + GridConverter.VertexToGridDz[i];
                    
                    if (_terrain.GridInBounds(cursorX, cursorZ))
                    {
                        _cursors[i].Place(new Point2(cursorX, cursorZ));
                        _cursors[i].SetMaterial(
                            isValidTerrain[dxCheck[i], dzCheck[i]] ?
                                _validMaterial :
                                _invalidMaterial);
                    }
                    else
                    {
                        _cursors[i].Deactivate();
                    }
                }
                
            }
            else
            {
                DeactivateCursors();
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
                bool[,] isValidTerrain = IsValidTerrain(args.VertexSelection);
                if (isValidTerrain[0, 0] &&
                    isValidTerrain[0, 1] &&
                    isValidTerrain[1, 0] &&
                    isValidTerrain[1, 1])
                {
                    Transition(GameState.PlacingRoad, args);
                }
            }
        }

        /// <summary>
        /// Gets values representing whether or not the grids under the cursor is valid for the start of a road.
        /// </summary>
        /// <returns>Boolean values representing whether or not the grid location is valid for a road.</returns>
        private bool[,] IsValidTerrain(Point2 selectedVertex)
        {
            // BUG: This does not allow you to start building roads on a valid smooth ramp.
            // bool[,] isFlatAndFree = Game.Campus.CheckLineSmoothAndFreeAlongVertices(new AxisAlignedLine(selectedVertex));
            int startingGridX = selectedVertex.x + GridConverter.VertexToGridDx[0];
            int startingGridZ = selectedVertex.z + GridConverter.VertexToGridDz[0];
            bool[,] isFlatAndFree = Game.Campus.CheckFlatAndFree(startingGridX, startingGridZ, 2, 2);
            for (int i = 0; i < 4; ++i)
            {
                int gridX = selectedVertex.x + GridConverter.VertexToGridDx[i];
                int gridZ = selectedVertex.z + GridConverter.VertexToGridDz[i];

                isFlatAndFree[dxCheck[i], dzCheck[i]] =
                    isFlatAndFree[dxCheck[i], dzCheck[i]] ||
                    (
                        _terrain.GridInBounds(gridX, gridZ) &&
                        Game.Campus.GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Road
                    );
            }
            return isFlatAndFree;
        }

        private void ActivateCursors()
        {
            foreach (GridCursor cursor in _cursors)
            {
                if (!cursor.IsActive)
                {
                    cursor.Activate();
                    cursor.Place(cursor.Position);
                }
            }
        }

        private void DeactivateCursors()
        {
            foreach (GridCursor cursor in _cursors)
            {
                if (cursor.IsActive)
                {
                    cursor.Deactivate();
                }
            }
        }
    }
}
