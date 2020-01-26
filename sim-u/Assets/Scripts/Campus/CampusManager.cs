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
        Parking     = (1 << 3),
        Building    = (1 << 4),

        Crosswalk = Path | Road,
        ParkingLot = Path | Road | Parking,
    }

    /// <summary>
    /// Root level object for the campus.
    /// </summary>
    public class CampusManager : GameDataLoader<CampusData>, IGameStateSaver<CampusSaveState>
    {
        private CampusData _config;
        private Dictionary<string, BuildingData> _buildingRegistry = new Dictionary<string, BuildingData>();

        private GridMesh _terrain;
        private CampusBuildings _buildings;
        private CampusPaths _paths;
        private CampusRoads _roads;
        private CampusParkingLots _lots;
        private CampusConnections _connections;

        private int _defaultMaterialIndex;

        public int SmallClassroomCapacity => _config.SmallClassroomCapacity;
        public int MediumClassroomCapacty => _config.MediumClassroomCapacity;
        public int LargeClassroomCapacity => _config.LargeClassroomCapacity;

        /// <summary>
        /// Gets the total number of accessible classrooms on the campus.
        /// </summary>
        public int TotalConnectedClassroomCount =>
            GetBuildingInfo(checkConnections: true)
                .Where(building => building.IsConnectedToPaths.Value)
                .Sum(building =>
                    building.SmallClassroomCount +
                    building.MediumClassroomCount +
                    building.LargeClassroomCount);

        /// <summary>
        /// Gets the sum of available
        /// </summary>
        public int TotalConnectedClassroomCapacity =>
            GetBuildingInfo(checkConnections: true)
                .Where(building => building.IsConnectedToPaths.Value)
                .Sum(building =>
                    (building.SmallClassroomCount * SmallClassroomCapacity) +
                    (building.MediumClassroomCount * MediumClassroomCapacty) +
                    (building.LargeClassroomCount * LargeClassroomCapacity));

        /// <summary>
        /// Gets the total number of accessible laboratories on the campus.
        /// </summary>
        public int TotalConnectedLaboratoryCount =>
            GetBuildingInfo(checkConnections: true)
                .Where(building => building.IsConnectedToPaths.Value)
                .Sum(building => building.LaboratoryCount);

        /// <summary>
        /// Returns what is at the campus grid position.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>The use of the queried grid.</returns>
        public CampusGridUse GetGridUse(Point2 pos)
        {
            CampusGridUse use = CampusGridUse.Empty;

            if (_buildings.BuildingAtGrid(pos))
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

            if (_lots.IsParkingLotAtGrid(pos))
            {
                use = use | CampusGridUse.ParkingLot;
            }

            // DEBUG: assert that only valid enums are created
            switch (use)
            {
                case CampusGridUse.Empty:
                case CampusGridUse.Path:
                case CampusGridUse.Road:
                case CampusGridUse.ParkingLot:
                case CampusGridUse.Building:
                case CampusGridUse.Crosswalk:
                    break;

                default:
                    GameLogger.FatalError("Unexpected grid use detected! Point {0}; Use: {1}", pos, use);
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
        /// Returns the parking lot at a given grid position, if it exists.
        /// </summary>
        /// <param name="pos">The grid position.</param>
        /// <returns>The parking lot info if it exists, null otherwise.</returns>
        public ParkingInfo GetParkingInfoAtGrid(Point2 pos)
        {
            return _lots.GetParkingLotAtGrid(pos);
        }

        /// <summary>
        /// Gets the parking lots on the campus.
        /// </summary>
        /// <param name="checkConnections">True to check that parking lots are connected to road sources.</param>
        public IEnumerable<ParkingInfo> GetParkingInfo(bool checkConnections)
        {
            IEnumerable<ParkingInfo> allParking = _lots.GetParkingLots();

            if (checkConnections)
            {
                foreach (ParkingInfo parking in allParking)
                {
                    IList<RoadConnection> connections = _connections.GetConnections(parking);
                    parking.IsConnectedToRoad = connections.Count > 0;
                }
            }

            return allParking;
        }

        /// <summary>
        /// Gets the buildings on the campus.
        /// </summary>
        /// <param name="checkConnections">True to check that buildings are connected to valid path sources.</param>
        public IEnumerable<BuildingInfo> GetBuildingInfo(bool checkConnections)
        {
            IEnumerable<BuildingInfo> allBuildings = _buildings.GetBuildings();

            if (checkConnections)
            {
                foreach (BuildingInfo building in allBuildings)
                {
                    bool isConnectedToValidLot = false;
                    IList<PathConnection> connections = _connections.GetConnections(building);
                    foreach (PathConnection connection in connections)
                    {
                        IList<RoadConnection> rConnections = _connections.GetConnections(connection.Source);
                        if (rConnections.Count > 0)
                        {
                            isConnectedToValidLot = true;
                        }
                    }

                    building.IsConnectedToPaths = isConnectedToValidLot;
                }
            }

            return allBuildings;
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
        /// Checks if there is a valid entry into a building from the point.
        /// </summary>
        /// <param name="buildingLocation">The location of the building.</param>
        /// <param name="checkLocation">The location to check if you can enter from.</param>
        /// <returns>True is this is a valid way to enter the building.</returns>
        public bool CanEnterBuildingFrom(Point2 buildingLocation, Point2 checkLocation)
        {
            BuildingInfo building = _buildings.GetBuildingAtGrid(buildingLocation);

            if (building == null)
                return false;

            return building.EntryPoints.Contains(checkLocation);
        }

        /// <summary>
        /// Checks that the requested location is valid for a new building.
        /// </summary>
        /// <param name="building">The building data.</param>
        /// <param name="location">The grid location for building.</param>
        /// <param name="location">The rotation for the building.</param>
        /// <param name="validGrids">Output: The valid grids for building. Used for updating cursors.</param>
        /// <returns>True if the location is valid for the building, false otherwise.</returns>
        public bool IsValidForBuilding(BuildingData building, Point3 location, BuildingRotation rotation, out bool[,] validGrids)
        {
            (Point3 footprintOrigin, bool[,] footprint) = BuildingRotationUtils.RotateFootprint(building, location, rotation);
            int xSize = footprint.GetLength(0);
            int zSize = footprint.GetLength(1);
            validGrids = new bool[xSize, zSize];

            bool isValid = true;
            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    if (!footprint[x, z])
                    {
                        continue; // Don't need to check outside of the building footprint
                    }

                    int gridX = footprintOrigin.x + x;
                    int gridZ = footprintOrigin.z + z;

                    bool isInBoundsFlatAndFree =
                        _terrain.GridInBounds(gridX, gridZ) &&
                        _terrain.IsGridFlat(gridX, gridZ) &&
                        _terrain.GetSquareHeight(gridX, gridZ) == footprintOrigin.y &&
                        GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Empty;

                    bool isOnEdge =
                        gridX == 0 || gridX == _terrain.CountX - 1 ||
                        gridZ == 0 || gridZ == _terrain.CountZ - 1;

                    // Can't build things on the edge of the terrain unless you are in admin mode.
                    if (isOnEdge && !Accessor.Game.AdminMode)
                    {
                        isInBoundsFlatAndFree = false;
                    }

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
        /// <param name="location">The rotation of the building.</param>
        public void ConstructBuilding(BuildingData building, Point3 location, BuildingRotation rotation, bool updateConnections = true)
        {
            UpdateGrids(_buildings.ConstructBuilding(building, location, rotation), updateConnections);
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

                bool isOnEdge =
                    point.x == 0 || point.x == _terrain.CountX - 1 ||
                    point.z == 0 || point.z == _terrain.CountZ - 1;

                // Can't construct paths on the edge of the terrain unless you are in admin mode.
                if (isOnEdge && !Accessor.Game.AdminMode)
                {
                    isValidTerrain = false;
                }

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
                    case CampusGridUse.ParkingLot:
                        isGridAvailable = _lots.IsOnEdgeOfParkingLot(point);
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
        public void ConstructPath(AxisAlignedLine line, bool updateConnections = true)
        {
            UpdateGrids(_paths.ConstructPath(line), updateConnections);
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

                    bool isOnEdge =
                        point.x == 0 || point.x == _terrain.CountX - 1 ||
                        point.z == 0 || point.z == _terrain.CountZ - 1;

                    // Can't construct roads on the edge of the terrain unless you are in admin mode.
                    if (isOnEdge && !Accessor.Game.AdminMode)
                    {
                        isValidTerrain = false;
                    }

                    bool isGridAvailable;
                    switch (GetGridUse(point))
                    {
                        case CampusGridUse.Empty:
                        case CampusGridUse.Road:
                            isGridAvailable = true;
                            break;
                        case CampusGridUse.Path:
                            isGridAvailable = false; // TODO: build crosswalks here?
                            break;
                        case CampusGridUse.ParkingLot:
                            isGridAvailable = _lots.IsOnEdgeOfParkingLot(point, disallowCorners: true);
                            break;
                        default:
                            isGridAvailable = false;
                            break;
                    }

                    bool isTightTurn = false;
                    if (isValidTerrain && isGridAvailable)
                    {
                        // Rule: You can't make a tight turn with roads. It messes up my lanes. And it's ugly. Don't do it.
                        int roadVertexCount = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            int vertX = point.x + GridConverter.GridToVertexDx[i];
                            int vertZ = point.z + GridConverter.GridToVertexDz[i];

                            if (_terrain.VertexInBounds(vertX, vertZ))
                            {
                                Point2 roadVertex = new Point2(vertX, vertZ);
                                if (GetVertexUse(roadVertex) == CampusGridUse.Road ||
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
        public void ConstructRoad(AxisAlignedLine line, bool updateConnections = true)
        {
            UpdateGrids(_roads.ConstructRoad(line), updateConnections);
        }

        /// <summary>
        /// Checks that the requested area is valid for a new path.
        /// </summary>
        /// <param name="rectangle">The parking lot footprint.</param>
        /// <param name="validGrids">Output: The valid grids for path building. Used for updating cursors.</param>
        /// <returns>True if the line is valid for a parking lot, false otherwise.</returns>
        public bool IsValidForParkingLot(Rectangle rectangle, out bool[,] validGrids, bool ignoreSizeConstraint = false)
        {
            // Parking lots must be at least 4x4 grids
            bool isLargeEnough = (rectangle.SizeX >= 4 && rectangle.SizeZ >= 4);

            if (ignoreSizeConstraint)
            {
                isLargeEnough = true;
            }

            validGrids = new bool[rectangle.SizeX, rectangle.SizeZ];

            bool isValid = true;
            for (int x = 0; x < rectangle.SizeX; ++x)
            {
                for (int z = 0; z < rectangle.SizeZ; ++z)
                {
                    int gridX = rectangle.MinX + x;
                    int gridZ = rectangle.MinZ + z;

                    bool isInBoundsFlatAndFree =
                        isLargeEnough &&
                        _terrain.GridInBounds(gridX, gridZ) &&
                        _terrain.IsGridFlat(gridX, gridZ) &&
                        GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Empty;

                    bool isOnEdge =
                        gridX == 0 || gridX == _terrain.CountX - 1 ||
                        gridZ == 0 || gridZ == _terrain.CountZ - 1;

                    // Can't construct lots on the edge of the terrain unless you are in admin mode.
                    if (isOnEdge && !Accessor.Game.AdminMode)
                    {
                        isInBoundsFlatAndFree = false;
                    }

                    validGrids[x, z] = isInBoundsFlatAndFree;
                    isValid = isValid && isInBoundsFlatAndFree;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Build a parking lot at the requested location.
        /// </summary>
        /// <param name="rectangle">The parking lot footprint.</param>
        public void ConstructParkingLot(Rectangle rectangle, bool updateConnections = true)
        {
            UpdateGrids(_lots.ConstructParkingLot(rectangle), updateConnections);
        }

        /// <summary>
        /// Checks that the requested point is a valid target for DestroyAt.
        /// </summary>
        /// <param name="pos"></param>
        public bool IsValidForDestruction(Point2 pos)
        {
            bool isOnEdge =
                pos.x == 0 || pos.x == _terrain.CountX - 1 ||
                pos.z == 0 || pos.z == _terrain.CountZ - 1;

            // Can't delete things on the edge of the terrain unless you are in admin mode.
            if (isOnEdge && !Accessor.Game.AdminMode)
            {
                return false;
            }

            CampusGridUse use = GetGridUse(pos);

            bool isWithinTwoOfEdge =
                pos.x <= 1 || pos.x >= _terrain.CountX - 2 ||
                pos.z <= 1 || pos.z >= _terrain.CountZ - 2;

            // Can't delete roads within 2 squares of the edge unless you are in admin mode.
            if (use == CampusGridUse.Road &&
                isWithinTwoOfEdge && !Accessor.Game.AdminMode)
            {
                return false;
            }

            return use != CampusGridUse.Empty;
        }

        /// <summary>
        /// Destroys the campus improvement at the desired position.
        /// </summary>
        /// <param name="pos">The position to delete at.</param>
        /// <param name="filter">Optional: only delete the requested type of item.</param>
        public void DestroyAt(Point2 pos, CampusGridUse? filter = null, bool updateConnections = true)
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

                case CampusGridUse.ParkingLot:
                    updatedPoints = _lots.DestroyParkingLotAt(pos);
                    break;

                case CampusGridUse.Building:
                    updatedPoints = _buildings.DestroyBuildingAt(pos);
                    break;

                default:
                    GameLogger.Error("Could not destroy item '{0}' at {1}.", itemAt, pos);
                    break;
            }

            UpdateGrids(updatedPoints, updateConnections);
        }

        /// <summary>
        /// Gets the cost of constructing things on campus.
        /// </summary>
        /// <param name="type">The type of construction</param>
        /// <param name="squares">The number of squares to construct.</param>
        /// <returns>The cost of the construction project in dollars.</returns>
        public int GetCostOfConstruction(CampusGridUse type, int squares)
        {
            switch (type)
            {
                case CampusGridUse.Path:
                    return _paths.CostPerSquare * squares;

                case CampusGridUse.Road:
                    return _roads.CostPerSquare * squares;

                case CampusGridUse.ParkingLot:
                    return _lots.CostPerSquare * squares;
            }

            GameLogger.Error("Attempted to get cost of an invalid construction type: {0}", type);
            return 0;
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
        /// Snapshot the campus state and return the save data.
        /// </summary>
        /// <returns>A snapshot of the campus state.</returns>
        public CampusSaveState SaveGameState()
        {
            return new CampusSaveState
            {
                Terrain = _terrain.SaveGameState(),
                Buildings = _buildings.SaveGameState(),
                PathGrids = _paths.SaveGameState(),
                RoadVertices = _roads.SaveGameState(),
                ParkingLots = _lots.SaveGameState(),
            };
        }

        /// <summary>
        /// Load the game from a snapshot.
        /// </summary>
        /// <param name="state">The game state to load from.</param>
        public void LoadGameState(CampusSaveState state)
        {
            if (state != null)
            {
                // NB: It's important to load game state that does not update grids first.
                // Does not update grids -----------------
                _paths.LoadGameState(state.PathGrids);
                _roads.LoadGameState(state.RoadVertices);

                // Does update grides --------------------
                _lots.LoadGameState(state.ParkingLots);
                _buildings.LoadGameState(state.Buildings);

                UpdateGrids(Enumerable.Empty<Point2>(), updateConnections: true);
            }
        }

        /// <summary>
        /// Load the campus game data.
        /// </summary>
        /// <param name="gameData">Campus game data.</param>
        protected override void LoadData(CampusData gameData)
        {
            _config = gameData;

            _terrain = CampusFactory.GenerateTerrain(transform, gameData);
            _buildings = new CampusBuildings(gameData, Accessor);
            _paths = new CampusPaths(gameData, Accessor);
            _roads = new CampusRoads(gameData, Accessor);
            _lots = new CampusParkingLots(gameData, Accessor);
            _connections = new CampusConnections(gameData, Accessor);

            _defaultMaterialIndex = gameData.Terrain.SubmaterialEmptyGrassIndex;

            var footprintCreatorObject = new GameObject("FootprintCreator");
            using (var footprintCreator = footprintCreatorObject.AddComponent<FootprintCreator>())
            {
                // Load the buildings
                foreach (var buildingData in gameData.Buildings)
                {
                    if (buildingData.BuildingEntries == null)
                    {
                        GameLogger.Info("Building {0} has no entries.", buildingData.Name);
                        buildingData.BuildingEntries = Array.Empty<BuildingEntry>();
                    }

                    buildingData.Footprint = footprintCreator.CalculateFootprint(buildingData.Model, Constant.GridSize);
                    _buildingRegistry[buildingData.Name] = buildingData;

                    GameLogger.Info("Loaded building {0}. Footprint size = {1}x{2}.", buildingData.Name, buildingData.Footprint.GetLength(0), buildingData.Footprint.GetLength(1));

                    foreach (BuildingEntry entry in buildingData.BuildingEntries)
                    {
                        if (entry.X < 0 || entry.X > buildingData.Footprint.GetLength(0) ||
                            entry.Y < 0 || entry.Y > buildingData.Footprint.GetLength(1) ||
                            !buildingData.Footprint[entry.X, entry.Y])
                        {
                            GameLogger.Warning("[ASSERT] Invalid building entry. Building = {0}; Entry = ({1}, {2})", buildingData.Name, entry.X, entry.Y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Link the campus game data.
        /// </summary>
        /// <param name="gameData">Campus game data.</param>
        protected override void LinkData(CampusData gameData)
        {
            // The link step runs after all intial data has been loaded.
            // The perfect time to load the saved game data.
            CampusSaveState savedGame = gameData.SavedData?.Campus;
            LoadGameState(savedGame);
        }

        /// <summary>
        /// Recalculate the state of a campus grid.
        /// Usually called directly after any edit operation.
        /// </summary>
        /// <param name="updatedPoints">The points that have been updated.</param>
        /// <param name="updateConnections">
        /// Whether or not to recalculate the terrain connections. Avoid over-computing. Only set to true on the last call.
        /// </param>
        private void UpdateGrids(IEnumerable<Point2> updatedPoints, bool updateConnections)
        {
            foreach (Point2 updatedPoint in updatedPoints)
            {
                UpdateGridMaterial(updatedPoint);
                UpdateGridAnchoring(updatedPoint);
            }

            if (updateConnections)
            {
                _connections.Recompute();
                if (Accessor.Game.VisualizeConnections)
                {
                    UpdateDebugConnectionsOnTerrain(enable: true);
                }
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

                case CampusGridUse.ParkingLot:
                    (materialIndex,
                     materialRotation,
                     materialInversion) = _lots.GetParkingLotMaterial(pos);
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
                case CampusGridUse.Path:
                case CampusGridUse.Road:
                case CampusGridUse.ParkingLot:
                case CampusGridUse.Building:
                case CampusGridUse.Crosswalk:
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

        /// <summary>
        /// Super hacky debug output to verify that connection path-finding is working.
        /// </summary>
        private List<GameObject> _markerSpheres = new List<GameObject>();
        private void UpdateDebugConnectionsOnTerrain(bool enable = false)
        {
            foreach (GameObject oldMarkerSphere in _markerSpheres)
            {
                Destroy(oldMarkerSphere);
            }

            if (enable)
            {
                GameObject template = ResourceLoader.Load<GameObject>(ResourceType.Prefabs, ResourceCategory.Terrain, "MarkerSphere");
                foreach (ParkingInfo parkingLotInfo in _lots.GetParkingLots())
                {
                    var rConnections = _connections.GetConnections(parkingLotInfo);
                    foreach (RoadConnection rConnection in rConnections)
                    {
                        foreach (Point2 pointAlongPath in rConnection.VertexConnection)
                        {
                            GameObject markerSphere = Instantiate(template, _terrain.transform);
                            var yHeight = _terrain.GetVertexHeight(pointAlongPath.x, pointAlongPath.z);
                            var pointOnTerrain = new Point3(pointAlongPath.x, yHeight, pointAlongPath.z);
                            markerSphere.transform.localPosition = _terrain.Convert.GridToWorld(pointOnTerrain) + new Vector3(0, 0.25f, 0);
                            _markerSpheres.Add(markerSphere);
                        }
                    }
                }

                foreach (BuildingInfo buildingInfo in _buildings.GetBuildings())
                {
                    var pCconnections = _connections.GetConnections(buildingInfo);
                    foreach (PathConnection pConnection in pCconnections)
                    {
                        foreach (Point2 pointAlongPath in pConnection.VertexConnection)
                        {
                            GameObject markerSphere = Instantiate(template, _terrain.transform);
                            var yHeight = _terrain.GetSquareHeight(pointAlongPath.x, pointAlongPath.z);
                            var pointOnTerrain = new Point3(pointAlongPath.x, yHeight, pointAlongPath.z);
                            markerSphere.transform.localPosition = _terrain.Convert.GridCenterToWorld(pointOnTerrain) + new Vector3(0, 0.25f, 0);
                            _markerSpheres.Add(markerSphere);
                        }
                    }
                }
            }
        }
    }
}
