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

        public CampusPaths(GridMesh terrain)
        {
            _terrain = terrain;
        }
    }
}
