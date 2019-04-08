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

using UniversitySim.Screens;
using UniversitySim.Utilities;
using System.Diagnostics;

namespace UniversitySim
{
    /// <summary>
    /// PUBLIC UNIVERSITY SIMULATOR PROJECT
    /// Code started at 4/13/2014 approx 11:30am
    /// author: Skyler Goodell & Brandon Scott & Erik Bergstrom
    /// 
    /// This game was going to be rapid-prototyped in Game Maker,
    /// technical limitations brought us back to XNA. Yay C#.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        // Global game classes
        public static GraphicsDeviceManager Graphics;
        public static Config GameConfig;
        public static float FullScreenRatio;

        // Graphics stuff
        SpriteBatch spriteBatch;
        ScreenManager screenManager;
        Matrix spriteScaleFullScreen;
        Matrix spriteScaleDefault;

        /// <summary>
        /// Game Constructor-
        /// set the game constants
        /// NOTE: at some point the constants should probably be read from a configuration file
        /// </summary>
        public Game()
        {
            // Load the game settings
            GameConfig = new Config(@"Configs\GameSettings.ini");

            // Set up logging
            if (GameConfig.GetBoolValue("DebugLogging", false))
            {
                string logPath = GameConfig.GetStringValue("DebugLoggingPath", null);
                int logsToKeep = GameConfig.GetIntValue("DebugLoggingToKeep", 3);
                if (logPath != null)
                {
                    Logger.Initialize(logPath, logsToKeep);
                }
            }
            
            // Set up the graphics device
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = Constants.WINDOW_WIDTH;
            Graphics.PreferredBackBufferHeight = Constants.WINDOW_HEIGHT;
            Graphics.IsFullScreen = GameConfig.GetBoolValue("FullScreen", false);
            Graphics.ApplyChanges();

            Content.RootDirectory = "Content";

            // Window Properties
            this.IsMouseVisible = true;
            this.Window.Title = "University Simulator 2014";
            this.Window.AllowUserResizing = false;
        }

        /// <summary>
        /// Called when the game is exiting
        /// </summary>
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            Logger.Close();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Prime the game screens
            screenManager = new ScreenManager();
            screenManager.AddScreen("game", new GameScreen(@"Configs\TestingGameRoom.ini"));

            screenManager.Transition("game");

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

            // Calculate the screen scale if we need one for scaling into full screen mode
            FullScreenRatio = (float)Graphics.GraphicsDevice.Viewport.Width / Constants.WINDOW_WIDTH;
            spriteScaleFullScreen = Matrix.CreateScale(FullScreenRatio, FullScreenRatio, 1);
            spriteScaleDefault = Matrix.CreateScale(1, 1, 1);

            screenManager.LoadScreenContents(this.Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            screenManager.UnloadScreenContents();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Start input step
            Input.BeginUpdate();

            // Only do things is the Window has focus 
            if (this.IsActive)
            {
                // Allows the game to exit
                if (Input.KeyClicked(Keys.Escape))
                {
                    this.Exit();
                }

                screenManager.Update(gameTime);
                base.Update(gameTime);
            }

            // End input step
            Input.EndUpdate();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// The screen manager is now responsible for begining and ending the spriteBatches
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //// Create the Viewport if necessary
            //if (Graphics.IsFullScreen)
            //{
            //    GraphicsDevice.Viewport = new Viewport
            //    {
            //        Width = (int)(Constants.WINDOW_WIDTH * FullScreenRatio),
            //        Height = (int)(Constants.WINDOW_HEIGHT * FullScreenRatio),
            //        MinDepth = 0,
            //        MaxDepth = 1
            //    };
            //}

            // Scale if we are in fullscreen
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, (Graphics.IsFullScreen ? spriteScaleFullScreen : spriteScaleDefault));

            screenManager.Draw(gameTime, spriteBatch);
            base.Draw(gameTime);

            spriteBatch.End();
        }

        #region Game Entry Point
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
