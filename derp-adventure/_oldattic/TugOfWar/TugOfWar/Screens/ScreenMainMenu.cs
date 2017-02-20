using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TugOfWar.guiElements;

namespace TugOfWar.Screens
{
    class ScreenMainMenu : Screen
    {
        Texture2D screen;
        SpriteFont font;

        TextButton PlayGame;
        TextButton QuitGame;

        Double start;
        Double duration;

        public ScreenMainMenu()
            : base()
        {
            
        }

        public override void TransitionTo()
        {
            base.TransitionTo();

            start = -1;
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            screen = Content.Load<Texture2D>(@"BlackScreen");
            font = Content.Load<SpriteFont>(@"MenuFontLarge");

            PlayGame = new TextButton(font, "Play Game", Color.White, Color.Green, 245, 230, 326, 67);
            PlayGame.MouseEvent = Play;
            QuitGame = new TextButton(font, "Exit", Color.White, Color.Green, 330, 327, 124, 68);
            QuitGame.MouseEvent = Exit;
        }

        public override void Update(GameTime gameTime)
        {
            if (start < 0.0) start = gameTime.TotalGameTime.TotalSeconds;

            duration = gameTime.TotalGameTime.TotalSeconds - start;

            PlayGame.Update(true);
            QuitGame.Update(true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);
            spriteBatch.DrawString(font, "Evolution: Tug of War", new Vector2(65, 55), Color.White);

            PlayGame.Draw(spriteBatch);
            QuitGame.Draw(spriteBatch);
        }

        public void Play()
        {
            Game.game.screenManager.TransitionScreen("Game");
        }

        public void Exit()
        {
            if (duration > 1.0)
                Game.game.Exit();
        }
    }
}
