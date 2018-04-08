using Common;
using System;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// A Custom terrain mesh based using a runtime generated plane mesh instead of Unity's heavy Terrain component.
    /// 
    /// 'GridTerrain v3.0'
    /// </summary>
    public class GridTerrainData2 : MonoBehaviour, IGridTerrain
    {
        // This is trying to counteract floating point errors. No guarantees though.
        private const float ep = 0.001f;

        // Offsets into the vertices array.
        private const int BottomLeft = 0;
        private const int BottomRight = 1;
        private const int TopRight = 2;
        private const int TopLeft = 3;
        private const int Center = 4;

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

        // World Unit minimum for the terrain height.
        private float MinTerrainHeight;

        /// <summary>Size of a grid square</summary>
        public float Size { get { return GridSize; } }

        public int CountX { get { return GridXCount; } }

        public int CountY { get { return GridStepsUp + GridStepsDown + 1; } }

        public int CountZ { get { return GridZCount; } }

        /// <summary>Half the grid step size. Used in conversions.</summary>
        private float HalfGridSize;

        // World Unit minimum for the terrain X-axis.
        private float MinTerrainX;

        // World Unit minimum for the terrain Z-axis.
        private float MinTerrainZ;

        private Mesh _mesh;
        private MeshCollider _meshCollider;

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private int[] _triangles;

        private int[,] _gridData;

        /// <summary>
        /// Unity's Start
        /// </summary>
        protected void Start()
        {
            _mesh = GetMesh();
            _meshCollider = GetComponent<MeshCollider>();

            if (Editable)
            {
                gameObject.AddComponent<EditableTerrain>();
                // editableTerrain.CursorPrefab = Resources.Load<GameObject>("cursor");
            }

            GenerateTerrain(_mesh);

            HalfGridSize = GridSize / 2;
            MinTerrainX = transform.position.x;
            MinTerrainZ = transform.position.z;
            MinTerrainHeight = GridStepsDown * -GridStepSize;

            _gridData = new int[GridXCount + 1, GridZCount + 1];

            Flatten(GridStepsDown);
        }

        /// <summary>
        /// Unity's Destroy
        /// </summary>
        protected void OnDestroy()
        {
            if (_mesh != null)
            {
                _mesh.Clear();
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

            int offset = GetGridOffset(x, z);
            return ConvertWorldHeightToGrid(_vertices[offset + Center].y);
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

            int offset = GetGridOffset(x, z);
            return _vertices[offset + Center].y;
        }

        /// <summary>
        /// Get the height of the grid.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <returns>The grid height.</returns>
        public int GetPointHeight(int x, int z)
        {
            if (x < 0 || x > GridXCount || z < 0 || z > GridZCount)
                GameLogger.FatalError("Attempted to GetPointHeight out of range. ({0},{1})", x, z);

            return _gridData[x, z];
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

            return ConvertGridHeightToWorld(_gridData[x, z]);
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

            // 1st pass: set the corner vertices
            for (int x = 0; x < xLength; ++x)
            {
                for (int z = 0; z < zLength; ++z)
                {
                    _gridData[xBase + x, zBase + z] = heights[x, z];

                    float newHeight = ConvertGridHeightToWorld(heights[x, z]);

                    // there are potentially 4 overlaps for each corner vertex.
                    // keep them all in sync!
                    int leftX = xBase + x - 1;
                    int rightX = xBase + x;
                    int downZ = zBase + z - 1;
                    int upZ = zBase + z;

                    if (leftX >= 0 && downZ >= 0)
                    {
                        _vertices[GetGridOffset(leftX, downZ) + TopRight].y = newHeight;
                    }

                    if (leftX >= 0 && upZ < GridZCount)
                    {
                        _vertices[GetGridOffset(leftX, upZ) + BottomRight].y = newHeight;
                    }

                    if (rightX < GridXCount && downZ >= 0)
                    {
                        _vertices[GetGridOffset(rightX, downZ) + TopLeft].y = newHeight;
                    }

                    if (rightX < GridXCount && upZ < GridZCount)
                    {
                        _vertices[GetGridOffset(rightX, upZ) + BottomLeft].y = newHeight;
                    }
                }
            }

            // 2nd pass: set the center vertices
            for (int x = -1; x < xLength + 1; ++x)
            {
                for (int z = -1; z < zLength + 1; ++z)
                {
                    int gridX = xBase + x;
                    int gridZ = zBase + z;

                    if (gridX >= 0 && gridX < GridXCount && gridZ >= 0 && gridZ < GridZCount)
                    {
                        int gridOffset = GetGridOffset(gridX, gridZ);
                        float newHeight = Utils.GetMajorityOrAverage(
                            _vertices[gridOffset + BottomLeft].y,
                            _vertices[gridOffset + BottomRight].y,
                            _vertices[gridOffset + TopRight].y,
                            _vertices[gridOffset + TopLeft].y);

                        _vertices[gridOffset + Center].y = newHeight;
                    }
                }
            }

            UpdateMesh();
        }

        public int GetMaterial(int x, int z)
        {
            throw new NotImplementedException();
        }

        public void SetMaterial(int x, int z, int materialId)
        {
            throw new NotImplementedException();
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
            return _meshCollider.Raycast(ray, out hit, maxDistance);
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
        /// Convert a world coordinate height into grid units.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public int ConvertWorldHeightToGrid(float world)
        {
            return Mathf.FloorToInt((world - MinTerrainHeight) / GridStepSize + ep);
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
        /// Convert a grid height to world units.
        /// </summary>
        /// <param name="grid">The grid height step.</param>
        /// <returns>World coordinate of the grid height.</returns>
        public float ConvertGridHeightToWorld(int grid)
        {
            return grid * GridStepSize + MinTerrainHeight;
        }

        /// <summary>
        /// Gets the offset into the vertex list for the requested grid.
        /// </summary>
        /// <param name="x">The grid's x position.</param>
        /// <param name="z">The grid's z position.</param>
        /// <returns>The index offset into the vertex list.</returns>
        private int GetGridOffset(int x, int z)
        {
            // 5 vertices per grid. Calculated based on how it is generated in the GenerateTerrain method.
            return 5 * (x * GridZCount + z);
        }

        /// <summary>
        /// Gets a new mesh and adds it to the game object.
        /// </summary>
        /// <returns></returns>
        private Mesh GetMesh()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (GetComponent<MeshCollider>() == null)
            {
                gameObject.AddComponent<MeshCollider>();
            }

            if (GetComponent<MeshRenderer>() == null)
            {
                var renderer = gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = Resources.Load<Material>("Terrain/grid-grass");
            }

            meshFilter.mesh = new Mesh();
            meshFilter.mesh.name = "DynamicMesh";

            return meshFilter.sharedMesh;
        }

        /// <summary>
        /// Generates the terrain plane.
        /// </summary>
        /// <param name="mesh"></param>
        private void GenerateTerrain(Mesh mesh)
        {
            float halfGridSize = GridSize / 2;

            // Collection of all vertices
            // 5 Vertices per grid square. 4 corners + 1 center.
            int gridCount = GridXCount * GridZCount;
            int vertexCount = gridCount * 5;
            _vertices = new Vector3[vertexCount];

            int vertexIndex = 0;
            for (int x = 0; x < GridXCount; ++x)
            {
                for (int z = 0; z < GridZCount; ++z)
                {
                    _vertices[vertexIndex++] = new Vector3(x * GridSize, 0.0f, z * GridSize); // bottom-left
                    _vertices[vertexIndex++] = new Vector3((x + 1) * GridSize, 0.0f, z * GridSize); // bottom-right
                    _vertices[vertexIndex++] = new Vector3((x + 1) * GridSize, 0.0f, (z + 1) * GridSize); // top-right
                    _vertices[vertexIndex++] = new Vector3(x * GridSize, 0.0f, (z + 1) * GridSize); // top-left
                    _vertices[vertexIndex++] = new Vector3(x * GridSize + halfGridSize, 0.0f, z * GridSize + halfGridSize); // center
                }
            }

            // Collection of all UV coordinates for textures
            _uv = new Vector2[vertexCount];

            vertexIndex = 0;
            for (int x = 0; x < GridXCount; ++x)
            {
                for (int z = 0; z < GridZCount; ++z)
                {
                    _uv[vertexIndex++] = new Vector2(0.0f, 0.0f); // bottom-left
                    _uv[vertexIndex++] = new Vector2(1.0f, 0.0f); // bottom-right
                    _uv[vertexIndex++] = new Vector2(1.0f, 1.0f); // top-right
                    _uv[vertexIndex++] = new Vector2(0.0f, 1.0f); // top-left
                    _uv[vertexIndex++] = new Vector2(0.5f, 0.5f); // center
                }
            }

            // Create 4 triangles per grid square (each triangle has 3 vertices)
            _triangles = new int[4 * 3 * gridCount];

            vertexIndex = 0;
            int triangleIndex = 0;
            for (int x = 0; x < GridXCount; ++x)
            {
                for (int z = 0; z < GridZCount; ++z)
                {
                    int bottomLeft = vertexIndex++;
                    int bottomRight = vertexIndex++;
                    int topRight= vertexIndex++;
                    int topLeft = vertexIndex++;
                    int center = vertexIndex++;

                    _triangles[triangleIndex++] = bottomLeft; _triangles[triangleIndex++] = center; _triangles[triangleIndex++] = bottomRight;
                    _triangles[triangleIndex++] = bottomRight; _triangles[triangleIndex++] = center; _triangles[triangleIndex++] = topRight;
                    _triangles[triangleIndex++] = topRight; _triangles[triangleIndex++] = center; _triangles[triangleIndex++] = topLeft;
                    _triangles[triangleIndex++] = topLeft; _triangles[triangleIndex++] = center; _triangles[triangleIndex++] = bottomLeft;
                }
            }

            UpdateMesh();
        }

        /// <summary>
        /// Update the dynamic mesh with the new vertices, uvs and triangles.
        /// Update the normals, bounds and collider.
        /// </summary>
        private void UpdateMesh()
        {
            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _meshCollider.sharedMesh = _mesh;
        }
    }
}
