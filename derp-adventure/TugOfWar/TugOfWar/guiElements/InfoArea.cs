using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// This is a bit confusing, but basically I decided that only Buttons and tiles will update the Info Text
// So this is for a non-button area that might need an info update

namespace TugOfWar.guiElements
{
    class InfoArea : Button
    {
        public InfoArea(int x, int y, int width, int height) : base(x, y, width, height) { }

        public override bool Update(bool top)
        {
            return base.Update(top);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // do nothing
        }
    }
}
