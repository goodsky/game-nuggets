using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// The Derp Class will control a single Derp instance including the derp's stats, position, etc

namespace TugOfWar.GameObject.Derps
{
    enum direction { NORTH, EAST, WEST, SOUTH };
    enum derpState { MOVING, COMBAT, DEATH };
    class Derp
    {
        //Info
        public double x, y;
        public direction facing;
        public derpState state;
        public int team;
        public Texture2D graphic;
        public DerpStats stats;

        public Derp(double x, double y, int team)
        {
            ContentManager Content = Game.game.Content;

            this.x = x;
            this.y = y;
            facing = direction.EAST;
            state = derpState.MOVING;
            stats = new DerpStats(10, 1, 1, 1, 0, 0, 0, 0);
            this.team = team;

            if (team == 1)
                graphic = Content.Load<Texture2D>(@"GameBlocks\Derp_1");
            else if (team == 2)
                graphic = Content.Load<Texture2D>(@"GameBlocks\Derp_2");
        }

        public void Update(Node node)
        {
            if (team == 1)
            {
                if (x != node.x && y == node.y)
                    x += stats.spd * 0.05;
                else if (x != node.x && y != node.y) 
                {
                    x += (stats.spd/1.41) * 0.05;
                    y += (stats.spd / 1.41) * 0.05 * (node.y - y > 0 ? 1 : -1);
                }
            }
            else
            {
                if (x != node.x && y == node.y)
                    x -= stats.spd * 0.05;
                else if (x != node.x && y != node.y)
                {
                    x -= (stats.spd / 1.41) * 0.05;
                    y += (stats.spd / 1.41) * 0.05 * (node.y - y > 0 ? 1 : -1);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle rect)
        {
            spriteBatch.Draw(graphic, new Rectangle((int)(x*Field.BLOCK_WIDTH-rect.X), (int)(y*Field.BLOCK_HEIGHT-rect.Y),graphic.Width,graphic.Height), Color.White);
        }

        public class DerpStats
        {
            //Varied Stats
            public int hp;
            //Required Stats
            public int atk, def, spd;
            //Optional Stats
            public int range, melee, flight, burrow;

            public DerpStats(int hp, int atk, int def, int spd, int range, int melee, int flight, int burrow)
            {
                this.hp = hp;
                this.atk = atk;
                this.def = def;
                this.spd = spd;
                this.range = range;
                this.melee = melee;
                this.flight = flight;
                this.burrow = burrow;
            }
        }
    }
}
