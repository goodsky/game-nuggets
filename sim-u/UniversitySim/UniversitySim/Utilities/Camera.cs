using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim.Utilities
{
    /// <summary>
    /// Camera class used to keep track of camera state.
    /// </summary>
    public class Camera
    {
        // X and Y position of the top left of the camera
        public int x { get; private set; }
        public int y { get; private set; }

        // Camera size
        public int width { get; private set; }
        public int height { get; private set; }

        // The camera has to be aware of its world
        private int worldWidth;
        private int worldHeight;

        // Singleton object
        public static Camera Instance 
        { 
            get
            {
                return camera;
            }
        }

        private static Camera camera = null;

        // Set up the singleton camera
        public static void Setup(int startX, int startY, int worldWidth, int worldHeight)
        {
            camera = new Camera(startX, startY, worldWidth, worldHeight);
        }

        /// <summary>
        /// Create a new instace of the camera
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="worldWidth"></param>
        /// <param name="worldHeight"></param>
        private Camera(int startX, int startY, int worldWidth, int worldHeight)
        {
            this.x = startX;
            this.y = startY;
            this.width = Constants.WINDOW_WIDTH;
            this.height = Constants.WINDOW_HEIGHT;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
        }

        /// <summary>
        /// Move the camera around
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void moveCamera(int dx, int dy)
        {
            x += dx;
            y += dy;

            // Cap camera movement with the world size
            if (x + width > worldWidth) 
                x = worldWidth - width;

            if (x < 0) 
                x = 0;

            if (y + height > worldHeight) 
                y = worldHeight - height;

            if (y < 0) 
                y = 0;
        }
    }
}
