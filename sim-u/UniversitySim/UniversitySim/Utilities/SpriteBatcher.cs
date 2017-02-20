using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim.Utilities
{
    /// <summary>
    /// In my dreams this class will be able to create a sprite batch to a side, non-rendered buffer.
    /// Then these separate buffers can be blitted onto the main buffer.
    /// This will allow me to do fancier things with rendering sometimes.
    /// *fingers crossed*
    /// UPDATE: yay
    /// </summary>
    class SpriteBatcher
    {
        // custom spritebatch
        private SpriteBatch customSpriteBatch;

        // custom rendertarget
        private RenderTarget2D customRenderTarget;

        // width of the buffer to draw to
        private int width;

        // height of the buffer to draw to
        private int height;

        /// <summary>
        /// Construct a new sprite batcher.
        /// </summary>
        /// <param name="width">Width of the custom Texture2D</param>
        /// <param name="height">Height of the custom Texture2D</param>
        public SpriteBatcher(int width, int height)
        {
            this.width = width;
            this.height = height;

            this.customRenderTarget = new RenderTarget2D(Game.Graphics.GraphicsDevice, width, height);
            this.customSpriteBatch = new SpriteBatch(Game.Graphics.GraphicsDevice);
        }

        /// <summary>
        /// Set the graphics device to draw to this buffer.
        /// Start the spritebatch. Clear the buffer with the given color.
        /// </summary>
        public void BeginDraw(Color clear)
        {
            Game.Graphics.GraphicsDevice.SetRenderTarget(this.customRenderTarget);
            Game.Graphics.GraphicsDevice.Clear(clear);
            this.customSpriteBatch.Begin();
        }

        /// <summary>
        /// Finish the side buffer drawing. 
        /// Reset the graphics device buffer.
        /// End the spritebatch.
        /// </summary>
        public void EndDraw()
        {
            this.customSpriteBatch.End();
            Game.Graphics.GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Draw to the side buffer. Keep in mind that the destination rectangle is relative to the sub-image. Not screen units any more.
        /// </summary>
        /// <param name="image">Image to draw</param>
        /// <param name="destination">destination rectangle, relative to the sub-image's size</param>
        /// <param name="source">source rectangle from the image</param>
        public void Draw(Texture2D image, Rectangle destination, Rectangle? source = null)
        {
            if (source != null)
            {
                this.customSpriteBatch.Draw(image, destination, (Rectangle)source, Color.White);
            }
            else
            {
                this.customSpriteBatch.Draw(image, destination, Color.White);
            }
        }

        /// <summary>
        /// Draw a string to the buffer. Just like drawing to a normal sprite batch call 
        /// </summary>
        /// <param name="font"></param>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void DrawString(SpriteFont font, string message, Vector2 position, Color color)
        {
            this.customSpriteBatch.DrawString(font, message, position, color);
        }

        /// <summary>
        /// Get the texture from the SpriteBatcher
        /// We clond the data, instead of just returning the renderTarget so that it is not randomly removed from RAM... or GPU RAM.
        /// (I'm not sure why it happens, but without this, randomly the images disapear)
        /// </summary>
        /// <returns>The final texture</returns>
        public Texture2D GetTexture()
        {
            Texture2D clonedImage = new Texture2D(Game.Graphics.GraphicsDevice, width, height);
            Color[] content = new Color[width * height];

            ((Texture2D)this.customRenderTarget).GetData<Color>(content);
            clonedImage.SetData<Color>(content);

            return clonedImage;
        }
    }
}
