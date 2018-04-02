using Common;
using System;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Unit's TerrainData stores heightmap data in floats normalized from 0.0 - 1.0.
    /// This class takes heightmaps and maps them to integer grid points,
    /// 
    /// GridTerrain 2.0:
    /// Double the resolution of the underlying TerrainData. This hides some rendering weirdness.
    /// This means that the gridData stored here is not 1:1 with the terrainData. Keep that in mind.
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

            GridSize = args.GridSize;
            HalfGridSize = GridSize / 2.0f;
            GridHeightSize = args.GridHeightSize;
            UndergroundGridCount = args.UndergroundGridCount;

            GridCountX = Mathf.RoundToInt(_terrainData.size.x / GridSize);
            GridCountY = Mathf.RoundToInt(_terrainData.size.y / GridHeightSize);
            GridCountZ = Mathf.RoundToInt(_terrainData.size.z / GridSize);

            _gridData = new int[GridCountX + 1, GridCountZ + 1];

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

                GameLogger.Info(summaryLog);
            }
        }

        /// <summary>
        /// Gets the world height of a square.
        /// NOTE: This averages the 4 control points to get the 'center'. It isn't totally accurate.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The world y position of the grid square</returns>
        public float GetWorldHeight(int x, int z)
        {
            var selectedHeights = _terrainData.GetHeights(x * 2, z * 2, 3, 3);

            // average of 4 points in the square snapped to a grid height
            // note: added a small number to  avoid inprecision caused by truncation
            return ConvertHeightmapToWorld((selectedHeights[0, 0] + selectedHeights[0, 2] + selectedHeights[2, 0] + selectedHeights[2, 2]) / 4.0f + ep);
        }

        /// <summary>
        /// Gets the grid height of a control point.
        /// </summary>
        /// <param name="x">Point x position</param>
        /// <param name="z">Point z position</param>
        /// <returns>The grid y position of the grid square</returns>
        public int GetPointHeight(int x, int z)
        {
            return _gridData[x, z];
        }

        /// <summary>
        /// Set a grid square to a grid height.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid z position</param>
        /// <param name="gridHeight">Grid y height</param>
        public void SetHeight(int x, int z, int gridHeight)
        {
            if (x < 0 || x >= GridCountX || z < 0 || z >= GridCountZ)
                throw new ArgumentOutOfRangeException(string.Format("Attempted to set a square height outside of range! ({0},{1}) is outside of ({2},{3})", x, z, GridCountX, GridCountZ));

            SetPointHeights(x, z, new int[,] { { gridHeight, gridHeight }, { gridHeight, gridHeight } });
        }

        /// <summary>
        /// Set an area of points to a grid height.
        /// </summary>
        /// <param name="xBase">Starting x point</param>
        /// <param name="zBase">Starting z point</param>
        /// <param name="heights">Grid heights to set</param>
        public void SetPointHeights(int xBase, int zBase, int[,] heights)
        {
            int xLength = heights.GetLength(0);
            int zLength = heights.GetLength(1);

            if (xBase < 0 || xBase + xLength > GridCountX + 1 || zBase < 0 || zBase + zLength > GridCountZ + 1)
                throw new ArgumentOutOfRangeException(string.Format("Attempted to set points height outside of range! ({0},{1}) + ({2},{3}) is outside of ({4},{5})", xBase, zBase, xLength, zLength, GridCountX + 1, GridCountZ + 1));

            int heightmapXBase; // read the comments on CreateDoubleResolutionArray to try to understand this headache.
            int heightmapXOriginAdjust; // no seriously. I had to take a break and play guitar while writing this.
            int heightmapXSizeAdjust;
            int heightmapZBase;
            int heightmapZOriginAdjust;
            int heightmapZSizeAdjust;
            var heightmapHeights = CreateDoubleResolutionArray(xBase, zBase, xLength, zLength, out heightmapXBase, out heightmapXOriginAdjust, out heightmapXSizeAdjust, out heightmapZBase, out heightmapZOriginAdjust, out heightmapZSizeAdjust);

            int heightmapZLength = heightmapHeights.GetLength(0);
            int heightmapXLength = heightmapHeights.GetLength(1);

            // 1st pass: set the control nodes
            for (int x = 0; x < xLength; ++x)
            {
                for (int z = 0; z < zLength; ++z)
                {
                    _gridData[xBase + x, zBase + z] = heights[x, z];
                    heightmapHeights[z * 2 + heightmapZOriginAdjust, x * 2 + heightmapXOriginAdjust] = ConvertGridHeightToHeightmap(heights[x, z]);
                }
            }

            // 2nd pass: set the x-axis edges
            for (int x = 0; x < xLength + heightmapXOriginAdjust + heightmapXSizeAdjust; ++x)
            {
                for (int z = 0; z < zLength; ++z)
                {
                    int heightmapX = x * 2 + (1 - heightmapXOriginAdjust);
                    int heightmapZ = z * 2 + heightmapZOriginAdjust;

                    int leftX = heightmapX - 1;
                    int rightX = heightmapX + 1;

                    // random reminder: the heightmap array is in (Z, X) while the GetHeight call is (X, Z).
                    //                  you know, exactly the way you would expect.
                    float leftHeight = leftX < 0 ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + leftX, heightmapZBase + heightmapZ)) : 
                        heightmapHeights[heightmapZ, leftX];

                    float rightHeight = rightX >= heightmapXLength ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + rightX, heightmapZBase + heightmapZ)) :
                        heightmapHeights[heightmapZ, rightX];

                    heightmapHeights[heightmapZ, heightmapX] = (leftHeight + rightHeight) / 2;
                }
            }

            // 3rd pass: set the z-axis edges
            for (int x = 0; x < xLength; ++x)
            {
                for (int z = 0; z < zLength + heightmapZOriginAdjust + heightmapZSizeAdjust; ++z)
                {
                    int heightmapX = x * 2 + heightmapXOriginAdjust;
                    int heightmapZ = z * 2 + (1 - heightmapZOriginAdjust);

                    int upZ = heightmapZ - 1;
                    int downZ = heightmapZ + 1;

                    float upHeight = upZ < 0 ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + heightmapX, heightmapZBase + upZ)) :
                        heightmapHeights[upZ, heightmapX];

                    float downHeight = downZ >= heightmapZLength ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + heightmapX, heightmapZBase + downZ)) :
                        heightmapHeights[downZ, heightmapX];

                    heightmapHeights[heightmapZ, heightmapX] = (upHeight + downHeight) / 2;
                }
            }

            // 4th pass: set the grid centers
            for (int x = 0; x < xLength + heightmapXOriginAdjust + heightmapXSizeAdjust; ++x)
            {
                for (int z = 0; z < zLength + heightmapZOriginAdjust + heightmapZSizeAdjust; ++z)
                {
                    int heightmapX = x * 2 + (1 - heightmapXOriginAdjust);
                    int heightmapZ = z * 2 + (1 - heightmapZOriginAdjust);

                    int leftX = heightmapX - 1;
                    int rightX = heightmapX + 1;
                    int upZ = heightmapZ - 1;
                    int downZ = heightmapZ + 1;

                    float topLeftHeight = upZ < 0 || leftX < 0 ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + leftX, heightmapZBase + upZ)) :
                        heightmapHeights[upZ, leftX];

                    float topRightHeight = upZ < 0 || rightX >= heightmapXLength ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + rightX, heightmapZBase + upZ)) :
                        heightmapHeights[upZ, rightX];

                    float bottomLeftHeight = downZ >= heightmapZLength || leftX < 0 ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + leftX, heightmapZBase + downZ)) :
                        heightmapHeights[downZ, leftX];

                    float bottomRightHeight = downZ >= heightmapZLength || rightX >= heightmapXLength ?
                        ConvertTerrainWorldToHeightmap(_terrainData.GetHeight(heightmapXBase + rightX, heightmapZBase + downZ)) :
                        heightmapHeights[downZ, rightX];

                    heightmapHeights[heightmapZ, heightmapX] = GetMajorityOrAverage(topLeftHeight, topRightHeight, bottomLeftHeight, bottomRightHeight);
                }
            }

            _terrainData.SetHeights(heightmapXBase, heightmapZBase, heightmapHeights);
        }

        /// <summary>
        /// Flatten the entire terrain to a certain height
        /// </summary>
        /// <param name="gridHeight">Grid y height</param>
        public void Flatten(int gridHeight = 0)
        {
            var resetHeights = new int[GridCountX + 1, GridCountZ + 1];
            for (int i = 0; i < GridCountX + 1; ++i)
                for (int j = 0; j < GridCountZ + 1; ++j)
                    resetHeights[i, j] = gridHeight;

            SetPointHeights(0, 0, resetHeights);
        }

        /// <summary>
        /// This adjustment is because an extra ring of points are needed to smooth out 
        /// higher resolution heightmaps. But if this ring is outside the terrain then we 
        /// don't need to adjust the resulting array to include it.
        /// </summary>
        /// <param name="xBase">The x origin of the grid resolution array.</param>
        /// <param name="zBase">The z origin of the grid resolution array.</param>
        /// <param name="xLength">The x length of the grid resolution array.</param>
        /// <param name="zLength">The z length of the grid resolutio narray.</param>
        /// <param name="heightmapXBase">The x origin in the high resolution array.</param>
        /// <param name="heightmapXOriginAdjust">The x origin adjustment. (do we start at 0 or 1?)</param>
        /// <param name="heightmapXSizeAdjust">The x size adjustment. (is the size of the array one less?)</param>
        /// <param name="heightmapZBase">The z origin in the high resolution array.</param>
        /// <param name="heightmapZOriginAdjust">The z  origin adjustment. (do we start at 0 or 1?)</param>
        /// <param name="heightmapZSizeAdjust">The z size adjustment. (is the size of the array one less?)</param>
        /// <returns>The higher resolution array.</returns>
        private float[,] CreateDoubleResolutionArray(
            int xBase, 
            int zBase, 
            int xLength, 
            int zLength, 
            out int heightmapXBase,
            out int heightmapXOriginAdjust,
            out int heightmapXSizeAdjust,
            out int heightmapZBase,
            out int heightmapZOriginAdjust,
            out int heightmapZSizeAdjust)
        {
            heightmapXBase = xBase * 2 - 1;
            heightmapZBase = zBase * 2 - 1;

            heightmapXOriginAdjust = 1;
            if (heightmapXBase < 0)
            {
                heightmapXBase = 0;
                heightmapXOriginAdjust = 0;
            }

            heightmapZOriginAdjust = 1;
            if (heightmapZBase < 0)
            {
                heightmapZBase = 0;
                heightmapZOriginAdjust = 0;
            }

            heightmapXSizeAdjust = 0;
            if (heightmapXBase + xLength * 2 + 1 > GridCountX * 2)
            {
                heightmapXSizeAdjust = -1;
            }

            heightmapZSizeAdjust = 0;
            if (heightmapZBase + zLength * 2 + 1 > GridCountZ * 2)
            {
                heightmapZSizeAdjust = -1;
            }

            // NB: unity flips the X/Z axis for their SetHeights
            return new float[zLength * 2 + heightmapZOriginAdjust + heightmapZSizeAdjust, xLength * 2 + heightmapXOriginAdjust + heightmapXSizeAdjust];
        }

        /// <summary>
        /// Return either the majority or the average (if there is a tie).
        /// I think this is optimizing this algorithm since we only have 4 floats.
        /// Maybe I'm just being lazy and don't want to use a sort.
        /// </summary>
        /// <param name="f1">1st float</param>
        /// <param name="f2">2nd float</param>
        /// <param name="f3">3rd float</param>
        /// <param name="f4">4th float</param>
        /// <returns>The mode or the average (if there is not a above 50% average)</returns>
        private float GetMajorityOrAverage(float f1, float f2, float f3, float f4)
        {
            // The logic here is if we find two who are equal then we need one more to make it a mode.
            // However, if there is no other, then we are best case in a tie. So return the average.
            if (Mathf.Approximately(f1, f2))
            {
                if (Mathf.Approximately(f1, f3) || Mathf.Approximately(f1, f4))
                {
                    return f1;
                }
            }
            else if (Mathf.Approximately(f1, f3))
            {
                if (Mathf.Approximately(f1, f4))
                {
                    return f1;
                }
            }
            else if (Mathf.Approximately(f2, f3))
            {
                if (Mathf.Approximately(f2, f4))
                {
                    return f2;
                }
            }

            return (f1 + f2 + f3 + f4) / 4;
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

        public int ConvertTerrainWorldToGridHeight(float world)
        {
            // WARNING WARNING WARNING this convert does not remove the min height
            //                         because apparently the terrain doesn't return actual world height.
            // this round assumes world height is close to snapped values
            return Mathf.RoundToInt(world / GridHeightSize + ep);
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

        public float ConvertTerrainWorldToHeightmap(float world)
        {
            return ConvertGridHeightToHeightmap(ConvertTerrainWorldToGridHeight(world));
        }

        public float ConvertGridHeightToHeightmap(int gridHeight)
        {
            return gridHeight * HeightmapStepSize;
        }
        #endregion
    }
}
