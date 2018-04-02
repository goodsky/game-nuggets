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
        public float GridSize { get; set; } // = 1.0f;

        /// <summary>
        /// World Unit size of each grid height step.
        /// </summary>
        public float GridHeightSize { get; set; } // = 0.5f;

        /// <summary>
        /// Count of height steps that exist below the start position.
        /// </summary>
        public int UndergroundGridCount { get; set; } // = 4;

        /// <summary>
        /// Create the configuration with default values.
        /// </summary>
        public GridTerrainArgs()
        {
            GridSize = 1.0f;
            GridHeightSize = 0.5f;
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

            float gridXCount = terrainData.size.x / GridSize;
            float gridZCount = terrainData.size.z / GridSize;
            Assert.AreApproximatelyEqual(terrainData.heightmapHeight, gridXCount * 2 + 1, string.Format("The heightmap should be 2*N+1 the terrain grid count (X). Count of grid squares {0}", gridXCount));
            Assert.AreApproximatelyEqual(terrainData.heightmapWidth, gridZCount * 2 + 1, string.Format("The heightmap should be 2*N+1 the terrain grid count (Z). Count of grid squares {0}", gridZCount));

            var scale = terrainData.heightmapScale; // we double the scale since we have 4 terrain squares per single grid square.
            Assert.AreApproximatelyEqual(scale.x * 2, GridSize, string.Format("The X ratio of the terrain does not map to a {0}x{0} grid. Actual Scale: {1}", GridSize, scale.x * 2));
            Assert.AreApproximatelyEqual(scale.z * 2, GridSize, string.Format("The Z ratio of the terrain does not map to a {0}x{0} grid. Actual Scale: {1}", GridSize, scale.z * 2));

            float height = terrainData.size.y;
            Assert.AreApproximatelyEqual(height % GridHeightSize, 0.0f, string.Format("The terrain height must divide exactly into requested terrain height step. Height: {0} Step: {1}", height, GridHeightSize));
            Assert.IsTrue(height / GridHeightSize < 101.0f, string.Format("The terrain is too tall for the requested step size. Please have 100 or fewer steps. Step Count: {0}", height / GridHeightSize));

            Assert.IsTrue(UndergroundGridCount <= Mathf.RoundToInt(height / GridHeightSize), string.Format("Too many underground steps. Requested: {0} Total Steps: {1}", UndergroundGridCount, Mathf.RoundToInt(height / GridHeightSize)));
        }
    }
}
