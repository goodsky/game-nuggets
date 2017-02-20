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
    class RogueBot : GameObject
    {
        // Rogue-Bot Stats
        RogueBotStats myStats;

        // sqrt(2)/2 shortcut for triangle math
        double rightTriConstant = Math.Sqrt(2) / 2.0;

        // Constructor sets the default stats for this turn
        // We probably want to be able to serialize these stats for different level loadings
        public RogueBot(double x, double y)
            : base(x, y, 50, 50)
        {
            myStats = new RogueBotStats();
        }

        // The standard LoadContent, Update and Draw
        public void LoadContent()
        {
            ContentManager Content = Game.game.Content;
            Texture2D mysprite = Content.Load<Texture2D>(@"Sprites/rogue_bot_1");

            AddSprite("MoveDown", mysprite, 0, 50, 50);
            AddSprite("MoveUp", mysprite, 1, 50, 50);
            AddSprite("MoveRight", mysprite, 2, 50, 50);
            AddSprite("MoveLeft", mysprite, 3, 50, 50);

            SetAnimationDelay(100);
        }

        public override void Update(GameTime gameTime)
        {
            int horMove = ((Input.KeyDown(Keys.D) ? 1 : 0) - (Input.KeyDown(Keys.A) ? 1 : 0));
            int vertMove = ((Input.KeyDown(Keys.S) ? 1 : 0) - (Input.KeyDown(Keys.W) ? 1 : 0));

            double moveval = myStats.speed;
            if (Math.Abs(horMove) + Math.Abs(vertMove) == 2)
                moveval *= rightTriConstant;

            x += horMove * moveval;
            y += vertMove * moveval;

            if (vertMove == -1)
                SetSprite("MoveUp");
            else if (vertMove == 1)
                SetSprite("MoveDown");
            else if (horMove == -1)
                SetSprite("MoveLeft");
            else if (horMove == 1)
                SetSprite("MoveRight");

            // animate
            if (Math.Abs(horMove) + Math.Abs(vertMove) == 0)
            {
                // SetFrame(0);
                SetAnimation(false);
            }
            else
            {
                SetAnimation(true);
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }

    class RogueBotStats
    {
        public double speed = 3.0;
    }
}
