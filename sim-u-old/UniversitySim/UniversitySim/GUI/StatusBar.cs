using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.ScreenElements;

namespace UniversitySim.GUI
{
    /// <summary>
    /// Print out status messages on the bottom status bar
    /// </summary>
    class StatusBar : ScreenElement
    {
        /// <summary>
        /// Status to print out on the bottom of the screen.
        /// </summary>
        private static string status;

        /// <summary>
        /// The 4 x 10 pixel template for drawing the bottom bar.
        /// </summary>
        private Texture2D bar;

        /// <summary>
        /// The source and destination rectangles for the bar image to be draw to the screen
        /// They are the same every step, so I create them once at initialization
        /// </summary>
        private Rectangle leftDestRect;
        private Rectangle midDestRect;
        private Rectangle rightDestRect;

        private Rectangle leftSrcRect;
        private Rectangle midSrcRect;
        private Rectangle rightSrcRect;

        /// <summary>
        /// SpriteFont for writing the status message on the bottom bar
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Allow anyone, anywhere to update the status.
        /// </summary>
        /// <param name="status"></param>
        public static void UpdateStatus(string status)
        {
            StatusBar.status = status;
        }

        /// <summary>
        /// Create a status bar.
        /// </summary>
        public StatusBar() : base(Constants.GUI_DEPTH + 3)
        {
            StatusBar.status = "Welcome to University Simulator 2014";
        }

        /// <summary>
        /// Load any content needed for this element.
        /// </summary>
        /// <param name="contentMan"></param>
        public override void LoadContent(ContentManager contentMan) 
        {
            this.bar = contentMan.Load<Texture2D>(@"GUI/StatusBar");
            this.font = contentMan.Load<SpriteFont>(@"Fonts/GUI_SMALL");

            this.leftDestRect = new Rectangle(0, Constants.WINDOW_HEIGHT - this.bar.Height, 1, this.bar.Height);
            this.midDestRect = new Rectangle(1, Constants.WINDOW_HEIGHT - this.bar.Height, Constants.WINDOW_WIDTH - 2, this.bar.Height);
            this.rightDestRect = new Rectangle(Constants.WINDOW_WIDTH - 1, Constants.WINDOW_HEIGHT - this.bar.Height, 1, this.bar.Height);

            this.leftSrcRect = new Rectangle(0, 0, 1, this.bar.Height);
            this.midSrcRect = new Rectangle(1, 0, 1, this.bar.Height);
            this.rightSrcRect = new Rectangle(2, 0, 1, this.bar.Height);
        }

        /// <summary>
        /// Update this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) { }

        /// <summary>
        /// Draw this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) 
        {
            // Draw the bar
            spriteBatch.Draw(this.bar, this.leftDestRect, this.leftSrcRect, Color.White);
            spriteBatch.Draw(this.bar, this.midDestRect, this.midSrcRect, Color.White);
            spriteBatch.Draw(this.bar, this.rightDestRect, this.rightSrcRect, Color.White);

            // Draw the message
            float widthOfStatus = this.font.MeasureString(StatusBar.status).X;
            spriteBatch.DrawString(this.font, StatusBar.status, new Vector2((float)(Constants.WINDOW_WIDTH - widthOfStatus - 5.0), (float)(Constants.WINDOW_HEIGHT - 13.0)), Color.White);
        }
    }
}
