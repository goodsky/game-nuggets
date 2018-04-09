using System;
using System.Collections.Generic;

namespace GridTerrain
{
    /// <summary>
    /// Class used to make safe changes to terrain
    /// It checks on what moves are valid.
    /// It smooths out blocky changes.
    /// </summary>
    internal class SafeTerrainEditor
    {
        // Shortcut for BFS
        private readonly int[] dx = new[] { -1, 0, 1, 0 };
        private readonly int[] dy = new[] { 0, -1, 0, 1 };

        private GridMesh _terrain;
        private bool[,] _gridAnchored;

        /// <summary>
        /// Instantiates an instance of a SafeTerrainEditor.
        /// </summary>
        /// <param name="terrain">The terrain</param>
        public SafeTerrainEditor(GridMesh terrain)
        {
            _terrain = terrain;

            var width = _terrain.CountX + 1;
            var height = _terrain.CountZ + 1;
            _gridAnchored = new bool[width, height];

            // Anchor all corners, otherwise start unanchored
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                    _gridAnchored[i, j] = (i == 0 || j == 0 || i == width - 1 || j == height - 1) ? true : false;
        }

        /// <summary>
        /// Attempt to set the grid height for a square.
        /// Will enforce smooth terrain.
        /// </summary>
        /// <param name="x">Grid x position.</param>
        /// <param name="z">Grid z position.</param>
        /// <param name="gridHeight">Grid height to set.</param>
        /// <returns>True if the set succeeded. False otherwise.</returns>
        public bool SafeSetHeight(int x, int z, int gridHeight)
        {
            var s = new Queue<Point2>();
            var visited = new HashSet<Point2>();
            var setHeights = new Dictionary<Point2, int>();

            var p1 = new Point2(x, z);
            var p2 = new Point2(x, z + 1);
            var p3 = new Point2(x + 1, z);
            var p4 = new Point2(x + 1, z + 1);

            if (_gridAnchored[p1.x, p1.y] || _gridAnchored[p2.x, p2.y] || _gridAnchored[p3.x, p3.y] || _gridAnchored[p4.x, p4.y])
                return false;

            // bounding box around the changes
            int minX = x;
            int maxX = x + 1;
            int minY = z;
            int maxY = z + 1;

            // initial state with the corners to traverse
            s.Enqueue(p1); s.Enqueue(p2); s.Enqueue(p3); s.Enqueue(p4);
            visited.Add(p1); visited.Add(p2); visited.Add(p3); visited.Add(p4);
            setHeights[p1] = setHeights[p2] = setHeights[p3] = setHeights[p4] = gridHeight;

            // breadth-first search to smooth the terrain around the change
            int visitedCount = 0;
            while (s.Count > 0)
            {
                if (visitedCount++ > 1023)
                    throw new InvalidOperationException(string.Format("Attempting to set height at ({0},{1}) resulted in too many operations!", x, z));

                var cur = s.Dequeue();

                minX = Math.Min(minX, cur.x);
                maxX = Math.Max(maxX, cur.x);
                minY = Math.Min(minY, cur.y);
                maxY = Math.Max(maxY, cur.y);

                for (int i = 0; i < dx.Length; ++i)
                {
                    var test = new Point2(cur.x + dx[i], cur.y + dy[i]);

                    if (visited.Contains(test))
                        continue;

                    // we are okay with grid difference up to 1 step
                    var heightDiff = _terrain.GetVertexHeight(test.x, test.y) - setHeights[cur];

                    if (heightDiff < -1)
                    {
                        if (_gridAnchored[test.x, test.y])
                            return false;

                        setHeights[test] = setHeights[cur] - 1;
                        s.Enqueue(test);
                        visited.Add(test);
                    }

                    if (heightDiff > 1)
                    {
                        if (_gridAnchored[test.x, test.y])
                            return false;

                        setHeights[test] = setHeights[cur] + 1;
                        s.Enqueue(test);
                        visited.Add(test);
                    }
                }
            }

            int sizeX = maxX - minX + 1;
            int sizeY = maxY - minY + 1;
            var newGridHeights = new int[sizeX, sizeY];
            for (int i = 0; i < sizeX; ++i)
            {
                for (int j = 0; j < sizeY; ++j)
                {
                    var p = new Point2(minX + i, minY + j);
                    newGridHeights[i, j] = setHeights.ContainsKey(p) ? setHeights[p] : _terrain.GetVertexHeight(minX + i, minY + j);
                }
            }

            _terrain.SetVertexHeights(minX, minY, newGridHeights);
            return true;
        }

        /// <summary>
        /// Write all heights to the Console.
        /// </summary>
        /// <param name="heights"></param>
        public static void DumpHeights(int[,] heights)
        {
            for (int j = 0; j < heights.GetLength(1); ++j)
            {
                for (int i = 0; i < heights.GetLength(0); ++i)
                    Console.Write(heights[i, j] + " ");
                Console.WriteLine();
            }
        }
    }
}