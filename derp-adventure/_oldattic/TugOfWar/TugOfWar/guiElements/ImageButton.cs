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
    class ImageButton : Button
    {
        Texture2D Image;

        public ImageButton(Texture2D image, int x, int y, int width, int height) : base(x, y, width, height)
        {
            Image = image;
        }

        public override bool Update(bool top)
        {
            return base.Update(top);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect;
            if (MouseOver)
                sourceRect = new Rectangle(Width, 0, Width, Height);
            else
                sourceRect = new Rectangle(0, 0, Width, Height);

            if (!Active)
                sourceRect = new Rectangle(2*Width, 0, Width, Height);

            spriteBatch.Draw(Image, new Rectangle(X, Y, Width, Height), sourceRect, Color.White);
        }
    }
}
