using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campus
{
    public class CampusRoads
    {
        private GridMesh _terrain;
        private bool[,] _road;

        public CampusRoads(CampusData campusData, GridMesh terrain)
        {
            _terrain = terrain;
            _road = new bool[campusData.Terrain.GridCountX + 1, campusData.Terrain.GridCountZ + 1];
            SetupRoadMapping(
                emptyGrassIndex: campusData.Terrain.SubmaterialEmptyGrassIndex,
                roadStartIndex: campusData.Terrain.SubmaterialRoadsIndex);
        }

        /// <summary>
        /// Gets a value representing whether or not there is a road at grid position.
        /// Note: The Roads values stored in this class are vertex based, but his 
        ///       query is grid based.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if there is a raod, false otherwise.</returns>
        public bool RoadAtPosition(Point2 pos)
        {
            return
                pos.x >= 0 && pos.x < _terrain.CountX &&
                pos.z >= 0 && pos.z < _terrain.CountZ &&
                ( 
                    _road[pos.x, pos.z] ||
                    _road[pos.x + 1, pos.z] ||
                    _road[pos.x, pos.z + 1] ||
                    _road[pos.x + 1, pos.z + 1]
                );
        }

        /// <summary>
        /// Build a road along the provided line
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="z">Z position</param>
        public void ConstructRoad(Point3 start, Point3 end)
        {
            int dx = 0;
            int dz = 0;
            int length = 1;

            if (start.x == end.x && start.z == end.z)
            {
                // Case: Building a single square
            }
            else if (start.x != end.x)
            {
                // Case: Building a line along the x-axis
                dx = start.x < end.x ? 1 : -1;
                length = Math.Abs(start.x - end.x) + 1;
            }
            else
            {
                // Case: Building a line along the z-axis
                dz = start.z < end.z ? 1 : -1;
                length = Math.Abs(start.z - end.z) + 1;
            }

            // Set the paths
            for (int i = 0; i < length; ++i)
            {
                int gridX = start.x + i * dx;
                int gridZ = start.z + i * dz;

                if (!_road[gridX, gridZ])
                {
                    _road[gridX, gridZ] = true;
                    _terrain.Editor.SetAnchored(gridX, gridZ);
                }
            }

            // Set the updated materials
            for (int scanX = Math.Min(start.x, end.x) - 1; scanX <= Math.Max(start.x, end.x) + 1; ++scanX)
                for (int scanZ = Math.Min(start.z, end.z) - 1; scanZ <= Math.Max(start.z, end.z) + 1; ++scanZ)
                    UpdateRoadMaterial(scanX, scanZ);
        }

        /// <summary>
        /// Remove a path at the position.
        /// </summary>
        /// <param name="pos">The position to remove the path at.</param>
        public void DestroyRoadAt(Point2 pos)
        {
            if (_road[pos.x, pos.z])
            {
                _road[pos.x, pos.z] = false;
                _terrain.Editor.RemoveAnchor(pos.x, pos.z);
            }

            for (int scanX = pos.x - 1; scanX <= pos.x + 1; ++scanX)
                for (int scanZ = pos.z - 1; scanZ <= pos.z + 1; ++scanZ)
                    UpdateRoadMaterial(scanX, scanZ);
        }

        /// <summary>
        /// Update the material of the grid to look like the path.
        /// </summary>
        readonly int[] dx = new[] { 1, 1, 0, 0 };
        readonly int[] dz = new[] { 0, 1, 1, 0 };
        private void UpdateRoadMaterial(int x, int z)
        {
            if (x < 0 || x > _terrain.CountX ||
                z < 0 || z > _terrain.CountZ)
            {
                return;
            }

            // check the 4 adjacent road vertices to pick the correct image
            int[] adj = new int[4];
            for (int i = 0; i < 4; ++i)
            {
                int checkX = x + dx[i];
                int checkZ = z + dz[i];
                adj[i] =
                    (checkX > 0 && checkX <= _terrain.CountX &&
                        checkZ > 0 && checkZ <= _terrain.CountZ &&
                        _road[checkX, checkZ])
                        ? 1 : 0;
            }

            _terrain.SetSubmaterial(
                x,
                z,
                _subMaterial[adj[0], adj[1], adj[2], adj[3]],
                _rotation[adj[0], adj[1], adj[2], adj[3]]);
        }

        // mapping from adjacent road vertices to the material + rotation
        // [top-right, bottom-right, bottom-left, top-left]
        // D--A
        // |  |
        // C--B
        private int[,,,] _subMaterial;
        private Rotation[,,,] _rotation;
        private void SetupRoadMapping(int emptyGrassIndex, int roadStartIndex)
        {
            _subMaterial = new int[2, 2, 2, 2];
            _rotation = new Rotation[2, 2, 2, 2];

            // no adjacent ---
            _subMaterial[0, 0, 0, 0] = emptyGrassIndex;
            _rotation[0, 0, 0, 0] = Rotation.deg0;

            // one adjacent ---
            // top-right
            _subMaterial[1, 0, 0, 0] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[1, 0, 0, 0] = Rotation.deg0;
            // bottom-right
            _subMaterial[0, 1, 0, 0] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[0, 1, 0, 0] = Rotation.deg90;
            // bottom-left
            _subMaterial[0, 0, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[0, 0, 1, 0] = Rotation.deg180;
            // top-left
            _subMaterial[0, 0, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[0, 0, 0, 1] = Rotation.deg270;

            // two adjacent (straight) ---
            // top-right & bottom-right
            _subMaterial[1, 1, 0, 0] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[1, 1, 0, 0] = Rotation.deg0;
            // bottom-right & bottom-left
            _subMaterial[0, 1, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[0, 1, 1, 0] = Rotation.deg90;
            // bottom-left & top-left
            _subMaterial[0, 0, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[0, 0, 1, 1] = Rotation.deg180;
            // top-left & top-right
            _subMaterial[1, 0, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[1, 0, 0, 1] = Rotation.deg270;

            // two adjacent (angled) ---
            // top-right & bottom-left
            _subMaterial[1, 0, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentAngledVertex;
            _rotation[1, 0, 1, 0] = Rotation.deg0;
            // bottom-right & top-left
            _subMaterial[0, 1, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentAngledVertex;
            _rotation[0, 1, 0, 1] = Rotation.deg90;

            // three adjacent ---
            // not top-left
            _subMaterial[1, 1, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[1, 1, 1, 0] = Rotation.deg0;
            // not top-right
            _subMaterial[0, 1, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[0, 1, 1, 1] = Rotation.deg90;
            // not bottom-right
            _subMaterial[1, 0, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[1, 0, 1, 1] = Rotation.deg180;
            // not bottom-left
            _subMaterial[1, 1, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[1, 1, 0, 1] = Rotation.deg270;

            // four adjacent ---
            _subMaterial[1, 1, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.FourAdjacentVertex;
            _rotation[1, 1, 1, 1] = Rotation.deg0;
        }

        /// <summary>
        /// This enum encodes the expected order of submaterials on the paths/roads sprite sheet.
        /// </summary>
        private enum PathSubmaterialIndex
        {
            OneAdjacentVertex = 0,
            TwoAdjacentStraightVertex = 1,
            TwoAdjacentAngledVertex = 2,
            ThreeAdjacentVertex = 3,
            FourAdjacentVertex = 4
        }
    }
}
