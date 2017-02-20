using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.Framework
{
    /// <summary>
    /// Manage all buildings, paths and landscaping on your campus
    /// This is like the ScreenElement manager, but instead of rendering we calculate the university score based on the buildings and their location.
    /// 
    /// This manager also keeps a grid of where all buildings, roads etc are located on the screen. This is useful for collision checking and pathing.
    /// </summary>
    class CampusManager
    {
        /// <summary>
        /// A hash table of all buildings, hashed by their Guids
        /// </summary>
        private Dictionary<long, ScreenElement> elements = new Dictionary<long, ScreenElement>();

        /// <summary>
        /// Grid that keeps track of the Guid of buildings at grid locations
        /// </summary>
        private long[][] grid;

        /// <summary>
        /// The minimum values you can get in isometric world coordinates.
        /// They can me negative, so we subtract this value from world coordinates to normalize to non-negative values
        /// </summary>
        private int minWorldIsoX;
        private int minWorldIsoY;

        /// <summary>
        /// Singleton Building Manager
        /// </summary>
        public static CampusManager Instance
        {
            get
            {
                return CampusManager.instance;
            }
        }

        /// <summary>
        /// Private field
        /// </summary>
        private static CampusManager instance = new CampusManager();

        /// <summary>
        /// Initialize the building manager with a clean new world grid
        /// </summary>
        /// <param name="width">Width of the world</param>
        /// <param name="height">Height of the world</param>
        public void Initialize(int width, int height)
        {
            int mapSize = (int)Math.Ceiling((double)width / Constants.TILE_WIDTH + (double)height / Constants.TILE_HEIGHT);
            this.minWorldIsoX = (int)Math.Round(-((double)width / Constants.TILE_WIDTH));
            this.minWorldIsoY = 0;

            this.grid = new long[mapSize][];
            for (int i = 0; i < mapSize; ++i)
            {
                this.grid[i] = new long[mapSize];

                for (int j = 0; j < mapSize; ++j)
                {
                    this.grid[i][j] = -1;
                }
            }
        }

        /// <summary>
        /// Create a new building
        /// </summary>
        /// <param name="position">Position of the new building</param>
        /// <param name="size">Size of the new building</param>
        /// <param name="data">Data about the building</param>
        /// <returns>True if the building was able to be created.</returns>
        public bool CreateBuilding(Pair<int> position, Pair<int> size, BuildingData data)
        {
            var newBuilding = new Building(position, size, data);
            this.Add(newBuilding);

            // Place this new building's Guid into the world grid
            var buildingIsoPosition = Geometry.ToIsometricGrid(position);
            for (int y = 0; y < data.Footprint.Length; ++y)
            {
                for (int x = 0; x < data.Footprint[y].Length; ++x)
                {
                    if (data.Footprint[y][x])
                    {
                        int gridX = this.WorldIsoToGridX(buildingIsoPosition.x + x - data.FootprintIndexOffsetX);
                        int gridY = this.WorldIsoToGridY(buildingIsoPosition.y + y - data.FootprintIndexOffsetY);

                        // Sanity checks before we place the building
                        if (gridX < 0 || gridX >= this.grid.Length || gridY < 0 || gridY >= this.grid.Length)
                        {
                            Logger.Log(LogLevel.Error, "Creating a building outside grid limits", string.Format("Trying to build building {0} at position: Iso: {1},{2} World: {3}, {4}. Outside world boundry of {5}", data.Name, gridX, gridY, position.x, position.y, this.grid.Length));
                            continue;
                        }

                        if (this.grid[gridY][gridX] != -1)
                        {
                            Logger.Log(LogLevel.Error, "Creating a building on top of another building", string.Format("Trying to build building {0} at position: Iso: {1},{2} World: {3}, {4}. Building {5} already exists at location.", data.Name, gridX, gridY, position.x, position.y, this.grid[gridY][gridX]));
                        }

                        this.grid[gridY][gridX] = newBuilding.Guid;
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Create a new set of path segments
        /// </summary>
        /// <param name="segments">List of the path segments</param>
        /// <returns>True always. This will probably bother you brandon if you ever read this code.</returns>
        public bool CreatePath(List<PathSegment> segments)
        {
            // First update the grid with new positions
            foreach (var segment in segments)
            {
                // Make sure there isn't already a path here
                if (this.ElementAtWorldPosition(segment.WorldPosition.x, segment.WorldPosition.y) != null)
                {
                    continue;
                }

                var newPath = new Path(segment);
                this.Add(newPath);

                var buildingIsoPosition = Geometry.ToIsometricGrid(segment.WorldPosition);
                int gridX = this.WorldIsoToGridX(buildingIsoPosition.x);
                int gridY = this.WorldIsoToGridY(buildingIsoPosition.y);

                // Sanity checks before we place the path
                if (gridX < 0 || gridX >= this.grid.Length || gridY < 0 || gridY >= this.grid.Length)
                {
                    Logger.Log(LogLevel.Error, "Creating a path outside grid limits", string.Format("Trying to build path {0} at position: Iso: {1},{2} World: {3}, {4}. Outside world boundry of {5}", segment.data.Name, gridX, gridY, buildingIsoPosition.x, buildingIsoPosition.y, this.grid.Length));
                    continue;
                }

                if (this.grid[gridY][gridX] != -1)
                {
                    Logger.Log(LogLevel.Error, "Creating a path on top of another building", string.Format("Trying to build path {0} at position: Iso: {1},{2} World: {3}, {4}. Building {5} already exists at location.", segment.data.Name, gridX, gridY, buildingIsoPosition.x, buildingIsoPosition.y, this.grid[gridY][gridX]));
                }

                this.grid[gridY][gridX] = newPath.Guid;
            }

            return true;
        }

        /// <summary>
        /// Remove this building from the game grid
        /// </summary>
        /// <param name="building">The building to remove</param>
        /// <returns></returns>
        public bool DeleteElement(ScreenElement element)
        {
            if (!this.Exists(element))
            {
                Logger.Log(LogLevel.Warning, "Tried to remove non-existant element", string.Format("The campus manager was told to remove element {0} ({1}) from the game grid, but no such element exists.", element, element.Guid));
                return false;
            }

            this.Remove(element);

            // Remove the element from the grid
            var elementIsoPosition = Geometry.ToIsometricGrid(element.Position);

            Building building = element as Building;
            if (building != null)
            {
                // Remove this building from the grid
                for (int y = 0; y < building.Footprint.Length; ++y)
                {
                    for (int x = 0; x < building.Footprint[y].Length; ++x)
                    {
                        if (building.Footprint[y][x])
                        {
                            var indexOffsets = building.FootprintIndexOffsets;
                            int gridX = this.WorldIsoToGridX(elementIsoPosition.x + x - indexOffsets.x);
                            int gridY = this.WorldIsoToGridY(elementIsoPosition.y + y - indexOffsets.y);

                            // Sanity checks before we remove the building
                            if (this.grid[gridY][gridX] != building.Guid)
                            {
                                Logger.Log(LogLevel.Error, "Removing a building from a corrupted grid location", string.Format("The grid location {0},{1} had Guid {2} set instead of {3}.", gridX, gridY, this.grid[gridY][gridX], building.Guid));
                            }

                            this.grid[gridY][gridX] = -1;
                        }
                    }
                }
            }
            else
            {
                // Remove this element from the grid
                int gridX = this.WorldIsoToGridX(elementIsoPosition.x);
                int gridY = this.WorldIsoToGridY(elementIsoPosition.y);

                // Sanity checks before we remove the element
                if (this.grid[gridY][gridX] != element.Guid)
                {
                    Logger.Log(LogLevel.Error, "Removing an element from a corrupted grid location", string.Format("The grid location {0},{1} had Guid {2} set instead of {3}.", gridX, gridY, this.grid[gridY][gridX], element.Guid));
                }

                this.grid[gridY][gridX] = -1;
            }

            return true;
        }

        /// <summary>
        /// Check if a building exists at the given location
        /// </summary>
        /// <param name="location">Isometric grid coordinates of where we are checking</param>
        /// <returns></returns>
        public bool IsBuildingAt(Pair<int> location)
        {
            int gridX = this.WorldIsoToGridX(location.x);
            int gridY = this.WorldIsoToGridY(location.y);

            if (gridX < 0 || gridX >= this.grid.Length || gridY < 0 || gridY >= this.grid.Length)
            {
                return false;
            }

            return this.grid[gridY][gridX] != -1;
        }

        /// <summary>
        /// Checks and returns a building at location X and Y
        /// </summary>
        /// <param name="x">X coordinate in world space</param>
        /// <param name="y">Y coordinate in world space</param>
        /// <returns>The building, otherwise null if no buildings exist at the position</returns>
        public ScreenElement ElementAtWorldPosition(int x, int y)
        {
            var worldGrid = Geometry.ToIsometricGrid(new Pair<int>(x, y));
            int gridX = this.WorldIsoToGridX(worldGrid.x);
            int gridY = this.WorldIsoToGridY(worldGrid.y);

            if (gridX < 0 || gridX >= this.grid.Length || gridY < 0 || gridY >= this.grid.Length)
            {
                Logger.Log(LogLevel.Warning, "Building at World Position Check", string.Format("Checking if there is a building outside the world position. Checking {0}, {1}", x, y));
                return null;
            }

            long id = this.grid[gridY][gridX];
            if (id == -1)
            {
                return null;
            }

            return this.elements[id];
        }


        // ///////////////////////////////////////////////
        // CRUD classes
        // ///////////////////////////////////////////////

        /// <summary>
        /// Add a new Screen Element
        /// </summary>
        /// <param name="building"></param>
        public void Add(ScreenElement building)
        {
            this.elements.Add(building.Guid, building);
        }

        /// <summary>
        /// Remove an element from the sorted lists
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        public void Remove(ScreenElement building)
        {
            this.elements.Remove(building.Guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Exists(ScreenElement building)
        {
            return this.elements.ContainsKey(building.Guid);
        }

        // ///////////////////////////////////////////////
        // Helper classes
        // ///////////////////////////////////////////////

        /// <summary>
        /// Convert the world Isometric coordinates to grid coordinates.
        /// The isometric X position in the world can be negative to the west (right side of the screen).
        /// This will make sure all numbers are non-negative.
        /// </summary>
        /// <param name="isox">World X position</param>
        /// <returns>The world position turned non-negative so it fits into our grid array</returns>
        private int WorldIsoToGridX(int isox)
        {
            return isox - this.minWorldIsoX;   
        }

        /// <summary>
        /// Convert the world Isometric coordinates to grid coordinates.
        /// The isometric X position in the world can be negative to the west (right side of the screen).
        /// This will make sure all numbers are non-negative.
        /// </summary>
        /// <param name="isoy">World Y position</param>
        /// <returns>The world position turned non-negative so it fits into our grid array</returns>
        private int WorldIsoToGridY(int isoy)
        {
            return isoy - this.minWorldIsoY;
        }
    }
}
