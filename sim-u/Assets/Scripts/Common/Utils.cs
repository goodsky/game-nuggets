using System;
using System.Collections.Generic;
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
        /// Camp a value between a low and high range.
        /// </summary>
        /// <param name="val">Value to clamp</param>
        /// <param name="lo">Low range</param>
        /// <param name="hi">High range</param>
        /// <returns>The value between Low and High</returns>
        public static double Clamp(double val, int lo, int hi)
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

        /// <summary>
        /// Iterate over a multi-dimensional array and return the distinct elements.
        /// </summary>
        /// <typeparam name="T">The type of the multi-dimensional array.</typeparam>
        /// <param name="items">The multi-dimensional array.</param>
        /// <returns>The distinct non-null elements from the items array.</returns>
        public static IEnumerable<T> GetDistinct<T>(T[,] items)
        {
            var distinctBuildings = new HashSet<T>();
            for (int x = 0; x < items.GetLength(0); ++x)
            {
                for (int z = 0; z < items.GetLength(1); ++z)
                {
                    if (items[x, z] != null)
                    {
                        distinctBuildings.Add(items[x, z]);
                    }
                }
            }

            return distinctBuildings;
        }

        /// <summary>
        /// Copy all items from one multi-dimensional array into another.
        /// </summary>
        /// <typeparam name="T">The type of the multi-dimensional array.</typeparam>
        /// <param name="source">The source multi-dimensional array.</param>
        /// <param name="destination">The destination multi-dimensional array.</param>
        public static void CopyArray<T>(T[,] source, T[,] destination)
        {
            if (source.GetLength(0) != destination.GetLength(0) ||
                source.GetLength(1) != destination.GetLength(1))
            {
                throw new InvalidOperationException($"Could not copy arrays with different sizes. Src={source.GetLength(0)}x{source.GetLength(1)}. Dst={destination.GetLength(0)}x{destination.GetLength(1)}");
            }

            for (int x = 0; x < source.GetLength(0); ++x)
            {
                for (int z = 0; z < source.GetLength(1); ++z)
                {
                    destination[x, z] = source[x, z];
                }
            }
        }
    }
}
