﻿using Common;
using GameData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Campus.GridTerrain
{
    /// <summary>
    /// Arguments structure to instantiate a grid mesh.
    /// </summary>
    public class GridMeshArgs
    {
        /// <summary>Size of each square in Unity world units.</summary>
        public float GridSquareSize;

        /// <summary>Size of each vertical step in the grid in Unity world units.</summary>
        public float GridStepSize;

        /// <summary>The size of the grid squares on the material.</summary>
        public int SubmaterialSize;

        /// <summary>Material to use on the grid.</summary>
        public Material GridMaterial;

        /// <summary>Number of grid squares along the x-axis.</summary>
        public int CountX;

        /// <summary>Number of grid steps along the y-axis.</summary>
        public int CountY;

        /// <summary>Number of grid squares along the z-axis.</summary>
        public int CountZ;

        /// <summary>The maximum number of grid steps down you can move the terrain.</summary>
        public int MaxDepth;

        /// <summary>The starting grid height of the mesh.</summary>
        public int[,] VertexHeights;

        /// <summary>The starting grid state of the mesh.</summary>
        public GridData[,] GridData;

        /// <summary>The starting anchored state of the mesh. (For safe terrain editor).</summary>
        public bool[,] GridAnchored;
    }

    /// <summary>
    /// A mesh created of square grids. Wraps around Unity's mesh behaviours.
    /// Supports setting the material on individual grids using dynamic submeshes.
    /// </summary>
    public class GridMesh : MonoBehaviour
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

        /// <summary>The maximum depth that the terrain can go down.</summary>
        public int MaxDepth { get; private set; }

        /// <summary>The size of submaterial squares.</summary>
        public int SubmaterialSize { get; private set; }

        /// <summary>Gets the number of submaterials available on this grid mesh.</summary>
        public int SubmaterialCount { get; private set; }

        /// <summary>Gets the unit converter.</summary>
        public GridConverter Convert { get; private set; }

        /// <summary>Gets the safe terrain editor.</summary>
        public SafeTerrainEditor Editor { get; private set; }

        /// <summary>Gets the Unity collider for the terrain.</summary>
        public Collider Collider { get { return _collider; } }

        /// <summary>Gets the selectable component of the grid. (Note: set after the constructor)</summary>
        public Selectable Selectable { get; set; }

        // Stores information about the grid square at [X, Z]
        private GridData[,] _gridData;

        // Stores the height of each vertex in the mesh.
        private int[,] _vertexHeight;

        private Mesh _mesh;
        private MeshCollider _collider;
        private MeshRenderer _renderer;

        private Vector3[] _vertices;
        private Vector2[] _uv;
        private List<int> _triangles;
        private Material _material;

        // The number of sub-sprites horizontally
        private int _submaterialCountX;
        // The number of sub-sprites vertically
        private int _submaterialCountZ;

        /// <summary>
        /// Instantiates an instance of the GridMesh.
        /// </summary>
        /// <param name="mesh">The Unity Mesh behavior.</param>
        /// <param name="collider">The Unity MeshCollider behavior.</param>
        /// <param name="renderer">The Unity MeshRenderer behavior.</param>
        /// <param name="args">The arguments used to create the GridMesh.</param>
        public void InitializeGridMesh(GridMeshArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            GameLogger.Info("Grid Mesh is loaded. Grid Count=({0}x{1}x{2}); Grid Size=({3:0.00f}x{4:0.00f}x{3:0.00f}); Material='{5}'; Submaterial Size={6}",
                args.CountX,
                args.CountY,
                args.CountZ,
                args.GridSquareSize,
                args.GridStepSize,
                args.GridMaterial == null ? "NULL" : args.GridMaterial.name,
                args.SubmaterialSize);

            _mesh = GetComponent<MeshFilter>()?.mesh ?? throw new InvalidOperationException("GridMesh must have a Mesh");
            _collider = GetComponent<MeshCollider>() ?? throw new InvalidOperationException("GridMesh must have a MeshCollider");
            _renderer = GetComponent<MeshRenderer>() ?? throw new InvalidOperationException("GridMesh must have a MeshRenderer");

            GridSquareSize = args.GridSquareSize;
            GridStepSize = args.GridStepSize;
            SubmaterialSize = args.SubmaterialSize;

            CountX = args.CountX;
            CountZ = args.CountZ;
            CountY = args.CountY;
            MaxDepth = args.MaxDepth;

            if (args.GridMaterial.mainTexture.width % SubmaterialSize != 0 ||
                args.GridMaterial.mainTexture.height % SubmaterialSize != 0)
            {
                throw new InvalidOperationException(string.Format("GridMesh material '{0}' is not a {1}x{1} grid sheet. [{2}x{3}]",
                    args.GridMaterial.name,
                    SubmaterialSize,
                    args.GridMaterial.mainTexture.width,
                    args.GridMaterial.mainTexture.height));
            }

            _submaterialCountX = (args.GridMaterial.mainTexture.width / SubmaterialSize);
            _submaterialCountZ = (args.GridMaterial.mainTexture.height / SubmaterialSize);
            SubmaterialCount = _submaterialCountX * _submaterialCountZ;

            _material = args.GridMaterial;
            _gridData = new GridData[CountX, CountZ];
            _vertexHeight = new int[CountX + 1, CountZ + 1];

            Convert = new GridConverter(
                gridSize: GridSquareSize,
                gridStepSize: GridStepSize,
                minTerrainX: gameObject.transform.position.x,
                minTerrainZ: gameObject.transform.position.z,
                minTerrainY: MaxDepth * -GridStepSize);

            Editor = new SafeTerrainEditor(this);

            // NB: Always flatten after generating the mesh
            GenerateMesh();
            Flatten(MaxDepth);
            LoadDefaultValues(args);
        }

        /// <summary>
        /// Check if a grid point is valid on this grid mesh.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Z coordinate of the grid square</param>
        /// <returns>True if the grid point is in the bounds of the mesh.</returns>
        public bool GridInBounds(int x, int z)
        {
            return  x >= 0 &&
                    x < CountX &&
                    z >= 0 &&
                    z < CountZ;
        }

        /// <summary>
        /// Check if a vertex point is valid on this grid mesh.
        /// </summary>
        /// <param name="x">X coordinate of the grid vertex.</param>
        /// <param name="z">Z coordinate of the grid vertex</param>
        /// <returns>True if the vertex point is in the bounds of the mesh.</returns>
        public bool VertexInBounds(int x, int z)
        {
            return  x >= 0 &&
                    x <= CountX &&
                    z >= 0 &&
                    z <= CountZ;
        }

        /// <summary>
        /// Gets the height of a grid square in grid steps.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <returns>The height of the grid square in grid steps.</returns>
        public int GetSquareHeight(int x, int z, int corner = Vertex.Center)
        {
            if (!GridInBounds(x, z))
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
            if (!GridInBounds(x, z))
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
            if (!VertexInBounds(x, z))
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
            if (!VertexInBounds(x, z))
                GameLogger.FatalError("Attempted to GetVertexWorldHeight out of range. ({0},{1})", x, z);

            return Convert.GridHeightToWorld(_vertexHeight[x, z]);
        }

        /// <summary>
        /// Checks if the grid square is flat or not.
        /// </summary>
        /// <param name="x">X coordinates of the grid square.</param>
        /// <param name="z">Z coorindates of the grid square.</param>
        /// <returns>True if the square is flat, false otherwise.</returns>
        public bool IsGridFlat(int x, int z)
        {
            if (!GridInBounds(x, z))
                GameLogger.FatalError("Attempted to get IsGridFlat out of range. ({0},{1})", x, z);

            return _vertexHeight[x, z] == _vertexHeight[x + 1, z] &&
                _vertexHeight[x, z] == _vertexHeight[x, z + 1] &&
                _vertexHeight[x, z] == _vertexHeight[x + 1, z + 1];
        }

        /// <summary>
        /// Check if the grid square is smooth along a specified axis alignment.
        /// </summary>
        /// <param name="x">X coordinates of the grid square.</param>
        /// <param name="z">Z coorindates of the grid square.</param>
        /// <param name="alignment">The direction to check for smoothness.</param>
        /// <returns>True if the grid is smooth, false otherwise.</returns>
        public bool IsGridSmooth(int x, int z, AxisAlignment alignment)
        {
            if (!GridInBounds(x, z))
                GameLogger.FatalError("Attempted to get IsGridSmooth out of range. ({0},{1})", x, z);

            switch (alignment)
            {
                case AxisAlignment.None:
                    return
                        (GetVertexHeight(x, z) == GetVertexHeight(x, z + 1) &&
                         GetVertexHeight(x + 1, z) == GetVertexHeight(x + 1, z + 1)) ||
                        (GetVertexHeight(x, z) == GetVertexHeight(x + 1, z) &&
                         GetVertexHeight(x, z + 1) == GetVertexHeight(x + 1, z + 1));

                case AxisAlignment.XAxis:
                    return
                        GetVertexHeight(x, z) == GetVertexHeight(x, z + 1) &&
                        GetVertexHeight(x + 1, z) == GetVertexHeight(x + 1, z + 1);

                case AxisAlignment.ZAxis:
                    return
                        GetVertexHeight(x, z) == GetVertexHeight(x + 1, z) &&
                        GetVertexHeight(x, z + 1) == GetVertexHeight(x + 1, z + 1);

                default:
                    throw new ArgumentException($"Unknown AxisAlignment {alignment}.");
            }
        }

        /// <summary>
        /// Set the height of a grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Y coordinate of the grid square.</param>
        /// <param name="height">The square height in grid steps.</param>
        public void SetSquareHeight(int x, int z, int gridHeight)
        {
            if (!GridInBounds(x, z))
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

            if (!VertexInBounds(xBase, zBase) || !VertexInBounds(xBase + xLength - 1, zBase + zLength - 1))
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
        /// Gets the submaterial id of the grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Z coordinate of the grid square.</param>
        /// <returns>Id of the submaterial used at the grid square.</returns>
        public int GetSubmaterial(int x, int z)
        {
            if (!GridInBounds(x, z))
                GameLogger.FatalError("Attempted to get square submaterial outside of range! ({0},{1}) is outside of ({2},{3})", x, z, CountX, CountZ);

            return _gridData[x, z].SubmaterialIndex;
        }

        /// <summary>
        /// Sets the submaterial on a grid square.
        /// </summary>
        /// <param name="x">X coordinate of the grid square.</param>
        /// <param name="z">Z coordinate of the grid square.</param>
        /// <param name="submaterialId">The id of the submaterial (the gridsheet on the material from left->right, top->bottom).</param>
        /// <param name="rotation">Value to rotate the submaterial by. (Applied before inversion)</param>
        /// <param name="inversion">Whether or not to flip the submaterial. (Applied after rotation)</param>
        public void SetSubmaterial(int x, int z, int submaterialId, SubmaterialRotation rotation = SubmaterialRotation.deg0, SubmaterialInversion inversion = SubmaterialInversion.None)
        {
            if (!GridInBounds(x, z))
                GameLogger.FatalError("Attempted to set square material outside of range! ({0},{1}) is outside of ({2},{3})", x, z, CountX, CountZ);

            int submaterialOffsetX = submaterialId % _submaterialCountX;
            int submaterialOffsetZ = submaterialId / _submaterialCountX;

            if (submaterialOffsetZ >= _submaterialCountZ)
            {
                throw new InvalidOperationException(string.Format("Submaterial index '{0}' is out of range for material {1} ({2}x{3}).", submaterialId, _material.name, _submaterialCountX, _submaterialCountZ));
            }

            float stepX = (1.0f / _submaterialCountX);
            float stepZ = (1.0f / _submaterialCountZ);

            int rotationOffset = 0;
            switch (rotation)
            {
                case SubmaterialRotation.deg90:
                    rotationOffset = 3;
                    break;
                case SubmaterialRotation.deg180:
                    rotationOffset = 2;
                    break;
                case SubmaterialRotation.deg270:
                    rotationOffset = 1;
                    break;
            }

            var grid = _gridData[x, z];
            grid.SubmaterialIndex = submaterialId;
            grid.Rotation = rotation;
            grid.Inversion = inversion;

            _uv[grid.VertexIndex + (Vertex.BottomLeft + rotationOffset) % 4] = new Vector2(submaterialOffsetX * stepX + Constant.uvEpsilon, 1.0f - (submaterialOffsetZ + 1) * stepZ + Constant.uvEpsilon);
            _uv[grid.VertexIndex + (Vertex.BottomRight + rotationOffset) % 4] = new Vector2((submaterialOffsetX + 1) * stepX - Constant.uvEpsilon, 1.0f - (submaterialOffsetZ + 1) * stepZ + Constant.uvEpsilon);
            _uv[grid.VertexIndex + (Vertex.TopRight + rotationOffset) % 4] = new Vector2((submaterialOffsetX + 1) * stepX - Constant.uvEpsilon, 1.0f - submaterialOffsetZ * stepZ - Constant.uvEpsilon);
            _uv[grid.VertexIndex + (Vertex.TopLeft + rotationOffset) % 4] = new Vector2(submaterialOffsetX * stepX + Constant.uvEpsilon, 1.0f - submaterialOffsetZ * stepZ - Constant.uvEpsilon);
            _uv[grid.VertexIndex + Vertex.Center] = new Vector2(submaterialOffsetX * stepX + (stepX / 2), 1.0f - submaterialOffsetZ * stepZ - (stepZ / 2));

            if ((inversion & SubmaterialInversion.InvertX) == SubmaterialInversion.InvertX)
            {
                Vector2 swapTop = _uv[grid.VertexIndex + Vertex.TopRight];
                _uv[grid.VertexIndex + Vertex.TopRight] = _uv[grid.VertexIndex + Vertex.TopLeft];
                _uv[grid.VertexIndex + Vertex.TopLeft] = swapTop;

                Vector2 swapBottom = _uv[grid.VertexIndex + Vertex.BottomRight];
                _uv[grid.VertexIndex + Vertex.BottomRight] = _uv[grid.VertexIndex + Vertex.BottomLeft];
                _uv[grid.VertexIndex + Vertex.BottomLeft] = swapBottom;
            }

            if ((inversion & SubmaterialInversion.InvertZ) == SubmaterialInversion.InvertZ)
            {
                Vector2 swapLeft = _uv[grid.VertexIndex + Vertex.TopLeft];
                _uv[grid.VertexIndex + Vertex.TopLeft] = _uv[grid.VertexIndex + Vertex.BottomLeft];
                _uv[grid.VertexIndex + Vertex.BottomLeft] = swapLeft;

                Vector2 swapRight = _uv[grid.VertexIndex + Vertex.TopRight];
                _uv[grid.VertexIndex + Vertex.TopRight] = _uv[grid.VertexIndex + Vertex.BottomRight];
                _uv[grid.VertexIndex + Vertex.BottomRight] = swapRight;
            }

            _mesh.uv = _uv;
        }

        /// <summary>
        /// Save a snapshot of the grid mesh for game saving.
        /// </summary>
        /// <returns></returns>
        public TerrainSaveState SaveGameState()
        {
            return new TerrainSaveState
            {
                CountX = CountX,
                CountY = CountY,
                CountZ = CountZ,
                MaxDepth = MaxDepth,
                VertexHeight = _vertexHeight,
                GridData = _gridData,
                GridAnchored = Editor.GridAnchored,
            };
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
            _triangles = new List<int>(3 * Vertex.TrianglesPerSquare * gridCount);

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
                    float stepX = (1.0f / _submaterialCountX);
                    float stepZ = (1.0f / _submaterialCountZ);
                    _uv[uvIndex++] = new Vector2(0.0f + Constant.uvEpsilon, 1.0f - stepZ + Constant.uvEpsilon); // bottom-left
                    _uv[uvIndex++] = new Vector2(stepX - Constant.uvEpsilon, 1.0f - stepZ + Constant.uvEpsilon); // bottom-right
                    _uv[uvIndex++] = new Vector2(stepX - Constant.uvEpsilon, 1.0f - Constant.uvEpsilon); // top-right
                    _uv[uvIndex++] = new Vector2(0.0f + Constant.uvEpsilon, 1.0f - Constant.uvEpsilon); // top-left
                    _uv[uvIndex++] = new Vector2(stepX / 2, 1.0f - (stepZ / 2)); // center

                    // Generate Triangle triplets
                    int bottomLeft = triangleVertexIndex++;
                    int bottomRight = triangleVertexIndex++;
                    int topRight = triangleVertexIndex++;
                    int topLeft = triangleVertexIndex++;
                    int center = triangleVertexIndex++;

                    _triangles.Add(bottomLeft); _triangles.Add(center); _triangles.Add(bottomRight);
                    _triangles.Add(bottomRight); _triangles.Add(center); _triangles.Add(topRight);
                    _triangles.Add(topRight); _triangles.Add(center); _triangles.Add(topLeft);
                    _triangles.Add(topLeft); _triangles.Add(center); _triangles.Add(bottomLeft);
                }
            }

            UpdateMesh();
        }

        /// <summary>
        /// Set default values for the terrain.
        /// </summary>
        /// <param name="args"></param>
        private void LoadDefaultValues(GridMeshArgs args)
        {
            if (args.VertexHeights != null)
            {
                if (args.VertexHeights.GetLength(0) != _vertexHeight.GetLength(0) ||
                    args.VertexHeights.GetLength(1) != _vertexHeight.GetLength(1))
                {
                    GameLogger.FatalError("Attempting to load vertex heights from invalid array! Required = {0}x{1}; Given = ({2}x{3})",
                        _vertexHeight.GetLength(0),
                        _vertexHeight.GetLength(1),
                        args.VertexHeights.GetLength(0),
                        args.VertexHeights.GetLength(1));
                }
                else
                {
                    SetVertexHeights(0, 0, args.VertexHeights);
                }
            }

            if (args.GridData != null)
            {
                if (args.GridData.GetLength(0) != _gridData.GetLength(0) ||
                    args.GridData.GetLength(1) != _gridData.GetLength(1))
                {
                    GameLogger.FatalError("Attempting to load grid data from invalid array! Required = {0}x{1}; Given = ({2}x{3})",
                        _gridData.GetLength(0),
                        _gridData.GetLength(1),
                        args.GridData.GetLength(0),
                        args.GridData.GetLength(1));
                }
                else
                {
                    for (int x = 0; x < args.GridData.GetLength(0); ++x)
                    {
                        for (int z = 0; z < args.GridData.GetLength(1); ++z)
                        {
                            GridData dataToCopy = args.GridData[x, z];
                            SetSubmaterial(x, z, dataToCopy.SubmaterialIndex, dataToCopy.Rotation, dataToCopy.Inversion);
                        }
                    }
                }
            }

            if (args.GridAnchored != null)
            {
                if (args.GridAnchored.GetLength(0) != _gridData.GetLength(0) ||
                    args.GridAnchored.GetLength(1) != _gridData.GetLength(1))
                {
                    GameLogger.FatalError("Attempting to load grid anchor data from invalid array! Required = {0}x{1}; Given = ({2}x{3})",
                        _gridData.GetLength(0),
                        _gridData.GetLength(1),
                        args.GridAnchored.GetLength(0),
                        args.GridAnchored.GetLength(1));
                }
                else
                {
                    for (int x = 0; x < args.GridAnchored.GetLength(0); ++x)
                    {
                        for (int z = 0; z < args.GridAnchored.GetLength(1); ++z)
                        {
                            if (args.GridAnchored[x, z])
                            {
                                Editor.SetAnchored(x, z);
                            }
                        }
                    }
                }
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
            _mesh.SetTriangles(_triangles, 0);

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _collider.sharedMesh = _mesh;
            _renderer.materials = new[] { _material };
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
