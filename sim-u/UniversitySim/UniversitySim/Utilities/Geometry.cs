using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.ScreenElements;

namespace UniversitySim.Utilities
{
    /// <summary>
    /// This class holds generic geometry methods.
    /// Add any generic sort of operation here.
    /// </summary>
    class Geometry
    {
        /// <summary>
        /// Check if a point is within a given rectangle
        /// </summary>
        /// <param name="pos">Position in world coordinates</param>
        /// <param name="rect">Rectangle (in world coordinates)</param>
        /// <returns>True if the point is in the given rectangle</returns>
        public static bool IsInRectangle(Pair<int> pos, Rectangle rect)
        {
            return (pos.x >= rect.X &&
                    pos.x <= rect.X + rect.Width &&
                    pos.y >= rect.Y &&
                    pos.y <= rect.Y + rect.Height);
        }

        /// <summary>
        /// Check if a point is within a given rectangle.
        /// Use an option offset term to convert between world and screen coordinates
        /// </summary>
        /// <param name="pos">Position (in either world or screen coordinates)</param>
        /// <param name="offset">Offset used to convert between coordinates</param>
        /// <param name="rect">Rectangle</param>
        /// <returns>True if the point is in the given rectangle</returns>
        public static bool IsInRectangle(Pair<int> pos, Pair<int> offset, Rectangle rect)
        {
            return (pos.x >= offset.x + rect.X &&
                    pos.x <= offset.x + rect.X + rect.Width &&
                    pos.y >= offset.y + rect.Y &&
                    pos.y <= offset.y + rect.Y + rect.Height);
        }

        /// <summary>
        /// Check if a point is within a given rectangle.
        /// </summary>
        /// <param name="pos">Position (in either world or screen coordinates)</param>
        /// <param name="x">Rectangle X position</param>
        /// <param name="y">Rectangle Y position</param>
        /// <param name="width">Rectangle width</param>
        /// <param name="height">Rectangle height</param>
        /// <returns>True if the point is in the given rectangle</returns>
        public static bool IsInRectangle(Pair<int> pos, int x, int y, int width, int height)
        {
            return (pos.x >= x &&
                    pos.x <= x + width &&
                    pos.y >= y &&
                    pos.y <= y + height);
        }

        /// <summary>
        /// Check if a point is within a given rectangle.
        /// Use an option offset term to convert between world and screen coordinates
        /// </summary>
        /// <param name="pos">Position (in either world or screen coordinates)</param>
        /// <param name="offset">Offset used to convert between coordinates</param>
        /// <param name="x">Rectangle X position</param>
        /// <param name="y">Rectangle Y position</param>
        /// <param name="width">Rectangle width</param>
        /// <param name="height">Rectangle height</param>
        /// <returns>True if the point is in the given rectangle</returns>
        public static bool IsInRectangle(Pair<int> pos, Pair<int> offset, int x, int y, int width, int height)
        {
            return (pos.x >= offset.x + x &&
                    pos.x <= offset.x + x + width &&
                    pos.y >= offset.y + y &&
                    pos.y <= offset.y + y + height);
        }

        /// <summary>
        /// Use this utility to tell if a Screen Element is currently within the screen.
        /// This is used to tell if we should draw the element or not during the Draw step.
        /// </summary>
        /// <param name="element">The Screen Element</param>
        /// <returns>True if the element is in the screen</returns>
        public static bool IsOnScreen(ScreenElement element)
        {
            return !(element.Position.x > Camera.Instance.x + Camera.Instance.width ||
                     element.Position.x + element.Size.x < Camera.Instance.x ||
                     element.Position.y > Camera.Instance.y + Camera.Instance.height ||
                     element.Position.y + element.Size.y < Camera.Instance.y);
        }

        /// <summary>
        /// Calculate the distance between two point
        /// </summary>
        /// <param name="p1">Point1</param>
        /// <param name="p2">Point2</param>
        /// <returns></returns>
        public static double Dist(Pair<int> p1, Pair<int> p2)
        {
            return Math.Sqrt((p2.x - p1.x)*(p2.x - p1.x) + (p2.y - p1.y)*(p2.y - p1.y));
        }

        /// <summary>
        /// Convert the regular coordinates to isometric grid coordinates. Assumes 0,0 is the top left.
        /// </summary>
        /// <param name="coordinates">Regular coordinates</param>
        /// <returns>The isometric grid location</returns>
        public static Pair<int> ToIsometricGrid(Pair<int> coordinates)
        {
            int m = (int)Math.Round(((double)coordinates.y / Constants.TILE_HEIGHT) - ((double)coordinates.x / Constants.TILE_WIDTH));
            int n = (int)Math.Round(((double)coordinates.y / Constants.TILE_HEIGHT) + ((double)coordinates.x / Constants.TILE_WIDTH)) - 1;

            return new Pair<int>(m, n);
        }

        /// <summary>
        /// Given the position in the isometric grid, turn that position into a world position
        /// </summary>
        /// <param name="isoCoordinates"></param>
        /// <returns></returns>
        public static Pair<int> ToWorldGrid(Pair<int> isoCoordinates)
        {
            return new Pair<int>((int)((isoCoordinates.x - isoCoordinates.y) / 2.0 * Constants.TILE_WIDTH), (int)((isoCoordinates.x + isoCoordinates.y) / 2.0 * Constants.TILE_HEIGHT));
        }
    }
}
