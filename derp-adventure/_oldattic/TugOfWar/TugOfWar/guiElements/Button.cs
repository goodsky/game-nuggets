using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TugOfWar.GameObject;

namespace TugOfWar.guiElements
{
    class Button
    {
        public int Width;
        public int Height;
        public int X;
        public int Y;

        public bool Active = true;
        public bool MouseOver = false;

        public delegate void OnMouseDown();
        public OnMouseDown MouseEvent { get; set; }

        public String info = null;

        public Button(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            MouseEvent = null;
        }

        public virtual bool Update(bool top)
        {
            bool oldmouseover = MouseOver;

            if (Game.input.MouseX() >= X && Game.input.MouseX() <= X + Width &&
                Game.input.MouseY() >= Y && Game.input.MouseY() <= Y + Height)
                MouseOver = true;
            else
                MouseOver = false;
            if (!top) MouseOver = false;

            if (!oldmouseover && MouseOver)
                GUI.infoText = info;

            if (top && MouseOver && Game.input.MouseLeftKeyClicked())
                if (MouseEvent != null) MouseEvent.Invoke();

            return MouseOver;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        
        }
    }
}
