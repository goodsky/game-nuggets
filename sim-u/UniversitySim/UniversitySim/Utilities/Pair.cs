using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim.Utilities
{
    /// <summary>
    /// Pair class.
    /// Stores a pair of values.
    /// </summary>
    public class Pair<T>
    {
        /// <summary>
        /// Create a pair
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Pair(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// First element in the pair
        /// </summary>
        public T x { get; set; }

        /// <summary>
        /// Second element in the pair
        /// </summary>
        public T y { get; set; }

        /// <summary>
        /// Create a clone of the values in this pair.
        /// Use this to stop awkward referencing bugs.
        /// </summary>
        /// <returns></returns>
        public Pair<T> Clone()
        {
            return new Pair<T>(this.x, this.y);
        }

        /// <summary>
        /// Compared to another Pair of T
        /// </summary>
        /// <param name="obj">The other</param>
        /// <returns>True if x and y are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            Pair<T> otherPair = obj as Pair<T>;
            if (otherPair == null)
                return false;

            return this.x.Equals(otherPair.x) && this.y.Equals(otherPair.y);
        }

        /// <summary>
        /// Hash the Pair
        /// I don't think I actually use this anywhere... but the .NET compiler complained when I override Equals but not GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            string hash = string.Format("{0}`{1}", x.ToString(), y.ToString());
            return hash.GetHashCode();
        }

        /// <summary>
        /// Print this pair out in a readable format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0}, {1})", this.x.ToString(), this.y.ToString());
        }
    }
}
