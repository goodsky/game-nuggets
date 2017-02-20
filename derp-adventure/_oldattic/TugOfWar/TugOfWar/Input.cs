using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TugOfWar
{
    public class Input
    {
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;
        MouseState lastMouseState;
        MouseState currentMouseState;

        public KeyboardState keyboardState { get { return currentKeyboardState; } }
        public MouseState mouseState { get { return currentMouseState; } }

        public Input()
        {
            lastKeyboardState = Keyboard.GetState();
            lastMouseState = Mouse.GetState();
        }

        public void BeginUpdate()
        {
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
        }

        public void EndUpdate()
        {
            lastKeyboardState = currentKeyboardState;
            lastMouseState = currentMouseState;
        }

        public bool KeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public bool KeyClicked(Keys key)
        {
            return !lastKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }

        public bool MouseLeftKeyDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool MouseRightKeyDown()
        {
            return currentMouseState.RightButton == ButtonState.Pressed;
        }

        public bool MouseLeftKeyClicked()
        {
            return lastMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool MouseRightKeyClicked()
        {
            return lastMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
        }

        public int MouseX()
        {
            return currentMouseState.X;
        }

        public int MouseY()
        {
            return currentMouseState.Y;
        }
    }
}
