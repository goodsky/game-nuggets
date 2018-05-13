using GridTerrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Campus
{
    /// <summary>
    /// Collection of all paths on campus.
    /// </summary>
    public class CampusPaths
    {
        private GridMesh _terrain;
        private bool[,] _path;

        public CampusPaths(GridMesh terrain)
        {
            _terrain = terrain;
            _path = new bool[_terrain.CountX, _terrain.CountZ];
        }

        public void BuildPath(int startx, int startz, int endx, int endz)
        {
            if (startx != endx && startz != endz)
                throw new InvalidOperationException("Path must be built along an axis-aligned line.");

            int dx = 0;
            int dz = 0;
            int length = 1;

            if (startx != endx)
            {
                dx = (startx < endx) ? 1 : -1;
                length = Math.Abs(startx - endx) + 1;
            }

            if (startz != endz)
            {
                dz = (startz < endz) ? 1 : -1;
                length = Math.Abs(startz - endz) + 1;
            }

            int posx = startx;
            int posz = startz;
            for (int cursorIndex = 0; cursorIndex < length; ++cursorIndex)
            {
                _path[posx, posz] = true;

                posx += dx;
                posz += dz;
            }

            posx = startx - dx;
            posz = startz - dz;
            for (int cursorIndex = 0; cursorIndex < length + 2; ++cursorIndex)
            {
                // this is a gross way to try to get the surrounding grids around the line we updated
                if (dx == 0)
                {
                    for (int scanx = posx - 1; scanx <= posx + 1; ++scanx)
                    {
                        if (scanx >= 0 && scanx < _terrain.CountX &&
                            posz >= 0 && posz < _terrain.CountZ)
                            SetPathMaterial(scanx, posz);
                    }
                }
                else
                {
                    for (int scanz = posz - 1; scanz <= posz + 1; ++scanz)
                    {
                        if (scanz >= 0 && scanz < _terrain.CountZ &&
                            posx >= 0 && posx < _terrain.CountX)
                            SetPathMaterial(posx, scanz);
                    }
                }
                
                posx += dx;
                posz += dz;
            }
        }

        private void SetPathMaterial(int x, int z)
        {
            _terrain.SetMaterial(x, z, 1, 0);
        }
    }
}
