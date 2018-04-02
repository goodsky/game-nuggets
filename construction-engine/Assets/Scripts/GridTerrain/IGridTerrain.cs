using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GridTerrain
{
    public interface IGridTerrain
    {
        /// <summary>
        /// Gets the count of grid squares along the x-axis.
        /// </summary>
        int CountX { get; }

        /// <summary>
        /// Gets the maximum number of grid steps along the y-axis.
        /// </summary>
        int CountY { get; }

        /// <summary>
        /// Gets the count of grid squares along the z-axis.
        /// </summary>
        int CountZ { get; }

        /// <summary>
        /// Gets the world height of a square.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The world y position of the grid square</returns>
        float GetWorldHeight(int x, int z);

        /// <summary>
        /// Gets the grid height of a control point.
        /// </summary>
        /// <param name="x">Point x position</param>
        /// <param name="z">Point z position</param>
        /// <returns>The grid y position of the grid square</returns>
        int GetPointHeight(int x, int z);

        /// <summary>
        /// Set a grid square to a grid height.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid z position</param>
        /// <param name="gridHeight">Grid y height</param>
        void SetHeight(int x, int z, int gridHeight);

        /// <summary>
        /// Set an area of points to a grid height.
        /// </summary>
        /// <param name="xBase">Starting x point</param>
        /// <param name="zBase">Starting z point</param>
        /// <param name="heights">Grid heights to set</param>
        void SetPointHeights(int xBase, int zBase, int[,] heights);

        /// <summary>
        /// Flatten the entire terrain to a certain height
        /// </summary>
        /// <param name="gridHeight">Grid y height</param>
        void Flatten(int gridHeight = 0);

        Point3 ConvertWorldToGrid(Vector3 world);

        Vector3 ConvertGridToWorld(Point3 grid);

        Vector3 ConvertGridCenterToWorld(Point3 grid);
    }
}
