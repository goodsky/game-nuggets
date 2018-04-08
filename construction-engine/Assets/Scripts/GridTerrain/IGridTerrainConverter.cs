using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Helpful class to convert units between grid squares and Unity world units.
    /// </summary>
    public interface IGridTerrainConverter
    {
        /// <summary>
        /// Convert a world point to grid coordinates.
        /// </summary>
        /// <param name="world">Point in Unity world space.</param>
        /// <returns>The coorisponding grid coordinate.</returns>
        Point3 WorldToGrid(Vector3 world);

        /// <summary>
        /// Convert a grid coordinate into world point at the origin of the grid.
        /// </summary>
        /// <param name="grid">Grid point.</param>
        /// <returns>The world coordinate.</returns>
        Vector3 GridToWorld(Point3 grid);

        /// <summary>
        /// Convert a grid coordinate into world point at the center of the grid.
        /// </summary>
        /// <param name="grid">Grid point.</param>
        /// <returns>The world coordinate.</returns>
        Vector3 GridCenterToWorld(Point3 grid);

        /// <summary>
        /// Convert a world coordinate height into grid units.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        int WorldHeightToGrid(float world);

        /// <summary>
        /// Convert a grid height to world units.
        /// </summary>
        /// <param name="grid">The grid height step.</param>
        /// <returns>World coordinate of the grid height.</returns>
        float GridHeightToWorld(int grid);
    }
}
