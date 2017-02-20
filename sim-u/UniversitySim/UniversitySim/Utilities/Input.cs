using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace UniversitySim.Utilities
{
    /// <summary>
    ///  Static input class to handle all input calls
    /// </summary>
    public class Input
    {
        private static KeyboardState lastKeyboardState = Keyboard.GetState();
        private static KeyboardState currentKeyboardState = Keyboard.GetState();
        private static MouseState lastMouseState = Mouse.GetState();
        private static MouseState currentMouseState = Mouse.GetState();

        // Mouse position in screen units
        private static Pair<int> mousePositionScreen = new Pair<int>(0, 0);

        // Mouse position in world units
        private static Pair<int> mousePositionWorld = new Pair<int>(0, 0);

        // Mouse position in world isometric grid units
        private static Pair<int> isoMouse = new Pair<int>(0, 0);

        // Mouse position snapped to the isometric grid in world units
        private static Pair<int> isoMouseGrid = new Pair<int>(0, 0);

        /// <summary>
        /// Get the current Keyboard state
        /// </summary>
        public static KeyboardState CurrentKeyboardState { get { return currentKeyboardState; } }

        /// <summary>
        /// Get the current Mouse state
        /// </summary>
        public static MouseState CurrentMouseState { get { return currentMouseState; } }

        /// <summary>
        /// Get the mouse Position in SCREEN UNITS
        /// </summary>
        public static Pair<int> MousePosition { get { return mousePositionScreen; } }

        /// <summary>
        /// Get the mouse X position in SCREEN UNITS
        /// </summary>
        /// <returns></returns>
        public static int MouseX { get { return mousePositionScreen.x; } }

        /// <summary>
        /// Get the mouse Y position in SCREEN UNITS
        /// </summary>
        /// <returns></returns>
        public static int MouseY { get { return mousePositionScreen.y; } }

        /// <summary>
        /// Get the mouse Position in WORLD UNITS
        /// </summary>
        public static Pair<int> MouseWorldPosition { get { return mousePositionWorld; } }

        /// <summary>
        /// Get the mouse X position in WORLD UNITS
        /// </summary>
        /// <returns></returns>
        public static int MouseWorldX { get { return mousePositionWorld.x; } }

        /// <summary>
        /// Get the mouse Y position in WORLD UNITS
        /// </summary>
        /// <returns></returns>
        public static int MouseWorldY { get { return mousePositionWorld.y; } }

        /// <summary>
        /// Gets the Mouse position in ISOMETRIC GRID UNITS
        /// </summary>
        public static Pair<int> IsoGrid { get { return isoMouseGrid;  } }

        /// <summary>
        /// Get the Mouse X position in ISOMETRIC GRID UNITS
        /// </summary>
        public static int IsoGridX { get { return isoMouseGrid.x; } }

        /// <summary>
        /// Get the Mouse Y position in ISOMETRIC GRID UNITS
        /// </summary>
        public static int IsoGridY { get { return isoMouseGrid.y; } }

        /// <summary>
        /// Get the Mouse position in WORLD UNITS snapped to an ISOMETRIC GRID
        /// </summary>
        public static Pair<int> IsoWorld { get { return isoMouse;  } }

        /// <summary>
        /// Get the Mouse X position in WORLD UNITS snapped to an ISOMETRIC GRID 
        /// </summary>
        public static int IsoWorldX { get { return isoMouse.x; } }

        /// <summary>
        /// Get the Mouse Y position in WORLD UNITS snapped to an ISOMETRIC GRID 
        /// </summary>
        public static int IsoWorldY { get { return isoMouse.y; } }

        /// <summary>
        /// BeginUpdate must be called at the beginning of the Update step
        /// </summary>
        public static void BeginUpdate()
        {
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            bool isFullScreen = Game.GameConfig.GetBoolValue("FullScreen", false);
            mousePositionScreen.x = isFullScreen ? (int)(currentMouseState.X / Game.FullScreenRatio) : currentMouseState.X;
            mousePositionScreen.y = isFullScreen ? (int)(currentMouseState.Y / Game.FullScreenRatio) : currentMouseState.Y;

            mousePositionWorld.x = mousePositionScreen.x + Camera.Instance.x;
            mousePositionWorld.y = mousePositionScreen.y + Camera.Instance.y;

            isoMouseGrid.x = (int)Math.Round(((double)mousePositionWorld.y / Constants.TILE_HEIGHT) - ((double)mousePositionWorld.x / Constants.TILE_WIDTH));
            isoMouseGrid.y = (int)Math.Round(((double)mousePositionWorld.y / Constants.TILE_HEIGHT) + ((double)mousePositionWorld.x / Constants.TILE_WIDTH)) - 1; // why do I need this -1? because my (0,0) grid on the top left is actually not at (0,0), it's at (0,1). chew on that isometric math.
            isoMouse.x = (int)((isoMouseGrid.y - isoMouseGrid.x) / 2.0 * Constants.TILE_WIDTH);
            isoMouse.y = (int)((isoMouseGrid.y + isoMouseGrid.x) / 2.0 * Constants.TILE_HEIGHT);
        }

        /// <summary>
        /// EndUpdate must be called at the end of the Update step
        /// </summary>
        public static void EndUpdate()
        {
            lastKeyboardState = currentKeyboardState;
            lastMouseState = currentMouseState;
        }
        
        /// <summary>
        /// Check if a key is currently down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a key has been clicked since last step
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyClicked(Keys key)
        {
            return !lastKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if the left mouse button is currently down
        /// </summary>
        /// <returns></returns>
        public static bool MouseLeftKeyDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Check if the right mouse button is currently down
        /// </summary>
        /// <returns></returns>
        public static bool MouseRightKeyDown()
        {
            return currentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Check if the left mouse button has been clicked since the last update
        /// </summary>
        /// <returns></returns>
        public static bool MouseLeftKeyClicked()
        {
            return lastMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Check if the right mouse button has been clicked since the last update
        /// </summary>
        /// <returns></returns>
        public static bool MouseRightKeyClicked()
        {
            return lastMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Check if the left mouse button has been relelased since the last update
        /// </summary>
        /// <returns></returns>
        public static bool MouseLeftKeyReleased()
        {
            return lastMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Check if the right mouse button has been released since the last update
        /// </summary>
        /// <returns></returns>
        public static bool MouseRightKeyReleased()
        {
            return lastMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Get the amount the mouse scroll wheel has moved since the last update step
        /// </summary>
        /// <returns></returns>
        public static int MouseWheelMove()
        {
            return lastMouseState.ScrollWheelValue - currentMouseState.ScrollWheelValue;
        }
    }
}
