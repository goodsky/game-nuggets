using Common;
using System;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// A Custom terrain mesh based using a runtime generated plane mesh instead of Unity's heavy Terrain component.
    /// Splits squares using submeshes.
    /// 
    /// 'GridTerrain v3.2'
    /// </summary>
    public class GridTerrainData4 : MonoBehaviour, IGridTerrain
    {
        // This is trying to counteract floating point errors. No guarantees though.
        private const float ep = 0.001f;

        /// <summary>Size of the grid squares.</summary>
        public float GridSize = 1.0f;

        /// <summary>Size of a height change.</summary>
        public float GridStepSize = 0.5f;

        /// <summary>Number of grid squares along the X axis.</summary>
        public int GridXCount = 64;

        /// <summary>Number of grid squares along the Z axis.</summary>
        public int GridZCount = 64;

        /// <summary>Max number of grid steps you can go up.</summary>
        public int GridStepsUp = 8;

        /// <summary>Max number of grid steps you can go down.</summary>
        public int GridStepsDown = 4;

        /// <summary>Whether or not this terrain is runtime editable.</summary>
        public bool Editable = true;

        /// <summary>Whether or not to surround the terrain with a skirt.</summary>
        public bool Skirt = false;

        // World Unit minimum for the terrain height.
        private float MinTerrainHeight;

        /// <summary>Size of a grid square</summary>
        public float Size { get { return GridSize; } }

        /// <summary>Number of grid squares along the x-axis.</summary>
        public int CountX { get { return GridXCount; } }

        /// <summary>Number of grid squares along the y-axis.</summary>
        public int CountY { get { return GridStepsUp + GridStepsDown + 1; } }

        /// <summary>Number of grid squares along the z-axis.</summary>
        public int CountZ { get { return GridZCount; } }

        /// <summary>Half the grid step size. Used in conversions.</summary>
        private float HalfGridSize;

        // World Unit minimum for the terrain X-axis.
        private float MinTerrainX;

        // World Unit minimum for the terrain Z-axis.
        private float MinTerrainZ;

        // The custom mesh used to shape the terrain.
        private GridMesh _mesh;

        /// <summary>
        /// Unity's Start
        /// </summary>
        protected void Start()
        {
            HalfGridSize = GridSize / 2;
            MinTerrainX = transform.position.x;
            MinTerrainZ = transform.position.z;
            MinTerrainHeight = GridStepsDown * -GridStepSize;
            
            var filter = gameObject.AddComponent<MeshFilter>();
            var collider = gameObject.AddComponent<MeshCollider>();
            var renderer = gameObject.AddComponent<MeshRenderer>();

            filter.mesh = new Mesh();
            filter.mesh.name = "grid-terrain";

            _mesh = new GridMesh(filter.mesh, collider, renderer);
            _mesh.Initialize(GridSize, CountX, CountZ, GridMaterials.GetAll());

            if (Skirt)
            {
                var skirtPrefab = Resources.Load<GameObject>("terrain_skirt");
                var skirtObject = Instantiate(skirtPrefab);

                // the skirt prefab is rotated 90 degrees, so scale y-axis instead of z
                skirtObject.transform.localScale = new Vector3(CountX * GridSize, CountZ * GridSize, 1.0f);
            }

            if (Editable)
            {
                gameObject.AddComponent<EditableTerrain>();
            }

            Flatten(GridStepsDown);
        }

        /// <summary>
        /// Unity's Destroy
        /// </summary>
        protected void OnDestroy()
        {
            if (_mesh != null)
            {
                _mesh.Dispose();
            }
        }

        /// <summary>
        /// Gets the grid height of a square.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The grid y position of the grid square</returns>
        public int GetSquareHeight(int x, int z)
        {
            if (x < 0 || x >= GridXCount || z < 0 || z >= GridZCount)
                GameLogger.FatalError("Attempted to GetWorldHeight out of range. ({0},{1})", x, z);

            return ConvertWorldHeightToGrid(_mesh.GetHeight(x, z));
        }

        /// <summary>
        /// Get the height of the center of a square in world coordinates.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <returns>The coordinates of the center of the grid.</returns>
        public float GetSquareWorldHeight(int x, int z)
        {
            if (x < 0 || x >= GridXCount || z < 0 || z >= GridZCount)
                GameLogger.FatalError("Attempted to GetWorldHeight out of range. ({0},{1})", x, z);

            return _mesh.GetHeight(x, z);
        }

        /// <summary>
        /// Get the height of the grid point.
        /// This method looks at vertices and not grid squares.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <returns>The grid height.</returns>
        public int GetPointHeight(int x, int z)
        {
            if (x < 0 || x > GridXCount || z < 0 || z > GridZCount)
                GameLogger.FatalError("Attempted to GetPointHeight out of range. ({0},{1})", x, z);

            return ConvertWorldHeightToGrid(_mesh.GetVertexHeight(x, z));
        }

        /// <summary>
        /// Gets the world height of a vertex.
        /// </summary>
        /// <param name="x">Point x position</param>
        /// <param name="z">Point z position</param>
        /// <returns>The world y position of the vertex</returns>
        public float GetPointWorldHeight(int x, int z)
        {
            if (x < 0 || x > GridXCount || z < 0 || z > GridZCount)
                GameLogger.FatalError("Attempted to GetPointHeight out of range. ({0},{1})", x, z);

            return _mesh.GetVertexHeight(x, z);
        }

        /// <summary>
        /// Set the height of a grid coordinate.
        /// NOTE: This is an unsafe set. Use the SafeTerrainEditor instead.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <param name="gridHeight">The height to set.</param>
        public void SetHeight(int x, int z, int gridHeight)
        {
            if (x < 0 || x >= CountX || z < 0 || z >= CountZ)
                GameLogger.FatalError("Attempted to set a square height outside of range! ({0},{1}) is outside of ({2},{3})", x, z, CountX, CountZ);

            SetPointHeights(x, z, new int[,] { { gridHeight, gridHeight }, { gridHeight, gridHeight } });
        }

        /// <summary>
        /// Set the heights of several points in the grid.
        /// </summary>
        /// <param name="xBase">The starting point x position.</param>
        /// <param name="zBase">The starting point z position.</param>
        /// <param name="heights">The heights to set.</param>
        public void SetPointHeights(int xBase, int zBase, int[,] heights)
        {
            int xLength = heights.GetLength(0);
            int zLength = heights.GetLength(1);

            if (xBase < 0 || xBase + xLength > CountX + 1 || zBase < 0 || zBase + zLength > CountZ + 1)
                GameLogger.FatalError("Attempted to set points height outside of range! ({0},{1}) + ({2},{3}) is outside of ({4},{5})", xBase, zBase, xLength, zLength, CountX + 1, CountZ + 1);

            float[,] heightsf = new float[xLength, zLength];
            for (int x = 0; x < xLength; ++x)
                for (int z = 0; z < zLength; ++z)
                    heightsf[x, z] = ConvertGridHeightToWorld(heights[x, z]);

            _mesh.SetVertexHeights(xBase, zBase, heightsf);
        }

        public int GetMaterial(int x, int z)
        {
            return _mesh.GetMaterial(x, z);
        }

        public void SetMaterial(int x, int z, int materialId)
        {
            _mesh.SetMaterial(x, z, materialId);
        }

        /// <summary>
        /// Flatten the grid to a set height.
        /// </summary>
        /// <param name="gridHeight">The height to flatten to. Defaults to zero.</param>
        public void Flatten(int gridHeight = 0)
        {
            var resetHeights = new int[CountX + 1, CountZ + 1];
            for (int i = 0; i < CountX + 1; ++i)
                for (int j = 0; j < CountZ + 1; ++j)
                    resetHeights[i, j] = gridHeight;

            SetPointHeights(0, 0, resetHeights);
        }

        /// <summary>
        /// Raycast against the terrain.
        /// </summary>
        /// <param name="ray">Ray to cast along.</param>
        /// <param name="hit">Hit information.</param>
        /// <param name="maxDistance">Max distance to cast along the ray.</param>
        /// <returns>True if the collission occurred, false otherwise.</returns>
        public bool Raycast(Ray ray, out RaycastHit hit, float maxDistance)
        {
            return _mesh.Collider.Raycast(ray, out hit, maxDistance);
        }

        /// <summary>
        /// Convert a world point to grid coordinates.
        /// </summary>
        /// <param name="world">Point in Unity world space.</param>
        /// <returns>The coorisponding grid coordinate.</returns>
        public Point3 ConvertWorldToGrid(Vector3 world)
        {
            return new Point3(
                Mathf.FloorToInt((world.x - MinTerrainX) / GridSize + ep),
                Mathf.FloorToInt((world.y - MinTerrainHeight) / GridStepSize + ep),
                Mathf.FloorToInt((world.z - MinTerrainZ) / GridSize + ep));
        }

        /// <summary>
        /// Convert a grid coordinate into world point at the origin of the grid.
        /// </summary>
        /// <param name="grid">Grid point.</param>
        /// <returns>The world coordinate.</returns>
        public Vector3 ConvertGridToWorld(Point3 grid)
        {
            return new Vector3(
                grid.x * GridSize + MinTerrainX,
                grid.y * GridStepSize + MinTerrainHeight,
                grid.z * GridSize + MinTerrainZ);
        }

        /// <summary>
        /// Convert a grid coordinate into world point at the center of the grid.
        /// </summary>
        /// <param name="grid">Grid point.</param>
        /// <returns>The world coordinate.</returns>
        public Vector3 ConvertGridCenterToWorld(Point3 grid)
        {
            return new Vector3(
                grid.x * GridSize + MinTerrainX + HalfGridSize,
                grid.y * GridStepSize + MinTerrainHeight,
                grid.z * GridSize + MinTerrainZ + HalfGridSize);
        }

        /// <summary>
        /// Convert a world coordinate height into grid units.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public int ConvertWorldHeightToGrid(float world)
        {
            return Mathf.FloorToInt((world - MinTerrainHeight) / GridStepSize + ep);
        }

        /// <summary>
        /// Convert a grid height to world units.
        /// </summary>
        /// <param name="grid">The grid height step.</param>
        /// <returns>World coordinate of the grid height.</returns>
        public float ConvertGridHeightToWorld(int grid)
        {
            return grid * GridStepSize + MinTerrainHeight;
        }
    }
}
