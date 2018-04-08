using UnityEngine;

namespace Common
{
    public static class Utils
    {
        // This is trying to counteract floating point errors. No guarantees though.
        public const float Epsilon = 0.0001f;

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

        /// <summary>
        /// Return either the majority or the average (if there is a tie).
        /// I think this is optimizing this algorithm since we only have 4 floats.
        /// Maybe I'm just being lazy and don't want to use a sort.
        /// </summary>
        /// <param name="f1">1st float</param>
        /// <param name="f2">2nd float</param>
        /// <param name="f3">3rd float</param>
        /// <param name="f4">4th float</param>
        /// <returns>The mode or the average (if there is not a above 50% average)</returns>
        public static float GetMajorityOrAverage(float f1, float f2, float f3, float f4)
        {
            // The logic here is if we find two who are equal then we need one more to make it a mode.
            // However, if there is no other, then we are best case in a tie. So return the average.
            if (Mathf.Approximately(f1, f2))
            {
                if (Mathf.Approximately(f1, f3) || Mathf.Approximately(f1, f4))
                {
                    return f1;
                }
            }
            else if (Mathf.Approximately(f1, f3))
            {
                if (Mathf.Approximately(f1, f4))
                {
                    return f1;
                }
            }
            else if (Mathf.Approximately(f2, f3))
            {
                if (Mathf.Approximately(f2, f4))
                {
                    return f2;
                }
            }

            return (f1 + f2 + f3 + f4) / 4;
        }
    }
}
