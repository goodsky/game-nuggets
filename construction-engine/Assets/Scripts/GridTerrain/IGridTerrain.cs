using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Represents a square gridded terrain object.
    /// Can edit and change materials at runtime.
    /// </summary>
    public interface IGridTerrain
    {
        /// <summary>
        /// Gets the length of a side of the grid squares.
        /// </summary>
        float Size { get; }

        /// <summary>
        /// Gets the count of grid squares along the x-axis.
        /// </summary>
        int CountX { get; }

        /// <summary>
        /// Gets the count of grid squares along the z-axis.
        /// </summary>
        int CountZ { get; }

        /// <summary>
        /// Gets the maximum number of grid steps along the y-axis.
        /// </summary>
        int CountY { get; }

        /// <summary>
        /// Gets a converter for switching between grid and world units.
        /// </summary>
        IGridTerrainConverter Convert { get; }

        /// <summary>
        /// Gets the grid height of a square.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The grid y position of the grid square</returns>
        int GetSquareHeight(int x, int z);

        /// <summary>
        /// Gets the world height of a square.
        /// </summary>
        /// <param name="x">Grid x position</param>
        /// <param name="z">Grid y position</param>
        /// <returns>The world y position of the grid square</returns>
        float GetSquareWorldHeight(int x, int z);

        /// <summary>
        /// Gets the grid height of a vertex.
        /// </summary>
        /// <param name="x">Point x position</param>
        /// <param name="z">Point z position</param>
        /// <returns>The grid y position of the vertex</returns>
        int GetPointHeight(int x, int z);

        /// <summary>
        /// Gets the world height of a vertex.
        /// </summary>
        /// <param name="x">Point x position</param>
        /// <param name="z">Point z position</param>
        /// <returns>The world y position of the vertex</returns>
        float GetPointWorldHeight(int x, int z);

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
        /// Gets the material id at a grid square.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <returns>The id of the material on this square.</returns>
        int GetMaterial(int x, int z);

        /// <summary>
        /// Sets the material id at a grid square.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <param name="materialId">The material id to set on this square. The id is determined by the order in the MeshRenderer.</param>
        void SetMaterial(int x, int z, int materialId);

        /// <summary>
        /// Flatten the entire terrain to a certain height
        /// </summary>
        /// <param name="gridHeight">Grid y height</param>
        void Flatten(int gridHeight = 0);

        /// <summary>
        /// Raycast against the terrain.
        /// </summary>
        /// <param name="ray">Ray to cast along.</param>
        /// <param name="hit">Hit information.</param>
        /// <param name="maxDistance">Max distance to cast along the ray.</param>
        /// <returns>True if the collission occurred, false otherwise.</returns>
        bool Raycast(Ray ray, out RaycastHit hit, float maxDistance);
    }
}
