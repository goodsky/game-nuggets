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
    public class ScreenManager
    {
        // Dictionary of availiable screens
        Dictionary<String, Screen> screens;
        public Dictionary<String, Screen> Screens { get { return screens; } }

        // Transition Variables
        Screen CurrentScreen;
        Screen TransitionNextScreen;

        bool transition = false;
        bool fadingOut = true;
        float alpha;


        // BlackScreen for transitions
        public Texture2D blackscreen = null;

        public ScreenManager()
        {
            screens = new Dictionary<String, Screen>();

            CurrentScreen = null;
            TransitionNextScreen = null;
        }

        public void AddScreen(String name, Screen screen)
        {
            screens.Add(name, screen);
        }

        public void TransitionScreen(String name)
        {
            Screen ret = null;
            screens.TryGetValue(name, out ret);

            if (ret != null)
            {
                TransitionNextScreen = ret;

                if (CurrentScreen != null) CurrentScreen.TransitionFrom();
                TransitionNextScreen.TransitionTo();

                transition = true;

                if (CurrentScreen != null)
                {
                    fadingOut = true;
                    alpha = 0.0f;
                }
                else
                {
                    CurrentScreen = ret;
                    fadingOut = false;
                    alpha = 1.0f;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentScreen == null) return;

            if (transition)
            {
                // Fading out the current screen
                if (fadingOut)
                {
                    double dtime = gameTime.ElapsedGameTime.TotalSeconds;
                    alpha = alpha + (float)(dtime / CurrentScreen.OutTransitionTime);

                    // Do the swap (in the secrecy of the darkness!)
                    if (alpha > 1.0)
                    {
                        alpha = 1.0f;
                        fadingOut = false;

                        CurrentScreen = TransitionNextScreen;
                        TransitionNextScreen = null;
                    }
                }
                // Fading in the new screen
                else
                {
                    double dtime = gameTime.ElapsedGameTime.TotalSeconds;
                    alpha = alpha - (float)(dtime / CurrentScreen.InTransitionTime);

                    // Do the swap (in the secrecy of the darkness!)
                    if (alpha <= 0.0)
                    {
                        alpha = 0.0f;
                        transition = false;
                    }
                }
            }
            
            if (!fadingOut) CurrentScreen.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentScreen == null) return;

            CurrentScreen.Draw(spriteBatch);

            if (transition)
                spriteBatch.Draw(blackscreen, new Rectangle(0, 0, blackscreen.Width, blackscreen.Height), Color.Black * alpha);
                
        }
    }
}
