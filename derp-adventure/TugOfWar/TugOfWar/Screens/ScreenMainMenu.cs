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

        TextButton SinglePlayer;
        TextButton MultiPlayer;
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

            SinglePlayer = new TextButton(font, "Single Player", Color.White, Color.Green, 65, 250, (int)font.MeasureString("Single Player").X + 1, 67);
            SinglePlayer.MouseEvent = Play;
            MultiPlayer = new TextButton(font, "Online Play", Color.White, Color.Green, 65, 330, (int)font.MeasureString("Online Play").X + 1, 68);
            MultiPlayer.MouseEvent = Online;
            QuitGame = new TextButton(font, "Exit", Color.White, Color.Green, 65, 410, (int)font.MeasureString("Exit").X + 1, 68);
            QuitGame.MouseEvent = Exit;
        }

        public override void Update(GameTime gameTime)
        {
            if (start < 0.0) start = gameTime.TotalGameTime.TotalSeconds;

            duration = gameTime.TotalGameTime.TotalSeconds - start;

            SinglePlayer.Update(true);
            MultiPlayer.Update(true);
            QuitGame.Update(true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);
            spriteBatch.DrawString(font, "Evolution:", new Vector2(65, 55), Color.ForestGreen);
            spriteBatch.DrawString(font, "Survival of the Fittest", new Vector2(65, 110), Color.LightGreen);

            SinglePlayer.Draw(spriteBatch);
            MultiPlayer.Draw(spriteBatch);
            QuitGame.Draw(spriteBatch);
        }

        public void Play(Button button)
        {
            if (duration > 0.25)
                Game.game.screenManager.TransitionScreen("Game");
        }

        public void Online(Button button)
        {
            if (duration > 0.25)
                Game.game.screenManager.TransitionScreen("Multiplayer");
        }

        public void Exit(Button button)
        {
            if (duration > 0.25)
                Game.game.Exit();
        }
    }
}
