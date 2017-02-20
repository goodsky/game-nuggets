using System;
using System.Collections.Generic;
using System.Timers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot.GameObjects
{
    class GameObject
    {
        Dictionary<String, Sprite> sprites;

        // Sprite and Frame
        Sprite currentSprite = null;
        int frame;
        bool visible = true;

        // Parameters about position
        public double x, y;
        public double width, height;

        // Parameters about animation
        private bool animated = false;
        private double speed = 1000; // seconds between frame changes
        Timer animTimer;

        // Collision parameters
        // COMING SOON!

        // Constructor
        public GameObject(double x, double y, double width, double height)
        {
            sprites = new Dictionary<String, Sprite>();
            animTimer = new Timer();
            animTimer.Elapsed += new ElapsedEventHandler(TimerTick);

            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }


        // Two methods for adding sprites to the game object
        // Each sprite is hashed with a string
        public void AddSprite(String spriteName, Texture2D sprite, int strip, int frameWidth, int frameHeight)
        {
            AddSprite(spriteName, sprite, strip, frameWidth, frameHeight, Color.White);
        }

        void AddSprite(String spriteName, Texture2D sprite, int strip, int frameWidth, int frameHeight, Color transparency)
        {
            sprites.Add(spriteName, new Sprite(sprite, strip, frameWidth, frameHeight, transparency));

            if (sprites.Count == 1)
                sprites.TryGetValue(spriteName, out currentSprite);
        }

        // Set the current sprite for the game object
        public void SetSprite(String spriteName)
        {
            if (sprites.ContainsKey(spriteName))
                sprites.TryGetValue(spriteName, out currentSprite);
        }

        // Set the current frame
        public void SetFrame(int frame)
        {
            this.frame = frame;
        }

        // Set the animation speed
        public void SetAnimationDelay(double delay_ms)
        {
            this.speed = delay_ms;
            animTimer.Interval = delay_ms;
        }

        // Enable or Disable Animation
        public void SetAnimation(bool animated)
        {
            this.animated = animated;

            if (animated)
                animTimer.Start();
            else
                animTimer.Stop();
        }

        // The Tick Event called by the animTimer
        public void TimerTick(object source, ElapsedEventArgs e)
        {
            if (currentSprite == null || currentSprite.frameCount == 0)
                return;

            ++frame;
            if (frame >= currentSprite.frameCount)
                frame %= currentSprite.frameCount;
        }

        // Check for collisions with another object
        public bool Overlap(GameObject o)
        {
            return (x + width >= o.x && x <= o.x + o.width && y + height >= o.y && y <= o.y + o.width);
        }

        // Update and Draw functions for the base GameObject
        public virtual void Update(GameTime gameTime)
        {
            
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (currentSprite != null && visible)
            {
                currentSprite.Draw(spriteBatch, new Rectangle((int)x, (int)y, currentSprite.Width, currentSprite.Height), frame);
            }
        }
    }
}
