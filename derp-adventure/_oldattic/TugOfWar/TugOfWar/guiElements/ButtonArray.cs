using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TugOfWar.guiElements
{
    class ButtonArray
    {
        public Boolean Active = false;

        private int x;
        private int y;

        private int MouseBoundary = 100;
        
        ImageButton[] buttons;

        public ButtonArray(Texture2D[] imgs, int x, int y)
        {
            this.x = x;
            this.y = y;

            ImageButton[] temp = new ImageButton[imgs.Length];
            for (int i = 0; i < imgs.Length; ++i)
                temp[i] = new ImageButton(imgs[i], x, y - 50 - imgs[0].Height*i, 50, 50);

            this.buttons = temp;
        }

        public bool Update(bool top)
        {
            if (!Active) return false;

            if (Game.input.MouseX() < x - MouseBoundary || Game.input.MouseX() > x + buttons[0].Width + MouseBoundary ||
                Game.input.MouseY() > y + MouseBoundary || Game.input.MouseY() < y - buttons[0].Height * buttons.Length - MouseBoundary)
                Active = false;

            bool mouseover = false;
            for (int i = 0; i < buttons.Length; ++i)
            {
                bool isover = buttons[i].Update(top);
                mouseover = mouseover || isover;
            }

            return mouseover;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Active) return;

            for (int i = 0; i < buttons.Length; ++i)
                buttons[i].Draw(spriteBatch);
        }
    }
}
