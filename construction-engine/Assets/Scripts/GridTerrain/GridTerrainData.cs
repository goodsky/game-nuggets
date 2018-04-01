using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace GridTerrain
{
    /// <summary>
    /// Arguments for a GridTerrain
    /// </summary>
    public class GridTerrainArgs
    {
        /// <summary>
        /// World Unit size of each grid square.
        /// </summary>
        public float GridSize { get; set; } // = 10.0f;

        /// <summary>
        /// World Unit size of each grid height step.
        /// </summary>
        public float GridHeightSize { get; set; } // = 4.0f;

        /// <summary>
        /// Count of height steps that exist below the start position.
        /// </summary>
        public int UndergroundGridCount { get; set; } // = 4;

        /// <summary>
        /// Create the configuration with default values.
        /// </summary>
        public GridTerrainArgs()
        {
            GridSize = 10.0f;
            GridHeightSize = 4.0f;
            UndergroundGridCount = 4;
        }

        /// <summary>
        /// Validates that a terrain works with the desired configuration.
        /// </summary>
        /// <param name="terrain">Terrain to validate against the configuration.</param>
        internal void ValidateTerrain(Terrain terrain)
        {
            if (terrain == null)
                throw new ArgumentNullException("terrain");

            var terrainData = terrain.terrainData;

            var scale = terrainData.heightmapScale;
            Assert.AreApproximatelyEqual(scale.x, GridSize, string.Format("The X ratio of the terrain does not map to a {0}x{0} grid. Actual Scale: {1}", GridSize, scale.x));
            Assert.AreApproximatelyEqual(scale.z, GridSize, string.Format("The Z ratio of the terrain does not map to a {0}x{0} grid. Actual Scale: {1}", GridSize, scale.z));

            // This restriction exists to make the floating point math better in heightmaps
            float height = terrainData.size.y;
            Assert.IsTrue(height == 16.0f || height == 32.0f || height == 64.0f || height == 128.0f || height == 256.0f, "Terrains with heights of 16,32, 64, 128 and 256 only are supported.");

            Assert.AreApproximatelyEqual(height % GridHeightSize, 0.0f, string.Format("The terrain height must divide exactly into requested terrain height step. Height: {0} Step: {1}", height, GridHeightSize));
            Assert.IsTrue(height / GridHeightSize < 101.0f, string.Format("The terrain is too tall for the requested step size. Please have 100 or fewer steps. Step Count: {0}", height / GridHeightSize));

            Assert.IsTrue(UndergroundGridCount <= Mathf.RoundToInt(height / GridHeightSize), string.Format("Too many underground steps. Requested: {0} Total Steps: {1}", UndergroundGridCount, Mathf.RoundToInt(height / GridHeightSize)));
        }
    }

    /// <summary>
    /// Unit's TerrainData stores heightmap data in floats normalized from 0.0 - 1.0.
    /// This is cool and all, but float math sucks. 
    /// This class takes heightmaps and maps them to integer grid points,
    /// ... so we can use integers and be beautiful.
    /// </summary>
    public class GridTerrainData
    {
        // This is trying to counteract floating point errors. No guarantees though.
        private const float ep = 0.001f;

        // World Unit size of each grid square.
        private readonly float GridSize;
        private readonly float HalfGridSize;

        // World Unit size of each grid height step.
        private readonly float GridHeightSize;

        // Count of height steps that exist below the start position.
        private readonly int UndergroundGridCount;

        // Heightmap Unit size of each heightmap step
        private readonly float HeightmapStepSize;

        // Size of the grid in each direction
        public readonly int GridCountX;
        public readonly int GridCountY;
        public readonly int GridCountZ;

        // World Unit minimum for the terrain X-axis.
        private readonly float MinTerrainX;

        // World Unit minimum for the terrain Z-axis.
        private readonly float MinTerrainZ;

        // World Unit minimum for the terrain height.
        private readonly float MinTerrainHeight;

        // Unit's terrain data
        private TerrainData _terrainData;

        // My grid data mapped to integers
        private int[,] _gridData;

        /// <summary>
        /// Create a grid-backed terrain object.
        /// </summary>
        /// <param name="terrain">Terrain to wrap.</param>
        /// <param name="args">Grid configuration.</param>
        public GridTerrainData(Terrain terrain, GridTerrainArgs args)
        {
            args.ValidateTerrain(terrain);

            _terrainData = terrain.terrainData;
            _gridData = new int[_terrainData.heightmapWidth, _terrainData.heightmapHeight];

            GridSize = args.GridSize;
            HalfGridSize = GridSize / 2.0f;
            GridHeightSize = args.GridHeightSize;
            UndergroundGridCount = args.UndergroundGridCount;

            GridCountX = Mathf.RoundToInt(_terrainData.size.x / GridSize);
            GridCountY = Mathf.RoundToInt(_terrainData.size.y / GridHeightSize);
            GridCountZ = Mathf.RoundToInt(_terrainData.size.z / GridSize);

            HeightmapStepSize = 1.0f / GridCountY;

            // Minimum values are needed for conversion base-lines
            var parent = terrain.gameObject;
            var position = parent.transform.position;
            MinTerrainX = position.x;
            MinTerrainZ = position.z;
            MinTerrainHeight = position.y - (UndergroundGridCount * GridHeightSize);

            // Adjust the actual y-position to match underground grid count 
            parent.transform.position = new Vector3(position.x, MinTerrainHeight, position.z);
            Flatten(UndergroundGridCount); // this initializes the _gridData

            if (Debug.isDebugBuild)
            {
                string summaryLog = string.Empty;
                summaryLog += "GridTerrain Initialized.\n";
                summaryLog += string.Format("Voxel Size {0}x{1}x{0}\n", GridSize, GridHeightSize);
                summaryLog += string.Format("Grid Size {0}x{1}x{2}\n", GridCountX, GridCountY, GridCountZ);
                summaryLog += string.Format("Origin Position {0}, {1}, {2}\n", MinTerrainX, MinTerrainHeight, MinTerrainZ);
                summaryLog += string.Format("Heightmap Step: {0}\n", HeightmapStepSize);
                summaryLog += string.Format("\n");

                Debug.Log(summaryLog);
            }
        }

        /// <summary>
        /// Gets the world height of a square.
        /// NOTE: This averages the 4 points to get the 'center'. It isn't totally accurate.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The world y position of the grid square</returns>
        public float GetWorldHeight(int x, int z)
        {
            var selectedHeights = _terrainData.GetHeights(x, z, 2, 2);

            // average of 4 points in the square snapped to a grid height
            // note: added a small number to  avoid inprecision caused by truncation
            return ConvertHeightmapToWorld((selectedHeights[0, 0] + selectedHeights[0, 1] + selectedHeights[1, 0] + selectedHeights[1, 1]) / 4.0f + ep);
        }

        /// <summary>
        /// Gets the grid height of a point.
        /// </summary>
        /// <param name="x">Point x position</param>
        /// <param name="z">Point z position</param>
        /// <returns>The grid y position of the grid square</returns>
        public int GetPointHeight(int x, int z)
        {
            return _gridData[x, z];
        }

        /// <summary>
        /// Set a square to a grid height.
        /// Note: this is an unsafe set!
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid z position</param>
        /// <param name="gridHeight">Grid y height</param>
        public void SetHeight(int x, int z, int gridHeight)
        {
            if (x < 0 || x > GridCountX || z < 0 || z > GridCountZ)
                throw new ArgumentOutOfRangeException(string.Format("Attempted to set a square height outside of range! ({0},{1}) is outside of ({2},{3})", x, z, GridCountX, GridCountZ));

            var heights = new float[2, 2];
            heights[0, 0] = heights[0, 1] = heights[1, 0] = heights[1, 1] = ConvertGridHeightToHeightmap(gridHeight);
            _gridData[x, z] = _gridData[x, z + 1] = _gridData[x + 1, z] = _gridData[x + 1, z + 1] = gridHeight;
            _terrainData.SetHeights(x, z, heights);
        }

        /// <summary>
        /// Set an area of points to a grid height
        /// </summary>
        /// <param name="xBase">Starting x point</param>
        /// <param name="zBase">Starting z point</param>
        /// <param name="heights">Grid heights to set</param>
        public void SetPointHeights(int xBase, int zBase, int[,] heights)
        {
            var xLength = heights.GetLength(0);
            var zLength = heights.GetLength(1);

            // NB: unity flips the X/Z axis for their SetHeights
            var heightmapHeights = new float[zLength, xLength];
            for (int i = 0; i < xLength; ++i)
            {
                for (int j = 0; j < zLength; ++j)
                {
                    _gridData[xBase + i, zBase + j] = heights[i, j];
                    heightmapHeights[j, i] = ConvertGridHeightToHeightmap(heights[i, j]);
                }
            }

            _terrainData.SetHeights(xBase, zBase, heightmapHeights);
        }

        /// <summary>
        /// Flatten the entire terrain to a certain height
        /// </summary>
        /// <param name="gridHeight">Grid y height</param>
        public void Flatten(int gridHeight = 0)
        {
            var width = _terrainData.heightmapWidth;
            var height = _terrainData.heightmapHeight;

            var resetHeights = new int[width, height];
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                    resetHeights[i, j] = gridHeight;

            SetPointHeights(0, 0, resetHeights);
        }

        #region Conversions
        public Point3 ConvertWorldToGrid(Vector3 world)
        {
            return new Point3(
                Mathf.FloorToInt((world.x - MinTerrainX) / GridSize + ep),
                Mathf.FloorToInt((world.y - MinTerrainHeight) / GridHeightSize + ep),
                Mathf.FloorToInt((world.z - MinTerrainZ) / GridSize + ep));
        }

        public Vector3 ConvertGridToWorld(Point3 grid)
        {
            return new Vector3(
                grid.x * GridSize + MinTerrainX,
                grid.y * GridHeightSize + MinTerrainHeight,
                grid.z * GridSize + MinTerrainZ);
        }

        public Vector3 ConvertGridCenterToWorld(Point3 grid)
        {
            return new Vector3(
                grid.x * GridSize + MinTerrainX + HalfGridSize,
                grid.y * GridHeightSize + MinTerrainHeight,
                grid.z * GridSize + MinTerrainZ + HalfGridSize);
        }

        public float ConvertGridHeightToWorld(int grid)
        {
            return grid * GridHeightSize + MinTerrainHeight;
        }

        public int ConvertHeightmapToGridHeight(float heightmap)
        {
            // this round assumes heightmaps are all close to snapped values
            return Mathf.RoundToInt(heightmap / HeightmapStepSize);
        }

        public float ConvertHeightmapToWorld(float heightmap)
        {
            return ConvertGridHeightToWorld(ConvertHeightmapToGridHeight(heightmap));
        }

        public float ConvertGridHeightToHeightmap(int gridHeight)
        {
            return gridHeight * HeightmapStepSize;
        }
        #endregion
    }
}
