/*
 * Rogue-Bot Game Class
 * 
 * This is the main starting point for the program and the main XNA Game class.
 * Also these comments are used as a test of my git-hub branch merging skills.
 * 
 * Authors:
 * Brandon Scott
 * Skyler Goodell
 */
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using rogue_bot.Screens;

namespace rogue_bot
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static int WINDOW_WIDTH = 800;
        public static int WINDOW_HEIGHT = 600;

        // Singleton Objects
        public static Game game;
        public static ScreenManager screenManager;

        // Game Instace Objects
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game()
        {
            // Set Singleton Objects
            game = this;
            screenManager = new ScreenManager();

            // Set up Graphics Manager
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Window Properties
            this.IsMouseVisible = true;
            this.Window.Title = "Rogue-Bot";
            this.Window.AllowUserResizing = false;

            // Add Screens
            screenManager.AddScreen("Intro", new IntroScreen());
            screenManager.AddScreen("MainMenu", new MainMenuScreen());
            screenManager.AddScreen("Game", new GameScreen());

            // Initialize the screens
            foreach (var screen in screenManager.Screens)
                screen.Value.Initialize();

            // Prime the first screen
            screenManager.Transition("Intro");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize the screens
            foreach (var screen in screenManager.Screens)
                screen.Value.LoadContent();

            screenManager.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            screenManager.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            Input.BeginUpdate();

            // Update the game logic via ScreenManager
            screenManager.Update(gameTime);

            Input.EndUpdate();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Draw the active screen via ScreenManager
            screenManager.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region MAIN_ENTRY_POINT
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                game.Run();
            }

        }
        #endregion
    }
}
