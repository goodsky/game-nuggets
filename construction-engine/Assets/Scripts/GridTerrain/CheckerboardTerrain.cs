using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Test class for <see cref="GridTerrainData"/>.
    /// Creates a checkerboard terrain.
    /// </summary>
    public class CheckerboardTerrain : MonoBehaviour
    {
        public float GridSize = 1.0f;
        public float GridHeight = 0.5f;

        private GridTerrainData _terrain;

        void Start()
        {
            var terrainComponent = GetComponent<Terrain>();
            _terrain = new GridTerrainData(terrainComponent, new GridTerrainArgs() { GridSize = GridSize, GridHeightSize = GridHeight, UndergroundGridCount = 0 });

            for (int i = 0; i < _terrain.CountX / 2; ++i)
                for (int j = 0; j < _terrain.CountZ / 2; ++j)
                    _terrain.SetHeight(i * 2, j * 2, (i + j + 1) % 2);
        }

        /// <summary>
        /// Flatten the terrain when you are done.
        /// </summary>
        void OnApplicationQuit()
        {
            _terrain.Flatten();
        }
    }
}