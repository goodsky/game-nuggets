using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim.Screens
{
    /// <summary>
    /// Transition Styles
    /// </summary>
    public enum TransitionType
    {
        Instant,
        Fade
    };

    /// <summary>
    /// Screen Manager class that keeps track of the various screens, and handles transitions between them and also draws and updates the current screen and the transitions between screens.
    /// </summary>
    public class ScreenManager
    {
        // Screen Dictionary of available screens
        Dictionary<String, BaseScreen> screens;

        BaseScreen current;
        BaseScreen next;

        // Transitions
        bool transitionIn;
        bool transitionOut;
        float transInVariable;
        float transOutVariable;

        private Texture2D onePixel = null;

        public ScreenManager()
        {
            screens = new Dictionary<String, BaseScreen>();
            transitionIn = false;
            current = null;
            next = null;
        }

        /// <summary>
        /// Adds a new screen to the Dictionary
        /// </summary>
        /// <param name="name">Name of the screen for referencing</param>
        /// <param name="screen">The screen being stored</param>
        public void AddScreen(String name, BaseScreen screen)
        {
            screen.Initialize();
            screens.Add(name, screen);
        }

        /// <summary>
        /// Loads the content for all screens
        /// </summary>
        public void LoadScreenContents(ContentManager contentMan)
        {
            onePixel = contentMan.Load<Texture2D>(@"WhitePixel");

            foreach (var screenPair in screens)
            {
                screenPair.Value.LoadContent(contentMan);
            }
        }

        /// <summary>
        /// Do any additional unloading of content work.
        /// </summary>
        public void UnloadScreenContents()
        {
            foreach (var screenPair in screens)
            {
                screenPair.Value.UnloadContent();
            }
        }

        /// <summary>
        /// Transition to a new screen
        /// </summary>
        /// <param name="name">Name of the next screen</param>
        public void Transition(String name)
        {
            BaseScreen get = null;
            screens.TryGetValue(name, out get);

            if (get == null)
                return;

            if (current != null)
            {
                next = get;
                transitionOut = true;
                transInVariable = 0.0f;
                transOutVariable = 0.0f;
            }
            else
            {
                current = get;
                transitionOut = false;
            }

        }

        /// <summary>
        /// Updates the Current Screen or transition
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (current == null)
                return;
            
            // Transition to next screen
            if (transitionOut) 
            {
                MediaPlayer.Stop();
                switch (current.TransitionOut)
                {
                    case TransitionType.Instant:
                        transitionOut = false;
                        transitionIn = true;
                        break;
                    case TransitionType.Fade:
                        transOutVariable += ProcessFade(1.0f, gameTime);
                        if (transOutVariable >= 1.0)
                        {
                            transOutVariable = 1.0f;
                            transitionOut = false;
                            transitionIn = true;
                        }
                        break;
                    default:
                        break;
                }

                if (current.ControlTransOut)
                    current.Update(gameTime);
            }
            // Transition from last screen
            else if (transitionIn) 
            {
                switch (next.TransitionIn)
                {
                    case TransitionType.Instant:
                        transInVariable = 0.0f;
                        transOutVariable = 0.0f;
                        current = next;
                        break;
                    case TransitionType.Fade:
                        if (transInVariable <= 0.0)
                        {
                            current = next;
                        }
                        transInVariable += ProcessFade(1.0f, gameTime);
                        if (transInVariable >= 1.0)
                        {
                            transInVariable = 1.0f;
                            transitionIn = false;
                        }
                        break;
                    default:
                        transInVariable = 0.0f;
                        transOutVariable = 0.0f;
                        current = next;
                        break;
                }
                if (current.ControlTransIn)
                    current.Update(gameTime);
            }
            // Update current Screen
            else 
            {
                current.Update(gameTime);
            }
        }

        /// <summary>
        /// Draw the current screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gametime, SpriteBatch spriteBatch)
        {
            if (current == null)
                return;

            // Transition to next screen
            if (transitionOut)
            {
                switch (current.TransitionOut)
                {
                    case TransitionType.Instant:
                        current.Draw(gametime, spriteBatch);
                        break;
                    case TransitionType.Fade:
                        current.Draw(gametime, spriteBatch);
                        spriteBatch.Draw(onePixel, new Rectangle(0, 0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT), Color.Black * transOutVariable);
                        break;
                    default:
                        break;
                }
            }
            // Transition from last screen
            else if (transitionIn) 
            {
                switch (current.TransitionIn)
                {
                    case TransitionType.Instant:
                        current.Draw(gametime, spriteBatch);
                        break;
                    case TransitionType.Fade:
                        current.Draw(gametime, spriteBatch);
                        spriteBatch.Draw(onePixel, new Rectangle(0, 0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT), Color.Black * transInVariable);
                        break;
                    default:
                        break;
                }
            }
            // Update current Screen
            else
            {
                current.Draw(gametime, spriteBatch);
            }

        }

        /// <summary>
        /// Using the amount of time we want to spend fading, and the amount of time since the last update, calculate the Fade Alpha
        /// </summary>
        /// <param name="fadeTime"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        private float ProcessFade(float fadeTime, GameTime gameTime)
        { 
            return (float)gameTime.ElapsedGameTime.TotalSeconds / fadeTime; 
        }
    }
}
