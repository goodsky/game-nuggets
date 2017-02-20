using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot
{
    public class Input
    {
        private static KeyboardState lastKeyboardState = Keyboard.GetState();
        private static KeyboardState currentKeyboardState;
        private static MouseState lastMouseState = Mouse.GetState();
        private static MouseState currentMouseState;

        public static KeyboardState keyboardState { get { return currentKeyboardState; } }
        public static MouseState mouseState { get { return currentMouseState; } }


        public static void BeginUpdate()
        {
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
        }

        public static void EndUpdate()
        {
            lastKeyboardState = currentKeyboardState;
            lastMouseState = currentMouseState;
        }

        public static bool KeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public static bool KeyClicked(Keys key)
        {
            return !lastKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }

        public static bool MouseLeftKeyDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool MouseRightKeyDown()
        {
            return currentMouseState.RightButton == ButtonState.Pressed;
        }

        public static bool MouseLeftKeyClicked()
        {
            return lastMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool MouseRightKeyClicked()
        {
            return lastMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
        }

        public static int MouseX()
        {
            return currentMouseState.X;
        }

        public static int MouseY()
        {
            return currentMouseState.Y;
        }
    }
}
