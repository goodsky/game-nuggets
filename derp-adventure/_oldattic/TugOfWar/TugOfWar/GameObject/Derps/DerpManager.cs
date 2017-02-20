using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// The DerpManager will have a collection of Derps that it draws out each step
// This will manage finding derps at positions and whatnot

namespace TugOfWar.GameObject
{

    class DerpManager
    {
        public List<Derp> team1 = new List<Derp>();
        public List<Derp> team2 = new List<Derp>();

        public void SpawnDerp(int xPos, int yPos, int team)
        {
            if (team == 1)
            {
                team1.Add(new Derp(xPos, yPos, 1));
            }
            else
            {
                team2.Add(new Derp(xPos, yPos, 2));
            }
        }

        public void Update(GameTime gameTime)
        {
            // Update all Derps
            foreach (Derp d in team1)
            {
                d.Update();
            }

            foreach (Derp d in team2)
            {
                d.Update();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle rect)
        {
            // Draw all Derps
            foreach (Derp d in team1)
            {
                if (((d.x * Field.BLOCK_WIDTH) + 50) > rect.X && ((d.x * Field.BLOCK_WIDTH) - 50) < rect.X + rect.Width)
                    d.Draw(spriteBatch, rect);
            }

            foreach (Derp d in team2)
            {
                if (((d.x * Field.BLOCK_WIDTH) + 50) > rect.X && ((d.x * Field.BLOCK_WIDTH) - 50) < rect.X + rect.Width)
                    d.Draw(spriteBatch, rect);
            }
        }
    }
}
