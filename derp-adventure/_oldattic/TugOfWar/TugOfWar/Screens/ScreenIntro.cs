using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TugOfWar.Screens
{
    class ScreenIntro : Screen
    {
        Texture2D screen;
        SpriteFont font;
        double start;
        bool done;

        public ScreenIntro()
            : base()
        {
            base.InTransitionTime = 3.0;
            base.OutTransitionTime = 0.25;
        }

        public override void TransitionTo()
        {
            base.TransitionTo();

            done = false;
            start = -1.0;
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            screen = Content.Load<Texture2D>(@"BlackScreen");
            font = Content.Load<SpriteFont>(@"MenuFontLarge");
        }

        public override void Update(GameTime gameTime) 
        {
            if (done) return;

            if (start < 0.0) start = gameTime.TotalGameTime.Seconds;


            if (Game.input.keyboardState.GetPressedKeys().Length > 0 || Game.input.MouseLeftKeyDown() || Game.input.MouseRightKeyDown()
                || (gameTime.TotalGameTime.Seconds - start >= 3))
            {
                Game.game.screenManager.TransitionScreen("MainMenu");
                done = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch) 
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);
            spriteBatch.DrawString(font, "Derp Week 1", new Vector2(180, 245), Color.White);
        }
    }
}
