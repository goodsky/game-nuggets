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
    // This singletone Input class will be where all input calls are handled.
    // Also, the current state of the mouse (if it is clear of buttons, etc) will be handled here
    // You can check this class to see if we are currently constructing a DNA (therefore requiring a 'ghost dna')
    public class Input
    {
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;
        MouseState lastMouseState;
        MouseState currentMouseState;

        // Button Clear- no buttons underneath the mouse
        public Boolean mouseClear = true;

        // Construction State- If we are building something
        // Confusing integer state (I know, it's bad convention, but boo to you)
        // 0 = no building
        // 1-9 = building DNA index 0 - 8
        // -1 = removing dna
        public int constructionState = 0;

        // The Keyboard and MouseState for various queries
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

            // After the gui updates, this will tell us if there are any buttons under the cursor
            mouseClear = true;
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
