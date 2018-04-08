using Common;
using System;
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

        /// <summary>Size of a grid square</summary>
        public float Size { get { return GridSize; } }

        /// <summary>Number of grid squares along the x-axis.</summary>
        public int CountX { get { return GridXCount; } }

        /// <summary>Number of grid squares along the y-axis.</summary>
        public int CountY { get { return GridStepsUp + GridStepsDown + 1; } }

        /// <summary>Number of grid squares along the z-axis.</summary>
        public int CountZ { get { return GridZCount; } }

        /// <summary>Gets the unit converter.</summary>
        public IGridTerrainConverter Convert { get; private set; }

        private Grid[,] _gridData;

        /// <summary>
        /// Unity's Start
        /// </summary>
        protected void Start()
        {
            Convert = new GridTerrainConverter(
                gridSize: GridSize,
                gridStepSize: GridStepSize,
                minTerrainX: transform.position.x,
                minTerrainZ: transform.position.z,
                minTerrainY: GridStepsDown * -GridStepSize);

            InitializeGrid();

            if (Editable)
            {
                gameObject.AddComponent<EditableTerrain>();
                // editableTerrain.CursorPrefab = Resources.Load<GameObject>("cursor");
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
        /// Gets the grid height of a square.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The grid y position of the grid square</returns>
        public int GetSquareHeight(int x, int z)
        {
            if (x < 0 || x >= GridXCount || z < 0 || z >= GridZCount)
                GameLogger.FatalError("Attempted to GetWorldHeight out of range. ({0},{1})", x, z);

            return Convert.WorldHeightToGrid(_gridData[x, z].vertices[Grid.Center].y);
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

            return Convert.WorldHeightToGrid(_gridData[x, z].vertices[Grid.Center].y);
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

            return _gridData[x, z].vertices[Grid.Center].y;
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
                    float newHeight = Convert.GridHeightToWorld(heights[x, z]);

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
                        new Vector3(x * GridSize + GridSize / 2, 0.0f, z * GridSize + GridSize / 2), // center
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
