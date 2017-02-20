using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.GUI.ScrollBars
{
    /// <summary>
    /// The element inside of a scroll bar
    /// </summary>
    abstract class ScrollElement : ScreenElement
    {
        // The scroll bar that this element is a part of.
        // Each element can only be in a single scroll bar.
        protected ScrollBar parent;

        public ScrollElement(Pair<int> size, ScrollBar parent, int depth) : base(depth)
        {
            this.Position = new Pair<int>(1337, 1337); // this will be overwritten by the ScrollBar once it's added to the list.
            this.Size = size;
            this.parent = parent;
            this.Clickable = true;
            this.Type = ElementType.GUI;
        }

        /// <summary>
        /// Use this tool from any Scroll Element to make sure that you draw your texture only within the valid area. 
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="spriteBatch">The current sprite Batch</param>
        protected void DrawScrollElement(Texture2D image, SpriteBatch spriteBatch)
        {
            // check if we have to draw anything
            if (this.Position.y + image.Height < this.parent.Position.y || this.Position.y > this.parent.Position.y + this.parent.Size.y)
            {
                return;
            }

            Rectangle srcRectangle = new Rectangle(0, 0, image.Width, image.Height);
            Rectangle destRectangle = new Rectangle(this.Position.x, this.Position.y, image.Width, image.Height);
            
            if (destRectangle.Y < this.parent.Position.y)
            {
                int overlap = this.parent.Position.y - destRectangle.Y;
                srcRectangle.Y += overlap;
                srcRectangle.Height -= overlap;
                destRectangle.Y += overlap;
                destRectangle.Height -= overlap;
            }

            if (destRectangle.Y + destRectangle.Height > this.parent.Position.y + this.parent.Size.y)
            {
                int overlap = destRectangle.Y + destRectangle.Height - (this.parent.Position.y + this.parent.Size.y);
                srcRectangle.Height -= overlap;
                destRectangle.Height -= overlap;
            }

            spriteBatch.Draw(image, destRectangle, srcRectangle, Color.White);
        }
    }
}
