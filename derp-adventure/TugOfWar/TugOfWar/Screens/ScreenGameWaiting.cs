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
using TugOfWar;

namespace TugOfWar.Screens
{
    // This is the screen that takes over once two players have decided to play it up
    // This will calculate an initial approximate RTT between players and select the random seed
    class ScreenGameWaiting : Screen
    {
        // The person who issued the challenge is the host
        public static Boolean host = false;
        private Boolean waiting;

        Texture2D screen;
        SpriteFont font;

        Double start;
        Double duration;

        public ScreenGameWaiting()
            : base()
        {
            
        }

        public override void TransitionTo()
        {
            base.TransitionTo();

            if (host)
                waiting = false;
            else
                waiting = true;

            // default the important defauly things
            curHandshakes = 0;
            rtt = 0;
            lastPing = 0;

            start = -1;
        }

        int Handshakes = 10;
        int curHandshakes = 0;
        long rtt = 0;
        long lastPing = 0;
        int rnd;
        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            screen = Content.Load<Texture2D>(@"BlackScreen");
            font = Content.Load<SpriteFont>(@"largeGUI");
        }

        public override void Update(GameTime gameTime)
        {
            if (start < 0.0) start = gameTime.TotalGameTime.TotalSeconds;
            duration = gameTime.TotalGameTime.TotalSeconds - start;

            // If you are a host, send out the first message
            if (!waiting && host)
            {
                waiting = true;

                rnd = new Random().Next();

                ++curHandshakes;
                Game.multiplayer.RTT(rnd);
                lastPing = gameTime.TotalGameTime.Milliseconds;
            }

            // The heartbeat of gathering messages
            if (Game.multiplayer.HasMessage())
            {
                string inputLine = Game.multiplayer.GetNextMessage();
                string[] recv = inputLine.Split(' ');

                if (recv[0].Equals("R"))
                {
                    rnd = Int32.Parse(recv[1]);

                    ++curHandshakes;
                    Game.multiplayer.RTT(rnd);

                    if (lastPing != 0)
                    {
                        rtt += gameTime.TotalGameTime.Milliseconds - lastPing;
                    }

                    lastPing = gameTime.TotalGameTime.Milliseconds;
                }
            }

            // Done setting up the connection
            if (curHandshakes == Handshakes)
            {
                MyRandom.setSeeds(rnd, rnd);
                rtt /= (Handshakes - 1);
                Game.multiplayer.setRTT(rtt);

                if (host)
                {
                    this.OutTransitionTime += (rtt / 2000.0);
                }

                Game.game.screenManager.TransitionScreen("Game");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);

            string s = "Waiting for game to start...";
            spriteBatch.DrawString(font, s, new Vector2(Game.WINDOW_WIDTH/2 - font.MeasureString(s).X / 2, 120), Color.White);

            s = "Get Ready!";
            spriteBatch.DrawString(font, s, new Vector2(Game.WINDOW_WIDTH / 2 - font.MeasureString(s).X / 2, 160), Color.White);
        }

    }
}
