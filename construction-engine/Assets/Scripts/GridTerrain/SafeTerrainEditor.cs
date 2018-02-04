using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Class used to make safe changes to terrain
    /// It checks on what moves are valid.
    /// It smooths out blocky changes.
    /// </summary>
    internal class SafeTerrainEditor
    {
        private GridTerrainData _terrain;
        private bool[,] _gridAnchored;

        public SafeTerrainEditor(GridTerrainData terrain)
        {
            _terrain = terrain;

            var width = _terrain.GridCountX;
            var height = _terrain.GridCountZ;
            _gridAnchored = new bool[width, height];

            // Anchor all corners, otherwise start unanchored
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                    _gridAnchored[i, j] = (i == 0 || j == 0 || i == width - 1 || j == height - 1) ? true : false;
        }

        public void SafeSetHeight(int x, int z, int gridHeight)
        {
            var s = new Stack<Point2>();
            var visited = new HashSet<Point2>();
            var newHeight = new Dictionary<Point2, int>();

            var p1 = new Point2(x, z);
            var p2 = new Point2(x, z + 1);
            var p3 = new Point2(x + 1, z);
            var p4 = new Point2(x + 1, z + 1);

            s.Push(p1); s.Push(p2); s.Push(p3); s.Push(p4);
            visited.Add(p1); visited.Add(p2); visited.Add(p3); visited.Add(p4);
            newHeight[p1] = newHeight[p2] = newHeight[p3] = newHeight[p4] = gridHeight;

            int visitedCount = 0;
            while (s.Count > 0)
            {
                if (visitedCount++ > 1024)
                    throw new InvalidOperationException(string.Format("Attempting to set height at ({0},{1}) resulted in too many operations!", x, z));


            }
        }
    }
}