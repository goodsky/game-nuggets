using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot.Screens
{
    class IntroScreen : Screen
    {
        Texture2D screen;
        SpriteFont font;
        double start;
        bool done;

        public IntroScreen() : base()
        {
            base.InTransitionTime = 2.0;
            base.OutTransitionTime = 0.25;
        }

        public override void TransitionTo()
        {
            base.TransitionTo();

            done = false;
            start = -1.0;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            screen = Content.Load<Texture2D>(@"BlackScreen");
            font = Content.Load<SpriteFont>(@"Fonts/MenuFont");
        }

        public override void Update(GameTime gameTime)
        {
            if (done) return;

            if (start < 0.0) start = gameTime.TotalGameTime.Seconds;

            if (Input.keyboardState.GetPressedKeys().Length > 0 || Input.MouseLeftKeyDown() || Input.MouseRightKeyDown()
                || (gameTime.TotalGameTime.Seconds - start >= 3))
            {
                Game.screenManager.Transition("MainMenu");
                done = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);

            String[] IntroText = {"15E Programmers:", "   Brandon Scott", "   Skyler Goodell"};

            for (int i = 0; i < IntroText.Length; ++i)
            {
                Vector2 TextSize = font.MeasureString(IntroText[i]);
                Vector2 ScreenCenter = new Vector2(Game.WINDOW_WIDTH / 2, Game.WINDOW_HEIGHT / 2 - (IntroText.Length*TextSize.Y/2) + (i*TextSize.Y));

                spriteBatch.DrawString(font, IntroText[i], ScreenCenter - (TextSize / 2), Color.White);
            }
        }
    }   
}
