using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TugOfWar.GameObject;
using TugOfWar.Screens;

// TugOfWar Project
// Code Started at 6/2/2012 approx. 12:00pm
// author: Skyler Goodell 

namespace TugOfWar
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        // Window is the entire Window
        // Game is just the viewport where the game plays
        public static int WINDOW_WIDTH = 800;
        public static int WINDOW_HEIGHT = 600;
        public static int GAME_WIDTH = 800;
        public static int GAME_HEIGHT = 480;

        // Singleton Object
        // I think this is bad OO design, but it's so convenient
        public static Game game;
        public static Input input;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public ScreenManager screenManager;

        public Game()
        {
            // Create Singletons
            game = this;
            input = new Input();
            
            // Graphics Properties
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;

            Content.RootDirectory = "Content";

            // Window Properties
            this.IsMouseVisible = true;
            this.Window.Title = "Evolution: Tug of War";
            this.Window.AllowUserResizing = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create the Screen Manager
            screenManager = new ScreenManager();
            // Add Screens
            screenManager.AddScreen("Intro", new ScreenIntro());
            screenManager.AddScreen("MainMenu", new ScreenMainMenu());
            screenManager.AddScreen("Game", new ScreenGame());

            // Initialize the screens
            foreach (var screen in screenManager.Screens)
            {
                screen.Value.Initialize();
            }

            // Initialize the first screen
            screenManager.TransitionScreen("Intro");

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

            Texture2D blackscreen = Content.Load<Texture2D>(@"BlackScreen");
            screenManager.blackscreen = blackscreen;

            foreach (var screen in screenManager.Screens)
            {
                screen.Value.LoadContent();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (var screen in screenManager.Screens)
            {
                screen.Value.UnloadContent();
            }
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

            input.BeginUpdate();

            // Update the active screen (via screen manager)
            screenManager.Update(gameTime);

            input.EndUpdate();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Draw the active screen (via screen manager)
            screenManager.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Entry Point
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
