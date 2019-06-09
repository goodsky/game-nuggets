using Campus.GridTerrain;
using Common;
using GameData;
using System.Linq;
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

        private readonly int[] dx = new int[] { -1, -1, 0, 0 };
        private readonly int[] dz = new int[] { -1, 0, -1, 0 };

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
                    int cursorX = args.VertexSelection.x + dx[i];
                    int cursorZ = args.VertexSelection.z + dz[i];
                    
                    if (_terrain.GridInBounds(cursorX, cursorZ))
                    {
                        _cursors[i].Place(new Point2(cursorX, cursorZ));
                        _cursors[i].SetMaterial(
                            isValidTerrain[i % 2, i / 2] ?
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
            // bool[,] isFlatAndFree = Game.Campus.CheckLineSmoothAndFreeAlongVertices(new AxisAlignedLine(selectedVertex));
            bool[,] isFlatAndFree = Game.Campus.CheckFlatAndFree(selectedVertex.x - 1, selectedVertex.z - 1, 2, 2);
            for (int i = 0; i < 4; ++i)
            {
                int gridX = selectedVertex.x + dx[i];
                int gridZ = selectedVertex.z + dz[i];
                isFlatAndFree[i % 2, i / 2] &=
                    _terrain.GridInBounds(gridX, gridZ) &&
                    Game.Campus.GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Road;
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
