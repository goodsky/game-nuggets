using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.GUI.ScrollBars;
using UniversitySim.ScreenElements;
using UniversitySim.Screens;
using UniversitySim.Utilities;

namespace UniversitySim.GUI
{
    /// <summary>
    /// Button that scrolls in the build tab. Click on it to build buildings.
    /// This button is made up of several parts.
    /// The title on top, the image on the left, then price and specs on right.
    /// </summary>
    class SideFolderButton : ScrollElement
    {
        // The font for all button titles
        internal static SpriteFont titleFont = null;

        // The font for all the details on the buttons
        internal static SpriteFont buttonFont = null;

        // Data for the building we are a button-gateway to
        private Data data;

        // Components of the button image drawing
        private int cardImageWidth = 60;
        private int cardImageHeight = 60;
        private Texture2D buildingBack;
        private List<string> specsString;

        // General purpose bit
        private Texture2D whitePixel;

        // Bit signaling if the image needs to be re-rendered
        private bool dirtyImageBit;

        // The rendered textures that will be displated to the screen
        private Texture2D image;
        private Texture2D imageHighlight;

        /// <summary>
        /// Create a build button. All this information should be stored in a configuration file for easy testing.
        /// The specs is a comma separated list of specs. Up to 4 can be displayed... any more and we will have to do a quick redesign.
        /// </summary>
        /// <param name="data">The data this side-button is based on</param>
        /// <param name="parent">The parent scroll bar</param>
        /// <param name="depth">Depth of the button</param>
        public SideFolderButton(Data data, Pair<int> size, ScrollBar parent, int depth) : base(size, parent, depth)
        {
            this.parent = parent;
            this.data = data;

            var specsStringList = new List<string>();

            BuildingData buildingData = data as BuildingData;
            if (buildingData != null)
            {
                foreach (var pair in buildingData.Specs)
                {
                    string key = pair.Key.ToString();
                    int value = pair.Value;
                    specsStringList.Add(string.Format("{0} {1}{2}", key, (value < 0 ? "" : "+"), value));
                }
            }

            this.specsString = specsStringList;
            this.dirtyImageBit = true;
        }

        /// <summary>
        /// Load the content for this button. Load the static fonts if they aren't loaded already.
        /// </summary>
        /// <param name="contentMan"></param>
        public override void LoadContent(ContentManager contentMan)
        {
            if (titleFont == null)
            {
                titleFont = contentMan.Load<SpriteFont>(@"Fonts\GUI_MEDIUM");
            }

            if (buttonFont == null)
            {
                buttonFont = contentMan.Load<SpriteFont>(@"Fonts\GUI_SMALL");
            }

            this.buildingBack = contentMan.Load<Texture2D>(@"GUI\BuildButtonImageBack");
            this.whitePixel = contentMan.Load<Texture2D>(@"WhitePixel");
        }

        /// <summary>
        /// Update this folder button.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (this.dirtyImageBit)
            {
                this.ReDrawImage();
            }
        }

        /// <summary>
        /// When clicked, we should start the ghosting construction process
        /// </summary>
        /// <param name="mousePosition"></param>
        public override void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            base.Clicked(mousePosition, clickType);

