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
        Texture2D BackImage = null;
        Texture2D TopImage = null;
        int mode = 0;

        public ImageButton(Texture2D backImage, Texture2D topImage, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            BackImage = backImage;
            TopImage = topImage;
        }

        public ImageButton(Texture2D image, int x, int y, int width, int height) : base(x, y, width, height)
        {
            BackImage = image;
        }

        // Mode is which set of images we want to use
        // 0 = first 2
        // 1 = second 2
        // 2 = third 2, etc
        public void setMode(int i)
        {
            mode = i;
        }

        public override bool Update(bool top)
        {
            return base.Update(top);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect;

            if (MouseOver)
                sourceRect = new Rectangle(Width * (mode*2 + 1), 0, Width, Height);
            else
                sourceRect = new Rectangle(Width * (mode*2), 0, Width, Height);

            spriteBatch.Draw(BackImage, new Rectangle(X, Y, Width, Height), sourceRect, Color.White);

            if (TopImage != null)
            {
                sourceRect.X = Width * (mode);
                spriteBatch.Draw(TopImage, new Rectangle(X, Y, Width, Height), sourceRect, Color.White);
            }
        }
    }
}
