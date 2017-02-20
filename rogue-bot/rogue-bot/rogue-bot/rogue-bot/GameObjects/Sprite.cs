using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot.GameObjects
{
    class Sprite
    {
        // Sprite Attributes
        Texture2D sprite = null;
        Color transparency;
        int frameWidth = 50, frameHeight = 50;
        int strip = 0;

        public int Width { get { return frameWidth; } }
        public int Height { get { return frameHeight; } }

        public int frameCount { get { return (sprite == null ? 0 : sprite.Width / frameWidth); } }

        public Sprite(Texture2D image, int strip, int frameWidth, int frameHeight)
            : this(image, strip, frameWidth, frameHeight, Color.White) { }

        public Sprite(Texture2D image, int strip, int frameWidth, int frameHeight, Color transparency)
        {
            this.sprite = image;
            this.strip = strip;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.transparency = transparency;
        }

        // Draw the sprite based on its current frame and strip (strip is vertical, frame is horizontal)
        // This will automatically split up the sprite for you.
        public void Draw(SpriteBatch spriteBatch, Rectangle destination, int frame)
        {
            spriteBatch.Draw(sprite, destination, new Rectangle(frame * frameWidth, strip * frameHeight, frameWidth, frameHeight), transparency);
        }
    }
}
