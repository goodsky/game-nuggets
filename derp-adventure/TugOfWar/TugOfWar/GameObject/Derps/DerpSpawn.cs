using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TugOfWar.GameObject.Derps
{
    // This potentially unnecessary class will be what is animated across the DNA during the spawning phase.
    // Once it hits the end of the strand it will turn into a Derp.
    // The Spawner class will keep these in line
    class DerpSpawn
    {
        public static int width = 5, height = 48;
        private static Texture2D spawnTexture;
        public static void InitializeTexture(Texture2D texture)
        {
            DerpSpawn.spawnTexture = texture;
        }

        // Position of the spawn to draw
        public int x, y;

        public DerpSpawn(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Tell it to take a step (either forward or backwards)
        public void Step(int speed)
        {
            x += speed;
        }

        // 
        public void Draw(SpriteBatch spriteBatch, Rectangle camera)
        {
            if (x + width >= camera.X && x <= camera.X + camera.Width)
            {
                int frameNumber = (x%50)/2;
                spriteBatch.Draw(spawnTexture, new Rectangle(x - camera.X, y, width, height), new Rectangle(frameNumber*5, 0, width, height),  Color.White);
            }
        }
    }
}
