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
            base.InTransitionTime = 0.0;
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
                || (gameTime.TotalGameTime.Seconds - start >= 0.5))
            {
                Game.game.screenManager.TransitionScreen("MainMenu");
                done = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch) 
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);

            //string intro_text = "";
            //spriteBatch.DrawString(font, intro_text, (new Vector2(Game.WINDOW_WIDTH, Game.WINDOW_HEIGHT) / 2) - (font.MeasureString(intro_text) / 2), Color.White);
        }
    }
}
