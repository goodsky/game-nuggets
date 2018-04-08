using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace GridTerrain
{
    /// <summary>
    /// Helper to access vertices in a square.
    /// </summary>
    internal static class Vertex
    {
        public const int BottomLeft = 0;
        public const int BottomRight = 1;
        public const int TopRight = 2;
        public const int TopLeft = 3;
        public const int Center = 4;

        public const int CountPerSquare = 5;
        public const int TrianglesPerSquare = 4;
    }

    /// <summary>
    /// Creates a grid terrain mesh using Unity's mesh behaviours.
    /// Supports setting the material on individual grids using dynamic submeshes.
    /// </summary>
    public class GridMesh : IDisposable
    {
        private int _countX;
        private int _countZ;
        private int _materialCount;

        private Mesh _mesh;
        private MeshCollider _collider;
        private MeshRenderer _renderer;

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private List<int>[] _triangles;
        private Material[] _materials;

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
        private float[,] _vertexHeight;

        // A reverse-lookup of grid data using the TriangleIndex
        // Used during submesh rotations
        private Dictionary<int, GridData>[] _gridDataTriLookup;

        /// <summary>Gets the number of grid squares along the x-axis.</summary>
        public int CountX { get { return _countX; } }

        /// <summary>Gets the number of grid squares along the z-axis.</summary>
        public int CountZ { get { return _countZ; } }

        /// <summary>Gets the number of material submeshes in this grid.</summary>
        public int MaterialCount { get { return _materialCount; } }

        /// <summary>Gets the Unity collider for the terrain.</summary>
        public Collider Collider { get { return _collider; } }

        /// <summary>
        /// Instantiates an instance of the GridMesh.
        /// </summary>
        /// <param name="mesh">The Unity Mesh behavior.</param>
        /// <param name="collider">The Unity MeshCollider behavior.</param>
        /// <param name="renderer">The Unity MeshRenderer behavior.</param>
        public GridMesh(Mesh mesh, MeshCollider collider, MeshRenderer renderer)
        {
            _mesh = mesh;
            _collider = collider;
            _renderer = renderer;
        }

        /// <summary>
        /// Create the Unity mesh.
        /// </summary>
        /// <param name="gridSize">Size of each square in Unity world units.</param>
        /// <param name="countX">Number of grid squares along the x-axis.</param>
        /// <param name="countZ">Number of grid squares along the z-axis.</param>
        /// <param name="materials">Materials to use in the mesh. Will create a submesh for each material.</param>
        /// <param name="defaultMaterial">The default material to color the terrain.</param>
        public void Initialize(float gridSize, int countX, int countZ, Material[] materials, int defaultMaterial = 0)
        {
            _countX = countX;
            _countZ = countZ;
            _materialCount = materials.Length;
            _materials = materials;
            _gridData = new GridData[countX, countZ];
            _vertexHeight = new float[countX + 1, countZ + 1];
            _gridDataTriLookup = new Dictionary<int, GridData>[_materialCount];

            int gridCount = countX * countZ;
            int vertexCount = gridCount * Vertex.CountPerSquare;

            _vertices = new Vector3[vertexCount];
            _uv = new Vector2[vertexCount];
            _triangles = new List<int>[_materialCount];

            // 3 vertices per triangle
            for (int i = 0; i < _materialCount; ++i)
            {
                _triangles[i] = new List<int>(3 * Vertex.TrianglesPerSquare * gridCount);
                _gridDataTriLookup[i] = new Dictionary<int, GridData>();
            }

            List<int> defaultSubmesh = _triangles[defaultMaterial];

            int vertexIndex = 0;
            int uvIndex = 0;
            int triangleVertexIndex = 0;
            for (int x = 0; x < countX; ++x)
            {
                for (int z = 0; z < countZ; ++z)
                {
                    // Keep pointer to start of square in vertices array
                    _gridData[x, z] = new GridData();
                    _gridData[x, z].VertexIndex = vertexIndex;

                    // Generate Vertices
                    _vertices[vertexIndex++] = new Vector3(x * gridSize, 0.0f, z * gridSize); // bottom-left
                    _vertices[vertexIndex++] = new Vector3((x + 1) * gridSize, 0.0f, z * gridSize); // bottom-right
                    _vertices[vertexIndex++] = new Vector3((x + 1) * gridSize, 0.0f, (z + 1) * gridSize); // top-right
                    _vertices[vertexIndex++] = new Vector3(x * gridSize, 0.0f, (z + 1) * gridSize); // top-left
                    _vertices[vertexIndex++] = new Vector3(x * gridSize + (gridSize / 2), 0.0f, z * gridSize + (gridSize / 2)); // center

                    // Generate UV coordinates for textures
                    _uv[uvIndex++] = new Vector2(0.0f, 0.0f); // bottom-left
                    _uv[uvIndex++] = new Vector2(1.0f, 0.0f); // bottom-right
                    _uv[uvIndex++] = new Vector2(1.0f, 1.0f); // top-right
                    _uv[uvIndex++] = new Vector2(0.0f, 1.0f); // top-left
                    _uv[uvIndex++] = new Vector2(0.5f, 0.5f); // center

                    // Keep pointer to start of square in triangles array
                    _gridData[x, z].TriangleIndex = defaultSubmesh.Count;
                    _gridData[x, z].MaterialIndex = defaultMaterial;
                    _gridDataTriLookup[defaultMaterial][_gridData[x, z].TriangleIndex] = _gridData[x, z];

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
        /// Gets the height of a grid square in world units.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <returns>The height of the grid square in Unity world units.</returns>
        public float GetHeight(int x, int z, int corner = Vertex.Center)
        {
            int centerIndex = _gridData[x, z].VertexIndex + corner;
            return _vertices[centerIndex].y;
        }

        /// <summary>
        /// Gets the height of a mesh vertex in world units.
        /// (Remember, there are one more of these than grid squares).
        /// </summary>
        /// <param name="x">X coordinate of the vertex.</param>
        /// <param name="z">Z coordinates of the vertex.</param>
        /// <returns>The height of the vertex in Unity world units.</returns>
        public float GetVertexHeight(int x, int z)
        {
            return _vertexHeight[x, z];
        }

        /// <summary>
        /// Set the height of a grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <param name="height">The height in Unity world units.</param>
        public void SetSquareHeight(int x, int z, float height)
        {
            SetVertexHeights(x, z, new float[,] { { height, height }, { height, height } });
        }

        /// <summary>
        /// Set the heights of several vertices in the grid.
        /// </summary>
        /// <param name="xBase">The starting x coordinate.</param>
        /// <param name="zBase">The starting z coordinate.</param>
        /// <param name="heights">The heights to set in Unity world units.</param>
        public void SetVertexHeights(int xBase, int zBase, float[,] heights)
        {
            // Reminder: vertices are one larger than square grid
            //           the bounds of this method are [0, CountX] [0, CountZ] inclusive
            int xLength = heights.GetLength(0);
            int zLength = heights.GetLength(1);

            // 1st pass: set the corner vertices
            for (int x = 0; x < xLength; ++x)
            {
                for (int z = 0; z < zLength; ++z)
                {
                    _vertexHeight[xBase + x, zBase + z] = heights[x, z];

                    // there are potentially 4 overlaps for each corner vertex.
                    // keep them all in sync!
                    int leftX = xBase + x - 1;
                    int rightX = xBase + x;
                    int downZ = zBase + z - 1;
                    int upZ = zBase + z;

                    if (leftX >= 0 && downZ >= 0)
                    {
                        int index = _gridData[leftX, downZ].VertexIndex + Vertex.TopRight;
                        _vertices[index].y = heights[x, z];
                    }

                    if (leftX >= 0 && upZ < _countZ)
                    {
                        int index = _gridData[leftX, upZ].VertexIndex + Vertex.BottomRight;
                        _vertices[index].y = heights[x, z];
                    }

                    if (rightX < _countX && downZ >= 0)
                    {
                        int index = _gridData[rightX, downZ].VertexIndex + Vertex.TopLeft;
                        _vertices[index].y = heights[x, z];
                    }

                    if (rightX < _countX && upZ < _countZ)
                    {
                        int index = _gridData[rightX, upZ].VertexIndex + Vertex.BottomLeft;
                        _vertices[index].y = heights[x, z];
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

                    if (gridX >= 0 && gridX < _countX && gridZ >= 0 && gridZ < _countZ)
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

        public void Dispose()
        {
            if (_mesh != null)
            {
                _mesh.Clear();
            }
        }
    }
}
