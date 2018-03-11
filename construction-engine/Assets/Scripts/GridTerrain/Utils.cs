namespace GridTerrain
{
    public static class Utils
    {
        /// <summary>
        /// Camp a value between a low and high range.
        /// </summary>
        /// <param name="val">Value to clamp</param>
        /// <param name="lo">Low range</param>
        /// <param name="hi">High range</param>
        /// <returns>The value between Low and High</returns>
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
