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
    class TextButton : Button
    {
        SpriteFont font;
        String text;
        Color c1;
        Color c2;

        public TextButton(SpriteFont font, String text, Color c1, Color c2, int x, int y, int width, int height) : base(x, y, width, height)
        {
            this.font = font;
            this.text = text;
            this.c1 = c1;
            this.c2 = c2;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (font == null) return;

            if (MouseOver)
                spriteBatch.DrawString(font, text, new Vector2(X, Y), c2);
            else
                spriteBatch.DrawString(font, text, new Vector2(X, Y), c1);
        }
    }
}
