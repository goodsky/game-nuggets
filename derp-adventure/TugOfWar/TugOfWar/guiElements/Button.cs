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

        public delegate void OnMouseDown(Button clicked);
        public OnMouseDown MouseEvent { get; set; }

        public String title = null;
        public String info = null;

        // These general purpose arguments are used when a button is clicked and the method needs an extra argument.
        // For example, the button that builds the ATK DNA will have ButtonArgs1 = 0, and the DEF DNA button will have ButonARgs1 = 1, etc
        public int ButtonArgs1;

        public Button(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            MouseEvent = null;
        }

        // Set the button's tool tips that are set on the Info Box
        public void SetInfo(String title, String info)
        {
            this.title = title;
            this.info = info;
        }
        public void SetInfo(String title, String info, int arg)
        {
            this.title = title;
            this.info = info;
            ButtonArgs1 = arg;
        }

        // Buttony Updating
        public virtual bool Update(bool top)
        {
            // mouse over
            if (Game.input.MouseX() >= X && Game.input.MouseX() <= X + Width && Game.input.MouseY() >= Y && Game.input.MouseY() <= Y + Height)
            {
                // If we just moused over, then set the GUI Info Box
                if (!MouseOver)
                    GUI.ShowInfo(title, info);

                MouseOver = true;
                Game.input.mouseClear = false;
            }
            else
            {
                MouseOver = false;
            }

            if (!top) MouseOver = false;

            if (top && MouseOver && Game.input.MouseLeftKeyClicked())
                if (MouseEvent != null) MouseEvent.Invoke(this);

            return MouseOver;
        }

        // Buttony Drawing (will be overriden in actual button implementations)
        public virtual void Draw(SpriteBatch spriteBatch)
        {
        
        }
    }
}
