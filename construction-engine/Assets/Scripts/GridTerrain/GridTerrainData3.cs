using Common;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// A Custom terrain mesh based using a runtime generated plane mesh instead of Unity's heavy Terrain component.
    /// Creates a unique mesh per each grid square. This allows us to set multiple materials.
    /// 
    /// 'GridTerrain v3.1'
    /// </summary>
    public class GridTerrainData3 : MonoBehaviour, IGridTerrain
    {
        // Grid square game object
        private struct Grid
        {
            // grid mesh
            public Mesh mesh;
            public Vector3[] vertices;
            public static Vector2[] uv;
            public static int[] triangles;

            // convenience shortcuts for location of vertices
            public static readonly int BottomLeft = 0;
            public static readonly int BottomRight = 1;
            public static readonly int TopRight = 2;
            public static readonly int TopLeft = 3;
            public static readonly int Center = 4;

            // grid collider
            public MeshCollider collider;

            // grid renderer
            public MeshRenderer renderer;
            public Material material;
        }

        // This is trying to counteract floating point errors. No guarantees though.
        private const float ep = 0.001f;

        /// <summary>The default material on the terrain.</summary>
        public Material DefaultMaterial;

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

        public int CountX { get { return GridXCount; } }

        public int CountY { get { return GridStepsUp + GridStepsDown + 1; } }

        public int CountZ { get { return GridZCount; } }

        /// <summary>Half the grid step size. Used in conversions.</summary>
        private float HalfGridSize;

        // World Unit minimum for the terrain X-axis.
        private float MinTerrainX;

        // World Unit minimum for the terrain Z-axis.
        private float MinTerrainZ;

        private Grid[,] _gridData;

        /// <summary>
        /// Unity's Start
        /// </summary>
        protected void Start()
        {
            HalfGridSize = GridSize / 2;
            MinTerrainX = transform.position.x;
            MinTerrainZ = transform.position.z;
            MinTerrainHeight = GridStepsDown * -GridStepSize;

            InitializeGrid();

            if (Editable)
            {
                var editableTerrain = gameObject.AddComponent<EditableTerrain>();
                editableTerrain.CursorPrefab = Resources.Load<GameObject>("cursor");
            }

            Flatten(GridStepsDown);
        }

        /// <summary>
        /// Unity's Destroy
        /// </summary>
        protected void OnDestroy()
        {
            if (_gridData != null)
            {
                foreach (var square in _gridData)
                {
                    square.mesh.Clear();
                }
            }
        }

        /// <summary>
        /// Get the height of the center of a square in world coordinates.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <returns>The coordinates of the center of the grid.</returns>
        public float GetWorldHeight(int x, int z)
        {
            if (x < 0 || x >= GridXCount || z < 0 || z >= GridZCount)
                GameLogger.FatalError("Attempted to GetWorldHeight out of range. ({0},{1})", x, z);

            // return the height of the middle vertex of the square
            return _gridData[x, z].vertices[Grid.Center].y;
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

            return ConvertWorldHeightToGrid(_gridData[x, z].vertices[Grid.Center].y);
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
                    float newHeight = ConvertGridHeightToWorld(heights[x, z]);

                    // there are potentially 4 overlaps for each corner vertex.
                    // keep them all in sync!
                    int leftX = xBase + x - 1;
                    int rightX = xBase + x;
                    int downZ = zBase + z - 1;
                    int upZ = zBase + z;

                    if (leftX >= 0 && downZ >= 0)
                    {
                        _gridData[leftX, downZ].vertices[Grid.TopRight].y = newHeight;
                    }

                    if (leftX >= 0 && upZ < GridZCount)
                    {
                        _gridData[leftX, upZ].vertices[Grid.BottomRight].y = newHeight;
                    }

                    if (rightX < GridXCount && downZ >= 0)
                    {
                        _gridData[rightX, downZ].vertices[Grid.TopLeft].y = newHeight;
                    }

                    if (rightX < GridXCount && upZ < GridZCount)
                    {
                        _gridData[rightX, upZ].vertices[Grid.BottomLeft].y = newHeight;
                    }
                }
            }

            // 2nd pass: set the center vertices & update grid mesh/collider/renderer
            for (int x = -1; x < xLength + 1; ++x)
            {
                for (int z = -1; z < zLength + 1; ++z)
                {
                    int gridX = xBase + x;
                    int gridZ = zBase + z;

                    if (gridX >= 0 && gridX < GridXCount && gridZ >= 0 && gridZ < GridZCount)
                    {
                        float newHeight = Utils.GetMajorityOrAverage(
                            _gridData[gridX, gridZ].vertices[Grid.BottomLeft].y,
                            _gridData[gridX, gridZ].vertices[Grid.BottomRight].y,
                            _gridData[gridX, gridZ].vertices[Grid.TopRight].y,
                            _gridData[gridX, gridZ].vertices[Grid.TopLeft].y);

                        _gridData[gridX, gridZ].vertices[Grid.Center].y = newHeight;

                        UpdateGrid(_gridData[gridX, gridZ]);
                    }
                }
            }
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
            hit = new RaycastHit();
            hit.distance = float.MaxValue;

            bool hasHit = false;
            RaycastHit testHit = new RaycastHit();

            foreach (var grid in _gridData)
            {
                if (grid.collider.Raycast(ray, out testHit, maxDistance) &&
                    testHit.distance < hit.distance)
                {
                    hasHit = true;
                    hit = testHit;
                }
            }

            return hasHit;
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

        /// <summary>
        /// Generates the terrain plane.
        /// </summary>
        /// <param name="mesh"></param>
        private void InitializeGrid()
        {
            // Create terrain skirt if requested
            if (Skirt)
            {
                var skirt = Resources.Load<GameObject>("terrain_skirt");
                Instantiate(skirt);
            }

            // Triangle and UV coordinates are the same for each square
            Grid.triangles = new int[] 
            {
                Grid.BottomLeft, Grid.Center, Grid.BottomRight,
                Grid.BottomRight, Grid.Center, Grid.TopRight,
                Grid.TopRight, Grid.Center, Grid.TopLeft,
                Grid.TopLeft, Grid.Center, Grid.BottomLeft
            };

            Grid.uv = new Vector2[]
            {
                new Vector2(0.0f, 0.0f), // bottom left
                new Vector2(1.0f, 0.0f), // bottom right
                new Vector2(1.0f, 1.0f), // top right
                new Vector2(0.0f, 1.0f), // top left
                new Vector2(0.5f, 0.5f)  // center
            };

            _gridData = new Grid[GridXCount, GridZCount];

            for (int x = 0; x < GridXCount; ++x)
            {
                for (int z = 0; z < GridZCount; ++z)
                {
                    var squareObject = new GameObject("grid");
                    squareObject.transform.parent = gameObject.transform;

                    var filter = squareObject.AddComponent<MeshFilter>();
                    _gridData[x, z].mesh = filter.mesh = new Mesh();
                    _gridData[x, z].collider = squareObject.AddComponent<MeshCollider>();
                    _gridData[x, z].renderer = squareObject.AddComponent<MeshRenderer>();

                    _gridData[x, z].vertices = new Vector3[]
                    {
                        new Vector3(x * GridSize, 0.0f, z * GridSize), // bottom left
                        new Vector3((x + 1) * GridSize, 0.0f, z * GridSize), // bottom right
                        new Vector3((x + 1) * GridSize, 0.0f, (z + 1) * GridSize), // top right
                        new Vector3(x * GridSize, 0.0f, (z + 1) * GridSize), // top left
                        new Vector3(x * GridSize + HalfGridSize, 0.0f, z * GridSize + HalfGridSize), // center
                    };

                    _gridData[x, z].material = DefaultMaterial;

                    UpdateGrid(_gridData[x, z]);
                }
            }
        }

        /// <summary>
        /// Update the dynamic mesh with the new vertices, uvs and triangles.
        /// Update the normals, bounds and collider.
        /// </summary>
        private void UpdateGrid(Grid grid)
        {
            grid.mesh.Clear();
            grid.mesh.vertices = grid.vertices;
            grid.mesh.uv = Grid.uv;
            grid.mesh.triangles = Grid.triangles;

            grid.mesh.RecalculateNormals();
            grid.mesh.RecalculateBounds();

            grid.collider.sharedMesh = grid.mesh;

            grid.renderer.material = grid.material;
        }
    }
}