            if (clickType == LeftOrRight.Left)
            {
                GameScreen.BeginBuilding(this.data.Key);
            }
        }

        /// <summary>
        /// Draw this folder button.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (this.IsMouseOver)
            {
                this.DrawScrollElement(this.imageHighlight, spriteBatch);
            }
            else
            {
                this.DrawScrollElement(this.image, spriteBatch);
            }
        }

        /// <summary>
        /// Redraw the scroll button's image
        /// </summary>
        private void ReDrawImage()
        {
            // ***
            // Draw the non-highlighted version
            // ***
            int width = this.Size.x - 5;
            int height = this.Size.y - 5;
            SpriteBatcher batcher = new SpriteBatcher(width, height);

            batcher.BeginDraw(Color.Transparent);

            // Draw your card
            // build picture background
            batcher.Draw(this.buildingBack, new Rectangle(0, 15, cardImageWidth, 3), new Rectangle(1, 0, 2, 3));
            batcher.Draw(this.buildingBack, new Rectangle(0, 15, 3, cardImageHeight), new Rectangle(1, 0, 3, 2));
            batcher.Draw(this.buildingBack, new Rectangle(3, 18, cardImageWidth - 3, cardImageHeight - 3), new Rectangle(2, 2, 1, 1));

            // build picture
            double scale = Math.Min(this.cardImageWidth / (double)this.data.IconImage.Width, this.cardImageHeight / (double)this.data.IconImage.Height);
            int offsetX = (this.cardImageWidth - (int)(this.data.IconImage.Width * scale)) / 2;
            int offsetY = (this.cardImageHeight - (int)(this.data.IconImage.Height * scale)) / 2;
            batcher.Draw(this.data.IconImage, new Rectangle(offsetX, 15 + offsetY, (int)(this.data.IconImage.Width * scale), (int)(this.data.IconImage.Height * scale)));

            // text
            int textY = 15;
            batcher.DrawString(titleFont, this.data.Name, new Vector2(1, 0), Color.Black);
            batcher.DrawString(buttonFont, "$" + this.data.Cost.ToString(), new Vector2(70, textY), Color.Green);

            foreach (var spec in this.specsString)
            {
                textY += 13;
                batcher.DrawString(buttonFont, spec, new Vector2(70, textY), Color.Black);
            }

            batcher.EndDraw();
            this.image = batcher.GetTexture();

            // ***
            // Draw the highlighted version
            // This is a lot of repeated work to make a second copy of the image... but it's the easiest solution I can think of now
            // (and I've spent too much time thinking about this already) 
            // ***
            batcher = new SpriteBatcher(width, height);

            batcher.BeginDraw(Color.Transparent);

            // Draw your card
            // build picture background
            batcher.Draw(this.buildingBack, new Rectangle(0, 15, cardImageWidth, 3), new Rectangle(1, 0, 2, 3));
            batcher.Draw(this.buildingBack, new Rectangle(0, 15, 3, cardImageHeight), new Rectangle(1, 0, 3, 2));
            batcher.Draw(this.buildingBack, new Rectangle(3, 18, cardImageWidth - 3, cardImageHeight - 3), new Rectangle(2, 2, 1, 1));

            // build picture
            scale = Math.Min(this.cardImageWidth / (double)this.data.IconImage.Width, this.cardImageHeight / (double)this.data.IconImage.Height);
            offsetX = (this.cardImageWidth - (int)(this.data.IconImage.Width * scale)) / 2;
            offsetY = (this.cardImageHeight - (int)(this.data.IconImage.Height * scale)) / 2;
            batcher.Draw(this.data.IconImage, new Rectangle(offsetX, 15 + offsetY, (int)(this.data.IconImage.Width * scale), (int)(this.data.IconImage.Height * scale)));

            // text
            textY = 15;
            batcher.DrawString(titleFont, this.data.Name, new Vector2(1, 0), Color.Black);
            batcher.DrawString(buttonFont, "$" + this.data.Cost.ToString(), new Vector2(70, textY), Color.Green);

            foreach (var spec in this.specsString)
            {
                textY += 13;
                batcher.DrawString(buttonFont, spec, new Vector2(70, textY), Color.Black);
            }

            // draw the highlight
            int line_width = 2;
            batcher.Draw(this.whitePixel, new Rectangle(0, 0, line_width, height));
            batcher.Draw(this.whitePixel, new Rectangle(0, 0, width, line_width));
            batcher.Draw(this.whitePixel, new Rectangle(width - line_width, 0, line_width, height));
            batcher.Draw(this.whitePixel, new Rectangle(0, height - line_width, width, line_width));

            batcher.EndDraw();
            this.imageHighlight = batcher.GetTexture();

            this.dirtyImageBit = false;
        }
    }
}
