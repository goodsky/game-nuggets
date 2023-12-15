﻿using Common;
using UnityEngine;

namespace Campus.GridTerrain
{
    /// <summary>
    /// Helpful class to convert units between grid squares and Unity world units.
    /// </summary>
    public class GridConverter
    {
        private float _gridSize;
        private float _halfGridSize;
        private float _gridStepSize;

        private float _minTerrainX;
        private float _minTerrainZ;
        private float _minTerrainY;

        /// <summary>
        /// Deltas to get the 4 adjacent grids around a grid plus the grid itself
        /// </summary>
        public static readonly int[] AdjacentPlusSelfGridDx = new int[] { 0, 0, 1, 0, -1 };
        public static readonly int[] AdjacentPlusSelfGridDz = new int[] { 0, 1, 0, -1, 0 };

        /// <summary>
        /// Deltas to get the 4 adjacent grids around a grid
        /// </summary>
        public static readonly int[] AdjacentGridDx = new int[] { 0, 1, 0, -1 };
        public static readonly int[] AdjacentGridDz = new int[] { 1, 0, -1, 0 };

        /// <summary>
        /// Deltas to get the 4 adjacent vertices around a vertex
        /// </summary>
        public static readonly int[] AdjacentVertexDx = new int[] { 0, 1, 0, -1 };
        public static readonly int[] AdjacentVertexDz = new int[] { 1, 0, -1, 0 };

        /// <summary>
        /// Deltas to get the 4 vertices around a grid
        /// </summary>
        public static readonly int[] GridToVertexDx = new int[] { 1, 1, 0, 0 };
        public static readonly int[] GridToVertexDz = new int[] { 1, 0, 0, 1 };

        /// <summary>
        /// Deltas to ge the 4 grids around a vertex
        /// </summary>
        public static readonly int[] VertexToGridDx = new int[] { -1, -1, 0, 0 };
        public static readonly int[] VertexToGridDz = new int[] { -1, 0, -1, 0 };

        /// <summary>
        /// Create an instance of the grid terrain converter.
        /// </summary>
        /// <param name="gridSize">Size of a grid square.</param>
        /// <param name="gridStepSize">Size of a grid height step.</param>
        /// <param name="minTerrainX">Minimum terrain position on the x-axis.</param>
        /// <param name="minTerrainZ">Minimum terrain position on the z-azis.</param>
        /// <param name="minTerrainY">Minimum terrain position on the y-axis.</param>
        public GridConverter(float gridSize, float gridStepSize, float minTerrainX, float minTerrainZ, float minTerrainY)
        {
            _gridSize = gridSize;
            _halfGridSize = gridSize / 2.0f;
            _gridStepSize = gridStepSize;
            _minTerrainX = minTerrainX;
            _minTerrainZ = minTerrainZ;
            _minTerrainY = minTerrainY;
        }

        /// <summary>
        /// Convert a world point to grid coordinates.
        /// </summary>
        /// <param name="world">Point in Unity world space.</param>
        /// <returns>The coorisponding grid coordinate.</returns>
        public Point3 WorldToGrid(Vector3 world)
        {
            return new Point3(
                Mathf.FloorToInt((world.x - _minTerrainX) / _gridSize + Utils.Epsilon),
                Mathf.FloorToInt((world.y - _minTerrainY) / _gridStepSize + Utils.Epsilon),
                Mathf.FloorToInt((world.z - _minTerrainZ) / _gridSize + Utils.Epsilon));
        }

        /// <summary>
        /// Convert a world point to the nearest vertex on the grid.
        /// </summary>
        /// <param name="world">Point in Unity world space.</param>
        /// <returns>The coorisponding grid vertex coordinate.</returns>
        public Point2 WorldToGridVertex(Vector3 world)
        {
            return new Point2(
                Mathf.FloorToInt(((world.x + _halfGridSize) - _minTerrainX) / _gridSize + Utils.Epsilon),
                Mathf.FloorToInt(((world.z + _halfGridSize) - _minTerrainZ) / _gridSize + Utils.Epsilon));
        }

        /// <summary>
        /// Convert a grid coordinate into world point at the origin of the grid.
        /// </summary>
        /// <param name="grid">Grid point.</param>
        /// <returns>The world coordinate.</returns>
        public Vector3 GridToWorld(Point3 grid)
        {
            return new Vector3(
                grid.x * _gridSize + _minTerrainX,
                grid.y * _gridStepSize + _minTerrainY,
                grid.z * _gridSize + _minTerrainZ);
        }

        /// <summary>
        /// Convert a grid coordinate into world point at the center of the grid.
        /// </summary>
        /// <param name="grid">Grid point.</param>
        /// <returns>The world coordinate.</returns>
        public Vector3 GridCenterToWorld(Point3 grid)
        {
            return new Vector3(
                grid.x * _gridSize + _minTerrainX + _halfGridSize,
                grid.y * _gridStepSize + _minTerrainY,
                grid.z * _gridSize + _minTerrainZ + _halfGridSize);
        }

        /// <summary>
        /// Convert a world coordinate height into grid units.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public int WorldHeightToGrid(float world)
        {
            return Mathf.FloorToInt((world - _minTerrainY) / _gridStepSize + Utils.Epsilon);
        }

        /// <summary>
        /// Convert a grid height to world units.
        /// </summary>
        /// <param name="grid">The grid height step.</param>
        /// <returns>World coordinate of the grid height.</returns>
        public float GridHeightToWorld(int grid)
        {
            return grid * _gridStepSize + _minTerrainY;
        }
    }
}
