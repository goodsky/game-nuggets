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

// Branch 3/17/2013 to TugOfWar Solo Project
// This is now kinda my hobby project. I don't think anyone else has time to work on it,
// maybe I'll get Brandon to help later if I get it off the ground.
// -Skyler

// Post-College Graduation Reboot
// Let's finally actually finish this project.
// 5/8/2013
// -Skyler

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

        // Singleton Objects
        public static Game game;
        public static Input input;
        public static Multiplayer multiplayer;

        // XNA Graphics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // My personal Screen Manager
        public ScreenManager screenManager;

        // Calculate Framerate
        bool SHOW_FPS = true;
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        public Game()
        {
            // Create Singletons
            game = this;
            input = new Input();
            multiplayer = new Multiplayer();
            
            // Graphics Properties
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.IsFullScreen = false;

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
            screenManager.AddScreen("Multiplayer", new ScreenMultiplayer());
            screenManager.AddScreen("GameWaiting", new ScreenGameWaiting());
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

            // Make sure we let everyone know we are logged out when we close the game
            // (even if we never connected to multiplayer, just try the call)
            multiplayer.LogOut();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Calculate FPS
            if (SHOW_FPS)
            {
                elapsedTime += gameTime.ElapsedGameTime;

                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    frameRate = frameCounter;
                    frameCounter = 0;
                }
            }

            // Allows the game to exit
            if (input.KeyDown(Keys.Escape))
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

            // Draw FPS if required
            if (SHOW_FPS)
            {
                frameCounter++;
                string fps = string.Format("fps: {0}", frameRate);
                spriteBatch.DrawString(GUI.largeFont, fps, new Vector2(13, 13), Color.Black);
                spriteBatch.DrawString(GUI.largeFont, fps, new Vector2(12, 12), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Entry Point
        [STAThread]
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
