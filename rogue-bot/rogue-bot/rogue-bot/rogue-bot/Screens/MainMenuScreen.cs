using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using rogue_bot.HUD;

namespace rogue_bot.Screens
{
    class MainMenuScreen : Screen
    {
        Texture2D screen;
        SpriteFont font;

        ButtonList buttonList;

        public MainMenuScreen() : base()
        {
            base.InTransitionTime = 1.0;
            base.OutTransitionTime = 1.0;
        }

        public override void TransitionTo()
        {
            base.TransitionTo();
        }

        public override void Initialize()
        {
            base.Initialize();

            buttonList = new ButtonList(50, 200, Color.Yellow, Color.White, false);
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            screen = Content.Load<Texture2D>(@"MainMenu");
            font = Content.Load<SpriteFont>(@"Fonts/MenuFont");

            buttonList.SetFont(font);

            // Populate the buttonList now
            buttonList.AddButton("Play Game", PlayGameDelegate);
            buttonList.AddButton("High Scores", HighScoreDelegate);
            buttonList.AddButton("Exit", ExitDelegate);
        }

        public override void Update(GameTime gameTime)
        {
            buttonList.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);

            buttonList.Draw(spriteBatch);
        }

        // MAIN MENU BUTTON DELEGATES
        void PlayGameDelegate()
        {
            Game.screenManager.Transition("Game");
        }

        void HighScoreDelegate()
        {

        }

        void ExitDelegate()
        {
            Game.game.Exit();
        }
    }
}
