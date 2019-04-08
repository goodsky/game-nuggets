using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.GUI
{
    /// <summary>
    /// GUI in the top center of the screen.
    /// Display information like your current money, GPA, date, etc
    /// </summary>
    class HUD : ScreenElement
    {
        /// <summary>
        /// The image used to draw the hud background
        /// </summary>
        private Texture2D hudImage;

        /// <summary>
        /// Font used to print things out
        /// </summary>
        private SpriteFont font;

        // The HUD is always drawn at the same location
        private Rectangle destRect;

        /// <summary>
        /// Create the HUD
        /// </summary>
        public HUD() : base(Constants.GUI_DEPTH + 2)
        {
            this.Position = new Pair<int>(224, 0);
        }

        /// <summary>
        /// Load any content needed for this element.
        /// </summary>
        /// <param name="contentMan"></param>
        public override void LoadContent(ContentManager contentMan)
        {
            this.hudImage = contentMan.Load<Texture2D>(@"GUI/HudBase");
            this.font = contentMan.Load<SpriteFont>(@"Fonts/GUI_SMALL");

            this.Size = new Pair<int>(this.hudImage.Width, this.hudImage.Height);
            this.destRect = new Rectangle(this.Position.x, this.Position.y, this.hudImage.Width, this.hudImage.Height);
        }

        /// <summary>
        /// Update the HUD each game step
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) { }

        /// <summary>
        /// Draw the HUD each game step
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.hudImage, this.destRect, Color.White);
        }
    }
}
