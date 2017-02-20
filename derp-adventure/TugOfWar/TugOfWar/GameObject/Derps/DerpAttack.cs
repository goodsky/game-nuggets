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
    public class DerpAttack
    {
        // Store the textures that will be used here
        private static Dictionary<String, Texture2D> textures = null;
        private static Dictionary<String, SpriteInfo> textureInfo = null;
        public static void InitializeTextures(Dictionary<String, Texture2D> textures, Dictionary<String, SpriteInfo> textureInfo)
        {
            DerpAttack.textures = textures;
            DerpAttack.textureInfo = textureInfo;
        }

        private double x, y;

        private Texture2D graphic;
        private SpriteInfo graphicInfo;
        int dir;
        int stepID = 0;

        // Animation and Timing
        DateTime lastTick;
        private long perStep = 50; // milliseconds per step
        public bool alive = true;

        // SortKey
        public SortKey key;

        public DerpAttack(double x, double y, int dir)
        {
            this.x = x;
            this.y = y;
            this.dir = dir;

            key = new SortKey(y);

            graphic = textures["small-close"];
            graphicInfo = textureInfo["small-close"];

            lastTick = DateTime.Now;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle camera)
        {
            // Animate
            if (DateTime.Now.Subtract(lastTick).TotalMilliseconds > perStep)
            {
                if (stepID + 1 < graphicInfo.frames)
                {
                    ++stepID;
                    lastTick = DateTime.Now;
                }
                else
                {
                    alive = false;
                }
            }

            spriteBatch.Draw(graphic, new Rectangle((int)(x - camera.X - graphicInfo.offsetX), (int)(y - camera.Y - graphicInfo.offsetY), graphicInfo.width, graphicInfo.height),
                                          new Rectangle(stepID * graphicInfo.width, dir * graphicInfo.height, graphicInfo.width, graphicInfo.height), Color.White);
        }
    }
}
