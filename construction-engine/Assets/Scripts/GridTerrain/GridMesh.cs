using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace GridTerrain
{
    /// <summary>
    /// Arguments structure to instantiate a grid mesh.
    /// </summary>
    public class GridMeshArgs
    {
        /// <summary>Size of each square in Unity world units.</summary>
        public float GridSquareSize = 1.0f;

        /// <summary>Size of each vertical step in the grid in Unity world units.</summary>
        public float GridStepSize = 0.4f;

        /// <summary>Number of grid squares along the x-axis.</summary>
        public int CountX = 16;

        /// <summary>Number of grid steps along the y-axis.</summary>
        public int CountY = 8;

        /// <summary>Number of grid squares along the z-axis.</summary>
        public int CountZ = 16;

        /// <summary>The starting grid height of the mesh.</summary>
        public int StartingHeight = 3;

        /// <summary>Materials to use in the grid mesh. Will create a submesh for each material.</summary>
        public Material[] Materials = null;

        /// <summary>The default material for the terrain.</summary>
        public int DefaultMaterial = 0;

        /// <summary>True if a skirt of non-gridmesh should be generated around the gridmesh.</summary>
        public bool Skirt = true;
    }

    /// <summary>
    /// A mesh created of square grids. Wraps around Unity's mesh behaviours.
    /// Supports setting the material on individual grids using dynamic submeshes.
    /// </summary>
    public class GridMesh : IDisposable
    {
        /// <summary>Gets the length of the sides of a grid square in Unity world units.</summary>
        public float GridSquareSize { get; private set; }

        /// <summary>Gets the size of a vertical step on the grid in Unity world units.</summary>
        public float GridStepSize { get; private set; }

        /// <summary>Gets the number of grid squares along the x-axis.</summary>
        public int CountX { get; private set; }

        /// <summary>Gets the number of grid squares along the z-axis.</summary>
        public int CountZ { get; private set; }

        /// <summary>Gets the number of grid squares along the y-axis.</summary>
        public int CountY { get; private set; }

        /// <summary>Gets the number of materials available on this grid mesh.</summary>
        public int MaterialCount { get; private set; }

        /// <summary>Gets the default material id used in this mesh.</summary>
        public int DefaultMaterial { get; private set; }

        /// <summary>Gets the unit converter.</summary>
        public GridConverter Convert { get; private set; }

        /// <summary>Gets the Unity collider for the terrain.</summary>
        public Collider Collider { get { return _collider; } }

        /// <summary>Gets the Unity game object that the mesh behaviours are attached to.</summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// Stores pointers into the mesh to locate a grid square.
        /// </summary>
        private class GridData
        {
            public int VertexIndex;
            public int TriangleIndex;
            public int MaterialIndex;
        }

        // Stores information about the grid square at [X, Z]
        private GridData[,] _gridData;

        // Stores the height of each vertex in the mesh.
        private int[,] _vertexHeight;

        // A reverse-lookup of grid data using the TriangleIndex
        // Used during submesh rotations
        private Dictionary<int, GridData>[] _gridDataTriLookup;

        private Mesh _mesh;
        private MeshCollider _collider;
        private MeshRenderer _renderer;

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private List<int>[] _triangles;
        private Material[] _materials;

        /// <summary>
        /// Instantiates an instance of the GridMesh.
        /// </summary>
        /// <param name="mesh">The Unity Mesh behavior.</param>
        /// <param name="collider">The Unity MeshCollider behavior.</param>
        /// <param name="renderer">The Unity MeshRenderer behavior.</param>
        /// <param name="args">The arguments used to create the GridMesh.</param>
        public GridMesh(Mesh mesh, MeshCollider collider, MeshRenderer renderer, GridMeshArgs args)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (collider == null)
                throw new ArgumentNullException("collider");

            if (renderer == null)
                throw new ArgumentNullException("renderer");

            if (args == null)
                throw new ArgumentNullException("args");

            GameLogger.Info("Generating GridMesh. {0}x{1} squares @ {2:0.00f}; Starting height {3} of {4} @ {5:0.00f}; Submesh count = {6}",
                args.CountX, 
                args.CountZ,
                args.GridSquareSize, 
                args.StartingHeight, 
                args.CountY, 
                args.GridStepSize,
                args.Materials == null ? "NULL" : args.Materials.Length.ToString());

            _mesh = mesh;
            _collider = collider;
            _renderer = renderer;
            GameObject = renderer.gameObject;

            GridSquareSize = args.GridSquareSize;
            GridStepSize = args.GridStepSize;
            CountX = args.CountX;
            CountZ = args.CountZ;
            CountY = args.CountY;
            MaterialCount = args.Materials.Length;

            _materials = args.Materials;
            _gridData = new GridData[CountX, CountZ];
            _vertexHeight = new int[CountX + 1, CountZ + 1];
            _gridDataTriLookup = new Dictionary<int, GridData>[MaterialCount];

            Convert = new GridConverter(
                gridSize: GridSquareSize,
                gridStepSize: GridStepSize,
                minTerrainX: GameObject.transform.position.x,
                minTerrainZ: GameObject.transform.position.z,
                minTerrainY: args.StartingHeight * -GridStepSize);

            if (args.Skirt)
            {
                var skirtPrefab = Resources.Load<GameObject>("terrain_skirt");
                var skirtObject = UnityEngine.Object.Instantiate(skirtPrefab);
                skirtObject.transform.parent = GameObject.transform;

                // the skirt prefab is rotated 90 degrees, so scale y-axis instead of z
                skirtObject.transform.localScale = new Vector3(CountX * GridSquareSize, CountZ * GridSquareSize, 1.0f);
            }

            GenerateMesh();
            Flatten(args.StartingHeight);
        }

        /// <summary>
        /// Gets the height of a grid square in grid steps.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <returns>The height of the grid square in grid steps.</returns>
        public int GetSquareHeight(int x, int z, int corner = Vertex.Center)
        {
            if (x < 0 || x >= CountX || z < 0 || z >= CountZ)
                GameLogger.FatalError("Attempted to GetSquareHeight out of range. ({0},{1})", x, z);

            int centerIndex = _gridData[x, z].VertexIndex + corner;
            return Convert.WorldHeightToGrid(_vertices[centerIndex].y);
        }

        /// <summary>
        /// Gets the height of a grid square in world units.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <returns>The height of the grid square in Unity world units.</returns>
        public float GetSquareWorldHeight(int x, int z, int corner = Vertex.Center)
        {
            if (x < 0 || x >= CountX || z < 0 || z >= CountZ)
                GameLogger.FatalError("Attempted to GetSquareWorldHeight out of range. ({0},{1})", x, z);

            int centerIndex = _gridData[x, z].VertexIndex + corner;
            return _vertices[centerIndex].y;
        }

        /// <summary>
        /// Gets the height of a mesh vertex in grid steps.
        /// (Remember, there is one more vertex than grid squares).
        /// </summary>
        /// <param name="x">X coordinate of the vertex.</param>
        /// <param name="z">Z coordinates of the vertex.</param>
        /// <returns>The height of the vertex in grid steps.</returns>
        public int GetVertexHeight(int x, int z)
        {
            if (x < 0 || x > CountX || z < 0 || z > CountZ)
                GameLogger.FatalError("Attempted to GetVertexHeight out of range. ({0},{1})", x, z);

            return _vertexHeight[x, z];
        }

        /// <summary>
        /// Gets the height of a mesh vertex in world units.
        /// (Remember, there is one more vertex than grid squares).
        /// </summary>
        /// <param name="x">X coordinate of the vertex.</param>
        /// <param name="z">Z coordinates of the vertex.</param>
        /// <returns>The height of the vertex in Unity world units.</returns>
        public float GetVertexWorldHeight(int x, int z)
        {
            if (x < 0 || x > CountX || z < 0 || z > CountZ)
                GameLogger.FatalError("Attempted to GetVertexWorldHeight out of range. ({0},{1})", x, z);

            return Convert.GridHeightToWorld(_vertexHeight[x, z]);
        }

        /// <summary>
        /// Set the height of a grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <param name="height">The square height in grid steps.</param>
        public void SetSquareHeight(int x, int z, int gridHeight)
        {
            if (x < 0 || x >= CountX || z < 0 || z >= CountZ)
                GameLogger.FatalError("Attempted to set square height outside of range! ({0},{1}) is outside of ({2},{3})", x, z, CountX, CountZ);

            SetVertexHeights(x, z, new int[,] { { gridHeight, gridHeight }, { gridHeight, gridHeight } });
        }

        /// <summary>
        /// Set the heights of several vertices in the grid.
        /// </summary>
        /// <param name="xBase">The starting x coordinate.</param>
        /// <param name="zBase">The starting z coordinate.</param>
        /// <param name="heights">The square heights in grid steps.</param>
        public void SetVertexHeights(int xBase, int zBase, int[,] gridHeights)
        {
            // Reminder: vertices are one larger than square grid
            //           the bounds of this method are [0, CountX] [0, CountZ] inclusive
            int xLength = gridHeights.GetLength(0);
            int zLength = gridHeights.GetLength(1);

            if (xBase < 0 || xBase + xLength > CountX + 1 || zBase < 0 || zBase + zLength > CountZ + 1)
                GameLogger.FatalError("Attempted to set vertex height outside of range! ({0},{1}) + ({2},{3}) is outside of ({4},{5})", xBase, zBase, xLength, zLength, CountX + 1, CountZ + 1);

            // 1st pass: set the corner vertices
            for (int x = 0; x < xLength; ++x)
            {
                for (int z = 0; z < zLength; ++z)
                {
                    _vertexHeight[xBase + x, zBase + z] = gridHeights[x, z];
                    float worldHeight = Convert.GridHeightToWorld(gridHeights[x, z]);

                    // there are potentially 4 overlaps for each corner vertex.
                    // keep them all in sync!
                    int leftX = xBase + x - 1;
                    int rightX = xBase + x;
                    int downZ = zBase + z - 1;
                    int upZ = zBase + z;

                    if (leftX >= 0 && downZ >= 0)
                    {
                        int index = _gridData[leftX, downZ].VertexIndex + Vertex.TopRight;
                        _vertices[index].y = worldHeight;
                    }

                    if (leftX >= 0 && upZ < CountZ)
                    {
                        int index = _gridData[leftX, upZ].VertexIndex + Vertex.BottomRight;
                        _vertices[index].y = worldHeight;
                    }

                    if (rightX < CountX && downZ >= 0)
                    {
                        int index = _gridData[rightX, downZ].VertexIndex + Vertex.TopLeft;
                        _vertices[index].y = worldHeight;
                    }

                    if (rightX < CountX && upZ < CountZ)
                    {
                        int index = _gridData[rightX, upZ].VertexIndex + Vertex.BottomLeft;
                        _vertices[index].y = worldHeight;
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

                    if (gridX >= 0 && gridX < CountX && gridZ >= 0 && gridZ < CountZ)
                    {
                        int index = _gridData[gridX, gridZ].VertexIndex;
                        float newHeight = Utils.GetMajorityOrAverage(
                            _vertices[index + Vertex.BottomLeft].y,
                            _vertices[index + Vertex.BottomRight].y,
                            _vertices[index + Vertex.TopRight].y,
                            _vertices[index + Vertex.TopLeft].y);

                        _vertices[index + Vertex.Center].y = newHeight;
                    }
                }
            }

            UpdateMesh();
        }

        /// <summary>
        /// Flatten the mesh to a set height.
        /// </summary>
        /// <param name="gridHeight">The height to flatten to. Defaults to zero.</param>
        public void Flatten(int gridHeight = 0)
        {
            var resetHeights = new int[CountX + 1, CountZ + 1];
            for (int i = 0; i < CountX + 1; ++i)
                for (int j = 0; j < CountZ + 1; ++j)
                    resetHeights[i, j] = gridHeight;

            SetVertexHeights(0, 0, resetHeights);
        }

        /// <summary>
        /// Gets the material id of the grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Z coordinate of the grid square.</param>
        /// <returns>Id of the material used at the grid square.</returns>
        public int GetMaterial(int x, int z)
        {
            return _gridData[x, z].MaterialIndex;
        }

        /// <summary>
        /// Sets the material on a grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Z coordinate of the grid square.</param>
        /// <param name="materialId">The id of the material to use at the grid square.</param>
        public void SetMaterial(int x, int z, int materialId)
        {
            try
            {
                int oldMaterialId = _gridData[x, z].MaterialIndex;
                int oldTriangleIndex = _gridData[x, z].TriangleIndex;

                if (oldMaterialId == materialId)
                    return;

                List<int> oldSubmesh = _triangles[oldMaterialId];
                List<int> newSubmesh = _triangles[materialId];
                int newTriangleIndex = newSubmesh.Count;

                // Move the triangles out of the old material sub mesh and into the end of the new material sub mesh.
                int toMoveCount = Vertex.TrianglesPerSquare * 3;
                for (int i = 0; i < toMoveCount; ++i)
                {
                    newSubmesh.Add(oldSubmesh[oldTriangleIndex + i]);
                }

                // Overwrite the old submesh triangle list with the last element.
                // Remove elements only from the end of the List.
                int oldSubmeshLength = oldSubmesh.Count;
                for (int i = 0; i < toMoveCount; ++i)
                {
                    oldSubmesh[oldTriangleIndex + toMoveCount - 1 - i] = oldSubmesh[oldSubmeshLength - 1 - i];
                    oldSubmesh.RemoveAt(oldSubmeshLength - 1 - i);
                }

                // keep our pointers state consistent
                _gridDataTriLookup[materialId][newTriangleIndex] = _gridData[x, z];
                _gridDataTriLookup[oldMaterialId].Remove(oldTriangleIndex);

                _gridData[x, z].MaterialIndex = materialId;
                _gridData[x, z].TriangleIndex = newTriangleIndex;

                int displacedTriangleIndex = oldSubmeshLength - toMoveCount;
                if (oldSubmesh.Count > 0 && displacedTriangleIndex != oldTriangleIndex)
                {
                    GridData displacedGrid = _gridDataTriLookup[oldMaterialId][displacedTriangleIndex];

                    Assert.AreEqual(displacedTriangleIndex, displacedGrid.TriangleIndex, "Grid TriangleIndex reverse lookup is corrupt!!!");

                    displacedGrid.TriangleIndex = oldTriangleIndex;
                    _gridDataTriLookup[oldMaterialId][oldTriangleIndex] = displacedGrid;
                    _gridDataTriLookup[oldMaterialId].Remove(displacedTriangleIndex);
                }

                UpdateMesh();
            }
            catch (Exception ex)
            {
                GameLogger.FatalError("Exception while updating terrain material! Ex = {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Generate the mesh based on the arguments set on the instance.
        /// </summary>
        private void GenerateMesh()
        {
            int gridCount = CountX * CountZ;
            int vertexCount = gridCount * Vertex.CountPerSquare;

            _vertices = new Vector3[vertexCount];
            _uv = new Vector2[vertexCount];
            _triangles = new List<int>[MaterialCount];

            // 3 vertices per triangle
            for (int i = 0; i < MaterialCount; ++i)
            {
                _triangles[i] = new List<int>(3 * Vertex.TrianglesPerSquare * gridCount);
                _gridDataTriLookup[i] = new Dictionary<int, GridData>();
            }

            List<int> defaultSubmesh = _triangles[DefaultMaterial];

            int vertexIndex = 0;
            int uvIndex = 0;
            int triangleVertexIndex = 0;
            for (int x = 0; x < CountX; ++x)
            {
                for (int z = 0; z < CountZ; ++z)
                {
                    // Keep pointer to start of square in vertices array
                    _gridData[x, z] = new GridData();
                    _gridData[x, z].VertexIndex = vertexIndex;

                    // Generate Vertices
                    _vertices[vertexIndex++] = new Vector3(x * GridSquareSize, 0.0f, z * GridSquareSize); // bottom-left
                    _vertices[vertexIndex++] = new Vector3((x + 1) * GridSquareSize, 0.0f, z * GridSquareSize); // bottom-right
                    _vertices[vertexIndex++] = new Vector3((x + 1) * GridSquareSize, 0.0f, (z + 1) * GridSquareSize); // top-right
                    _vertices[vertexIndex++] = new Vector3(x * GridSquareSize, 0.0f, (z + 1) * GridSquareSize); // top-left
                    _vertices[vertexIndex++] = new Vector3(x * GridSquareSize + (GridSquareSize / 2), 0.0f, z * GridSquareSize + (GridSquareSize / 2)); // center

                    // Generate UV coordinates for textures
                    _uv[uvIndex++] = new Vector2(0.0f, 0.0f); // bottom-left
                    _uv[uvIndex++] = new Vector2(1.0f, 0.0f); // bottom-right
                    _uv[uvIndex++] = new Vector2(1.0f, 1.0f); // top-right
                    _uv[uvIndex++] = new Vector2(0.0f, 1.0f); // top-left
                    _uv[uvIndex++] = new Vector2(0.5f, 0.5f); // center

                    // Keep pointer to start of square in triangles array
                    _gridData[x, z].TriangleIndex = defaultSubmesh.Count;
                    _gridData[x, z].MaterialIndex = DefaultMaterial;
                    _gridDataTriLookup[DefaultMaterial][_gridData[x, z].TriangleIndex] = _gridData[x, z];

                    // Generate Triangle triplets
                    int bottomLeft = triangleVertexIndex++;
                    int bottomRight = triangleVertexIndex++;
                    int topRight = triangleVertexIndex++;
                    int topLeft = triangleVertexIndex++;
                    int center = triangleVertexIndex++;

                    defaultSubmesh.Add(bottomLeft); defaultSubmesh.Add(center); defaultSubmesh.Add(bottomRight);
                    defaultSubmesh.Add(bottomRight); defaultSubmesh.Add(center); defaultSubmesh.Add(topRight);
                    defaultSubmesh.Add(topRight); defaultSubmesh.Add(center); defaultSubmesh.Add(topLeft);
                    defaultSubmesh.Add(topLeft); defaultSubmesh.Add(center); defaultSubmesh.Add(bottomLeft);
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

            int subMeshCount = _triangles.Count(submesh => submesh.Count > 0);
            var activeMaterials = new Material[subMeshCount];
            _mesh.subMeshCount = subMeshCount;

            int subMeshIndex = 0;
            for (int i = 0; i < _triangles.Length; ++i)
            {
                if (_triangles[i].Count > 0)
                {
                    _mesh.SetTriangles(_triangles[i], subMeshIndex);
                    activeMaterials[subMeshIndex] = _materials[i];
                    ++subMeshIndex;
                }
            }

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _collider.sharedMesh = _mesh;
            _renderer.materials = activeMaterials;
        }

        /// <summary>
        /// Dispose dynamic resources.
        /// </summary>
        public void Dispose()
        {
            if (_mesh != null)
            {
                _mesh.Clear();
            }
        }

        /// <summary>
        /// Helper to access vertices in a square.
        /// </summary>
        private static class Vertex
        {
            public const int BottomLeft = 0;
            public const int BottomRight = 1;
            public const int TopRight = 2;
            public const int TopLeft = 3;
            public const int Center = 4;

            public const int CountPerSquare = 5;
            public const int TrianglesPerSquare = 4;
        }
    }
}
