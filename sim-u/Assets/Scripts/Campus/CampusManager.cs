using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Campus
{
    /// <summary>
    /// The different ways a campus grid can be used.
    /// </summary>
    [Flags]
    public enum CampusGridUse
    {
        Empty       = 0,
        Path        = (1 << 1),
        Road        = (1 << 2),
        Building    = (1 << 3),

        Crosswalk = Path | Road,
    }

    /// <summary>
    /// Root level object for the campus.
    /// </summary>
    public class CampusManager : GameDataLoader<CampusData>
    {
        private Dictionary<string, BuildingData> _buildingRegistry = new Dictionary<string, BuildingData>();

        private GridMesh _terrain;
        private CampusBuildings _buildings;
        private CampusPaths _paths;
        private CampusRoads _roads;

        private int _defaultMaterialIndex;

        /// <summary>
        /// Returns what is at the campus grid position.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>The use of the queried grid.</returns>
        public CampusGridUse GetGridUse(Point2 pos)
        {
            CampusGridUse use = CampusGridUse.Empty;

            if (_buildings.BuildingAtPosition(pos))
            {
                use = use | CampusGridUse.Building;
            }

            if (_paths.PathAtPosition(pos))
            {
                use = use | CampusGridUse.Path;
            }

            if (_roads.RoadAtGrid(pos))
            {
                use = use | CampusGridUse.Road;
            }

            // DEBUG: assert that only valid enums are created
            switch (use)
            {
                case CampusGridUse.Empty:
                case CampusGridUse.Path:
                case CampusGridUse.Road:
                case CampusGridUse.Building:
                case CampusGridUse.Crosswalk:
                    break;

                default:
                    GameLogger.FatalError("Unexpected path use detected! Point {0}; Use: {1}", pos, use);
                    break;
            }

            return use;
        }

        /// <summary>
        /// Returns what is at the campus vertex position.
        /// This only queries systems the build on vertices.
        /// i.e. roads.
        /// </summary>
        /// <param name="pos">Vertex position to query.</param>
        /// <returns>The use of the queried vertex.</returns>
        public CampusGridUse GetVertexUse(Point2 pos)
        {
            CampusGridUse use = CampusGridUse.Empty;

            if (_roads.RoadAtVertex(pos))
            {
                Assert.AreEqual(CampusGridUse.Empty, use);
                use = CampusGridUse.Road;
            }

            return use;
        }

        /// <summary>
        /// Gets the metadata about the requested building.
        /// </summary>
        /// <param name="name">Name of the building.</param>
        /// <returns>The building data from config.</returns>
        public bool TryGetBuildingData(string name, out BuildingData buildingData)
        {
            return _buildingRegistry.TryGetValue(name, out buildingData);
        }

        /// <summary>
        /// Checks that the requested location is valid for a new building.
        /// </summary>
        /// <param name="building">The building data.</param>
        /// <param name="location">The grid location for building.</param>
        /// <param name="validGrids">Output: The valid grids for building. Used for updating cursors.</param>
        /// <returns>True if the location is valid for the building, false otherwise.</returns>
        public bool IsValidForBuilding(BuildingData building, Point3 location, out bool[,] validGrids)
        {
            int xSize = building.Footprint.GetLength(0);
            int zSize = building.Footprint.GetLength(1);
            validGrids = new bool[xSize, zSize];

            bool isValid = true;
            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    if (!building.Footprint[x, z])
                    {
                        continue; // Don't need to check outside of the building footprint
                    }

                    int gridX = location.x + x;
                    int gridZ = location.z + z;

                    bool isInBoundsFlatAndFree =
                        _terrain.GridInBounds(gridX, gridZ) &&
                        _terrain.IsGridFlat(gridX, gridZ) &&
                        _terrain.GetSquareHeight(gridX, gridZ) == location.y &&
                        GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Empty;

                    validGrids[x, z] = isInBoundsFlatAndFree;
                    isValid = isValid && isInBoundsFlatAndFree;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Construct a building at the location.
        /// Registers the taken grid location with the Safe Terrain Editor.
        /// </summary>
        /// <param name="building">The building to construct.</param>
        /// <param name="location">The location of the building.</param>
        public void ConstructBuilding(BuildingData building, Point3 location)
        {
            UpdateGrids(_buildings.ConstructBuilding(building, location));
        }

        /// <summary>
        /// Checks that the requested line is valid for a new path.
        /// </summary>
        /// <param name="line">Line we want to build a path along.</param>
        /// <param name="validGrids">Output: The valid grids for path building. Used for updating cursors.</param>
        /// <returns>True if the line is valid for a path, false otherwise.</returns>
        public bool IsValidForPath(AxisAlignedLine line, out bool[] validGrids)
        {
            validGrids = new bool[line.Length];

            bool isValid = true;
            foreach ((int lineIndex, Point2 point) in line.GetPointsAlongLine())
            {
                bool isValidTerrain =
                    _terrain.GridInBounds(point.x, point.z) &&
                    _terrain.IsGridSmooth(point.x, point.z, line.Alignment);

                bool isGridAvailable;
                switch (GetGridUse(point))
                {
                    case CampusGridUse.Empty:
                    case CampusGridUse.Path:
                    case CampusGridUse.Crosswalk:
                        isGridAvailable = true;
                        break;
                    case CampusGridUse.Road:
                        isGridAvailable = _roads.IsValidForCrosswalk(point);
                        break;
                    default:
                        isGridAvailable = false;
                        break;
                }

                bool gridIsValid = isValidTerrain && isGridAvailable;
                validGrids[lineIndex] = gridIsValid;
                isValid = isValid && gridIsValid;
            }

            return isValid;
        }

        /// <summary>
        /// Build a path along a line.
        /// </summary>
        /// <param name="line">The line to build path along.</param>
        public void ConstructPath(AxisAlignedLine line)
        {
            UpdateGrids(_paths.ConstructPath(line));
        }

        /// <summary>
        /// Checks that the requested lines are valid for a new road.
        /// NB: This method will be called in pairs. Once for each side of the road.
        /// </summary>
        /// <param name="line">Center vertex line for the new road.</param>
        /// <param name="gridlines">Output: The gridlines that surround the center vertex line.</param>
        /// <param name="validGrids">Output: The valid grids for road building. Used for updating cursors.</param>
        /// <returns>True if the line is valid for a road, false otherwise.</returns>
        public bool IsValidForRoad(AxisAlignedLine line, out AxisAlignedLine[] gridlines, out bool[][] validGrids)
        {
            gridlines = new AxisAlignedLine[2];
            (gridlines[0], gridlines[1]) = line.GetSurroundingGridLines(clampX: _terrain.CountX, clampZ: _terrain.CountZ);

            validGrids = new bool[2][];

            bool isValid = true;
            for (int gridlineIndex = 0; gridlineIndex < 2; ++gridlineIndex)
            {
                AxisAlignedLine gridline = gridlines[gridlineIndex];
                validGrids[gridlineIndex] = new bool[gridline.Length];

                foreach ((int lineIndex, Point2 point) in gridline.GetPointsAlongLine())
                {
                    bool isValidTerrain =
                        _terrain.GridInBounds(point.x, point.z) &&
                        _terrain.IsGridSmooth(point.x, point.z, gridline.Alignment);

                    bool isGridAvailable;
                    switch (GetGridUse(point))
                    {
                        case CampusGridUse.Empty:
                        case CampusGridUse.Road:
                            isGridAvailable = true;
                            break;
                        case CampusGridUse.Path:
                            isGridAvailable = _roads.IsValidForCrosswalk
                            break;
                        default:
                            isGridAvailable = false;
                            break;
                    }

                    bool isTightTurn = false;
                    if (isValidTerrain && isGridAvailable)
                    {
                        // Rule: You can't make a tight turn with roads. It messes up my lanes. And it's ugly.
                        int roadVertexCount = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            int vertX = point.x + GridConverter.GridToVertexDx[i];
                            int vertZ = point.z + GridConverter.GridToVertexDz[i];

                            if (_terrain.VertexInBounds(vertX, vertZ))
                            {
                                Point2 roadVertex = new Point2(vertX, vertZ);
                                if (Game.Campus.GetVertexUse(roadVertex) == CampusGridUse.Road ||
                                    line.IsPointOnLine(roadVertex))
                                {
                                    ++roadVertexCount;
                                }
                            }
                        }

                        if (roadVertexCount == 4)
                        {
                            isTightTurn = true;
                        }
                    }

                    bool gridIsValid = isValidTerrain && isGridAvailable && !isTightTurn;
                    validGrids[gridlineIndex][lineIndex] = gridIsValid;
                    isValid = isValid && gridIsValid;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Build a road along a vertex line.
        /// </summary>
        /// <param name="line">The line to build path along.</param>
        public void ConstructRoad(AxisAlignedLine line)
        {
            UpdateGrids(_roads.ConstructRoad(line));
        }

        /// <summary>
        /// Destroys the campus improvement at the desired position.
        /// </summary>
        /// <param name="pos">The position to delete at.</param>
        /// <param name="filter">Optional: only delete the requested type of item.</param>
        public void DestroyAt(Point2 pos, CampusGridUse? filter = null)
        {
            CampusGridUse itemAt = GetGridUse(pos);

            if (filter.HasValue &&
                (itemAt & filter.Value) != filter.Value)
            {
                return;
            }

            IEnumerable<Point2> updatedPoints = Enumerable.Empty<Point2>();
            switch (itemAt)
            {
                case CampusGridUse.Path:
                case CampusGridUse.Crosswalk:
                    updatedPoints = _paths.DestroyPathAt(pos);
                    break;

                case CampusGridUse.Road:
                    updatedPoints = _roads.DestroyRoadAt(pos);
                    break;

                case CampusGridUse.Building:
                    updatedPoints = _buildings.DestroyBuildingAt(pos);
                    break;

                default:
                    GameLogger.Error("Could not destroy item '{0}' at {1}.", itemAt, pos);
                    break;
            }

            UpdateGrids(updatedPoints);
        }

        /// <summary>
        /// Set the parent of the terrain object.
        /// In essence, this makes it so when you click the terrain,
        /// the parent selectable is not closed.
        /// </summary>
        /// <param name="parent">The selectable to keep open after terrain clicks.</param>
        public void SetTerrainSelectionParent(Selectable parent)
        {
            _terrain.Selectable.SelectionParent = parent;
        }

        /// <summary>
        /// Load the campus game data.
        /// </summary>
        /// <param name="gameData">Campus game data.</param>
        protected override void LoadData(CampusData gameData)
        {
            var gridMeshArgs = new GridMeshArgs()
            {
                GridSquareSize = Constant.GridSize,
                GridStepSize = Constant.GridStepSize,
                CountX = gameData.Terrain.GridCountX,
                CountZ = gameData.Terrain.GridCountZ,
                CountY = gameData.Terrain.GridCountY,
                StartingHeight = gameData.Terrain.StartingHeight,
                GridMaterial = gameData.Terrain.TerrainMaterial,
                SubmaterialSize = gameData.Terrain.SubmaterialSquareSize,
                SkirtPrefab = gameData.Terrain.TerrainSkirt,
            };

            GridMesh terrain;
            CampusFactory.GenerateTerrain(transform, gridMeshArgs, out terrain);

            _terrain = terrain;
            _buildings = new CampusBuildings(terrain);
            _paths = new CampusPaths(gameData, terrain);
            _roads = new CampusRoads(gameData, terrain);

            _defaultMaterialIndex = gameData.Terrain.SubmaterialEmptyGrassIndex;

            Game.State.RegisterController(GameState.SelectingTerrain, new SelectingTerrainController(terrain));
            Game.State.RegisterController(GameState.EditingTerrain, new EditingTerrainController(terrain));
            Game.State.RegisterController(GameState.PlacingConstruction, new PlacingConstructionController(terrain));
            Game.State.RegisterController(GameState.SelectingPath, new SelectingPathController(terrain));
            Game.State.RegisterController(GameState.PlacingPath, new PlacingPathController(terrain));
            Game.State.RegisterController(GameState.SelectingRoad, new SelectingRoadController(terrain));
            Game.State.RegisterController(GameState.PlacingRoad, new PlacingRoadController(terrain));
            Game.State.RegisterController(GameState.Demolishing, new DemolishingController(terrain));

            var footprintCreatorObject = new GameObject("FootprintCreator");
            using (var footprintCreator = footprintCreatorObject.AddComponent<FootprintCreator>())
            {
                // Load the buildings
                foreach (var buildingData in gameData.Buildings)
                {
                    buildingData.Footprint = footprintCreator.CalculateFootprint(buildingData.Mesh, Constant.GridSize);
                    _buildingRegistry[buildingData.Name] = buildingData;
                }
            }
        }

        /// <summary>
        /// Link the campus game data.
        /// </summary>
        /// <param name="gameData">Campus game data.</param>
        protected override void LinkData(CampusData gameData)
        {
        }

        /// <summary>
        /// Recalculate the state of a campus grid.
        /// Usually called directly after any edit operation.
        /// </summary>
        /// <param name="updatedPoints">The points that have been updated.</param>
        private void UpdateGrids(IEnumerable<Point2> updatedPoints)
        {
            foreach (Point2 updatedPoint in updatedPoints)
            {
                UpdateGridMaterial(updatedPoint);
                UpdateGridAnchoring(updatedPoint);
            }
        }

        /// <summary>
        /// Update the submaterial for a grid on the terrain.
        /// This taked into account all possible submaterial sources.
        /// </summary>
        /// <param name="pos">The grid position to update submaterial on.</param>
        private void UpdateGridMaterial(Point2 pos)
        {
            int materialIndex = _defaultMaterialIndex;
            SubmaterialRotation materialRotation = SubmaterialRotation.deg0;
            SubmaterialInversion materialInversion = SubmaterialInversion.None;

            switch (GetGridUse(pos))
            {
                case CampusGridUse.Path:
                    (materialIndex,
                     materialRotation,
                     materialInversion) = _paths.GetPathMaterial(pos);
                    break;

                case CampusGridUse.Road:
                    (materialIndex,
                     materialRotation,
                     materialInversion) = _roads.GetRoadMaterial(pos);
                    break;

                case CampusGridUse.Crosswalk:
                    (materialIndex,
                     materialRotation,
                     materialInversion) = _roads.GetRoadMaterial(pos, isPathPresent: true);
                    break;
            }

            _terrain.SetSubmaterial(pos.x, pos.z, materialIndex, materialRotation, materialInversion);
        }

        /// <summary>
        /// Update the 
        /// </summary>
        /// <param name="pos"></param>
        private void UpdateGridAnchoring(Point2 pos)
        {
            bool isAnchored = false;
            switch (GetGridUse(pos))
            {
                case CampusGridUse.Building:
                case CampusGridUse.Path:
                case CampusGridUse.Road:
                    isAnchored = true;
                    break;
            }

            if (isAnchored)
            {
                _terrain.Editor.SetAnchored(pos.x, pos.z);
            }
            else
            {
                _terrain.Editor.RemoveAnchor(pos.x, pos.z);
            }
        }
    }
}
