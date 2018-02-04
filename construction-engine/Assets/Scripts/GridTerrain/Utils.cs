using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridTerrain
{
    public static class Utils
    {
        public static int Clamp(int val, int lo, int hi)
        {
            if (val < lo)
                return lo;

            if (val > hi)
                return hi;

            return val;
        }
    }
}
