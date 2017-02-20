using System;
using System.Text;
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
    // This is where we can match-make and start online games
    class ScreenMultiplayer : Screen
    {
        // these will be more dynamic some day... maybe
        private string hostname = "64.37.51.159";
        private int port = 1903;

        Texture2D screen;
        ChatArea chatArea;
        PlayerArea playerArea;
        ImageButton BackButton;

        SpriteFont font;

        Double start;
        Double duration;

        public static String myName;

        static bool haveUserName = false;

        public ScreenMultiplayer()
            : base()
        {
            
        }

        public override void TransitionTo()
        {
            base.TransitionTo();

            haveUserName = false;
            myName = "";

            // Try to connect to the server
            if (!Game.multiplayer.Connect(hostname, port))
            {
                Game.game.screenManager.TransitionScreen("MainMenu");
                return;
            }

            // Prompt for a username
            while (myName.Equals(""))
            {
                SimpleInput si = new SimpleInput("Enter Username:", "Please type your desired username.");
                si.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                System.Windows.Forms.DialogResult result = si.ShowDialog(null);

                myName = si.res;
            }
            
            haveUserName = true;
            Game.multiplayer.LogIn(myName);
            playerArea.AddPlayer(Game.multiplayer.getPID(), myName);

            start = -1;
        }

        public override void TransitionFrom()
        {
            base.TransitionFrom();

            playerArea.Clear();
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            screen = Content.Load<Texture2D>(@"BlackScreen");
            font = Content.Load<SpriteFont>(@"MenuFontLarge");

            chatArea = new ChatArea(Content.Load<Texture2D>(@"GameGUI\ChatArea"), Content.Load<SpriteFont>(@"smallGUI"), 365, 35);
            playerArea = new PlayerArea(Content.Load<Texture2D>(@"GameGUI\PlayerArea"), Content.Load<SpriteFont>(@"smallGUI"), 35, 315);

            // ChallengeButton = new ImageButton(Content.Load<Texture2D>(@"GameGUI\ChallengeButton"), 50, 236, 284, 42);
            BackButton = new ImageButton(Content.Load<Texture2D>(@"GameGUI\BackButton"), 35, 35, 72, 66);
            BackButton.MouseEvent = GoBackButton;
        }

        static int MAX_MESSAGES_PER_ITERATION = 5;
        public override void Update(GameTime gameTime)
        {
            if (!haveUserName)
                return;

            if (start < 0.0) start = gameTime.TotalGameTime.TotalSeconds;

            duration = gameTime.TotalGameTime.TotalSeconds - start;

            // Check for Multiplayer Updates
            // (only do a max of X messages per update though)
            for (int iter = 0; iter < MAX_MESSAGES_PER_ITERATION && Game.multiplayer.HasMessage(); ++iter)
            {
                string inputLine = Game.multiplayer.GetNextMessage();

                // Do stuff with the inputLine
                string[] recv = inputLine.Split(' ');

                // Other Player Log In
                if (recv[0].Equals("AA"))
                {
                    string newname = "";
                    for (int i = 2; i < recv.Length; ++i)
                        newname += recv[i] + " ";

                    playerArea.AddPlayer(long.Parse(recv[1]), newname);
                }
                // Other Player Log Out
                else if (recv[0].Equals("BB"))
                {
                    playerArea.RemovePlayer(long.Parse(recv[1]));
                }
                // Other Player Chat
                else if (recv[0].Equals("C"))
                {
                    string ChatName = playerArea.GetPlayerName(long.Parse(recv[1]));

                    if (!ChatName.Equals(""))
                    {
                        string message = "";
                        for (int i = 2; i < recv.Length; ++i)
                            message += recv[i] + " ";

                        chatArea.AddChat(ChatName, message);
                    }
                }
                // Update Challenges
                // Receive Challenge
                else if (recv[0].Equals("DA"))
                {
                    playerArea.RecievedChallenge(long.Parse(recv[1]));
                }
                // Receive a Challenge Cancel
                else if (recv[0].Equals("DB"))
                {
                    playerArea.RecievedCancel(long.Parse(recv[1]));
                }
                // Receive an accepted challenge
                else if (recv[0].Equals("DC"))
                {
                    playerArea.RecievedAccept(long.Parse(recv[1]));
                }
                // Receive a declined challenge
                else if (recv[0].Equals("DD"))
                {
                    playerArea.RecievedDecline(long.Parse(recv[1]));
                }
            }

            chatArea.Update(gameTime);
            playerArea.Update(gameTime);

            // ChallengeButton.Update(true);
            BackButton.Update(true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);

            if (!haveUserName)
                return;

            chatArea.Draw(spriteBatch);
            playerArea.Draw(spriteBatch);

            // ChallengeButton.Draw(spriteBatch);
            BackButton.Draw(spriteBatch);
        }

        public void GoBackButton(Button button)
        {
            // Disconnect from Server
            Game.multiplayer.LogOut();
            Game.multiplayer.Disconnect();

            Game.game.screenManager.TransitionScreen("MainMenu");
        }
    }

    // The area where chatting occurs
    class ChatArea
    {
        Texture2D image;
        SpriteFont font;

        int x, y;
        Rectangle drawRect;

        int printable_start = 500; // this pixel on the image where the text area begins

        int num_chats = 15; // number of chats we can fit vertically
        int width = 380;    // number of chats (pixels) we can fit horizontally

        bool isActive = false; //happens when a player clicks inside the chat area to add new chat
        int active_blink; // counter to blink the cursor (this is all very hacky, but I'm writing it in one sitting with no planning)
        int active_blink_max = 60;
        string[] currentChats;
        StringBuilder WIPChat = new StringBuilder(); // the chat the player is working on

        // used for last key checking
        KeyboardState lastState;

        public ChatArea(Texture2D image, SpriteFont font, int x, int y)
        {
            this.image = image;
            this.font = font;

            this.x = x;
            this.y = y;
            drawRect = new Rectangle(x, y, image.Width, image.Height);

            active_blink = active_blink_max;
            currentChats = new string[num_chats];
        }

        // Add a block of text to the current Chats
        public void AddChat(string user, string chat)
        {
            // Move all the current chats down
            for (int i = num_chats - 1; i > 0; --i)
            {
                currentChats[i] = currentChats[i - 1];
            }

            // Add the new chat
            currentChats[0] = user + ": " + chat;
        }

        // This method interprets what you are getting from the keyboard
        // It will handle special characters, capitalization and all that jazz
        private string getKeyboardChar(Keys key, KeyboardState curState)
        {
            bool usesShift = curState.IsKeyDown(Keys.LeftShift) || curState.IsKeyDown(Keys.RightShift);

            if (key >= Keys.A && key <= Keys.Z)
            {
                if (usesShift)
                    return key.ToString();
                else
                    return "" + (char)(key.ToString()[0] - 'A' + 'a');
            }
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                return ((int)(key - Keys.NumPad0)).ToString();
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                string num = ((int)(key - Keys.D0)).ToString();
                #region special num chars
                if (usesShift)
                {
                    switch (num)
                    {
                        case "1":
                            {
                                num = "!";
                            }
                            break;
                        case "2":
                            {
                                num = "@";
                            }
                            break;
                        case "3":
                            {
                                num = "#";
                            }
                            break;
                        case "4":
                            {
                                num = "$";
                            }
                            break;
                        case "5":
                            {
                                num = "%";
                            }
                            break;
                        case "6":
                            {
                                num = "^";
                            }
                            break;
                        case "7":
                            {
                                num = "&";
                            }
                            break;
                        case "8":
                            {
                                num = "*";
                            }
                            break;
                        case "9":
                            {
                                num = "(";
                            }
                            break;
                        case "0":
                            {
                                num = ")";
                            }
                            break;
                        default:
                            //wtf?
                            break;
                    }
                }
                #endregion
                return num;
            }
            else if (key == Keys.OemPeriod)
                return ".";
            else if (key == Keys.OemTilde)
                return "~";
            else if (key == Keys.Space)
                return " ";
            else if (key == Keys.OemMinus)
                return "-";
            else if (key == Keys.OemPlus)
                return "+";
            else if (key == Keys.OemQuestion && usesShift)
                return "?";
            else if (key == Keys.Back) //backspace
                return "\b";

            // error checking
            return "";
        }

        public void Update(GameTime gameTime)
        {
            // We can gain or lose focus if we are clicking
            if (Game.input.MouseLeftKeyClicked())
            {
                if (Game.input.MouseX() >= x && Game.input.MouseX() <= x + image.Width && Game.input.MouseY() >= y + printable_start && Game.input.MouseY() <= y + image.Height)
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                }
            }

            // Typing
            if (isActive)
            {
                Keys[] keys = Game.input.keyboardState.GetPressedKeys();

                for (int i = 0; i < keys.Length; ++i)
                {
                    if (lastState.IsKeyUp(keys[i]))
                    {
                        // Check for Enter Key
                        if (keys[i] == Keys.Enter)
                        {
                            Game.multiplayer.SendChat(WIPChat.ToString());
                            WIPChat.Clear();
                        }
                        // Any other key
                        else
                        {
                            WIPChat.Append(getKeyboardChar(keys[i], Game.input.keyboardState));

                            // Handle Backspaces
                            if (WIPChat.Length > 0 && WIPChat[WIPChat.Length - 1] == '\b')
                                WIPChat.Remove(WIPChat.Length - (WIPChat.Length > 1 ? 2 : 1), (WIPChat.Length > 1 ? 2 : 1));
                        }
                    }
                } 

                // Make sure the string isn't too long
                while (font.MeasureString(ScreenMultiplayer.myName + ": " + WIPChat.ToString()).X > width)
                {
                    WIPChat.Remove(WIPChat.Length - 1, 1);
                }
            }

            lastState = Game.input.keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image, drawRect, Color.White);

            // Draw Text
            for (int i = 0; i < num_chats && currentChats[i] != null; ++i)
            {
                spriteBatch.DrawString(font, currentChats[i], new Vector2(x + 10, y + printable_start - (i + 1) * 30), Color.Green);
            }

            // Draw WIP Text
            spriteBatch.DrawString(font, ScreenMultiplayer.myName + ":", new Vector2(x + 10, y + printable_start + 5), Color.DarkGreen);
            spriteBatch.DrawString(font, WIPChat, new Vector2(x + 10 + font.MeasureString(ScreenMultiplayer.myName + ": ").X, y + printable_start + 5), Color.DarkGreen);

            // Blink
            if (isActive)
            {
                if (active_blink > 0)
                    spriteBatch.DrawString(font, "|", new Vector2(x + 10 + font.MeasureString(ScreenMultiplayer.myName + ": " + WIPChat).X + 1, y + printable_start + 5), Color.LightGreen);

                active_blink--;

                if (active_blink < -active_blink_max/2)
                    active_blink = active_blink_max;
            }
        }
    }

    // The area where players are kept track of
    class PlayerArea
    {
        Texture2D image;
        SpriteFont font;

        int x, y;
        Rectangle drawRect;

        List<Player> players = new List<Player>();

        public PlayerArea(Texture2D image, SpriteFont font, int x, int y)
        {
            this.image = image;
            this.font = font;

            this.x = x;
            this.y = y;
            drawRect = new Rectangle(x, y, image.Width, image.Height);
        }

        public void AddPlayer(long id, string name)
        {
            players.Add(new Player(id, name, font));
            players.Sort();
        }

        public void RemovePlayer(long id)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].id == id)
                {
                    players.RemoveAt(i);
                    break;
                }
            }
        }

        public void RecievedChallenge(long id)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].id == id)
                {
                    players[i].recievedChallenge = true;
                }
            }
        }

        public void RecievedCancel(long id)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].id == id)
                {
                    players[i].recievedChallenge = false;
                }
            }
        }

        public void RecievedAccept(long id)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].id == id)
                {
                    ScreenGameWaiting.host = false;

                    // Go to game screen
                    Game.game.screenManager.TransitionScreen("GameWaiting");
                }
            }
        }

        public void RecievedDecline(long id)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].id == id)
                {
                    players[i].sentChallenge = false;
                }
            }
        }

        public string GetPlayerName(long id)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].id == id)
                {
                    return players[i].name;
                }
            }

            return "";
        }

        public void Clear()
        {
            players.Clear();
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < players.Count; ++i)
                players[i].Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image, drawRect, Color.White);

            // Draw Player Names
            for (int i = 0; i < 6; ++i)
            {
                if (i >= players.Count)
                    break;

                players[i].Draw(spriteBatch, 45, 355 + i * 30);
            }
        }

        private class Player : IComparable<Player>
        {
            public long id;
            public string name;
            public int nameLength;

            public bool isMe = false;
            public bool sentChallenge = false;
            public bool recievedChallenge = false;

            TextButton challenge;
            TextButton cancelChallenge;
            TextButton acceptChallenge;
            TextButton declineChallenge;

            SpriteFont font;

            public Player(long id, string name, SpriteFont font)
            {
                this.id = id;
                this.name = name;
                this.font = font;

                nameLength = (int)font.MeasureString(name).X;

                isMe = (id == Game.multiplayer.getPID());

                string s = "Challenge!";
                challenge = new TextButton(font, s, Color.Red, Color.DarkRed, 0, 0, (int)font.MeasureString(s).X, (int)font.MeasureString(s).Y);
                challenge.MouseEvent += Challenge;

                s = "Cancel";
                cancelChallenge = new TextButton(font, s, Color.Red, Color.DarkRed, 0, 0, (int)font.MeasureString(s).X, (int)font.MeasureString(s).Y);
                cancelChallenge.MouseEvent += Cancel;

                s = "Accept";
                acceptChallenge = new TextButton(font, s, Color.Green, Color.DarkGreen, 0, 0, (int)font.MeasureString(s).X, (int)font.MeasureString(s).Y);
                acceptChallenge.MouseEvent += Accept;

                s = "Decline";
                declineChallenge = new TextButton(font, s, Color.Red, Color.DarkRed, 0, 0, (int)font.MeasureString(s).X, (int)font.MeasureString(s).Y);
                declineChallenge.MouseEvent += Decline;
            }

            public void Update()
            {
                if (isMe)
                    return;

                if (recievedChallenge)
                {
                    acceptChallenge.Update(true);
                    declineChallenge.Update(true);
                }
                else if (sentChallenge)
                {
                    cancelChallenge.Update(true);
                }
                else
                {
                    challenge.Update(true);
                }
            }

            public void Draw(SpriteBatch spriteBatch, int x, int y)
            {
                spriteBatch.DrawString(font, name, new Vector2(x, y), isMe ? Color.Yellow : Color.White);

                if (isMe)
                    return;

                challenge.Y = cancelChallenge.Y = acceptChallenge.Y = declineChallenge.Y = y;

                if (recievedChallenge)
                {
                    spriteBatch.DrawString(font, "Received-", new Vector2(x + nameLength + 3, y), Color.Gray);

                    acceptChallenge.X = x + nameLength + 75;
                    declineChallenge.X = x + nameLength + 130;
                    
                    acceptChallenge.Draw(spriteBatch);
                    declineChallenge.Draw(spriteBatch);
                }
                else if (sentChallenge)
                {
                    spriteBatch.DrawString(font, "Sent-", new Vector2(x + nameLength + 3, y), Color.Gray);

                    cancelChallenge.X = x + nameLength + 45;
                    cancelChallenge.Draw(spriteBatch);
                }
                else
                {
                    challenge.X = x + nameLength + 3;
                    challenge.Draw(spriteBatch);
                }
                
            }

            public void Challenge(Button b)
            {
                sentChallenge = true;

                Game.multiplayer.SendChallenge(id);
            }

            public void Cancel(Button b)
            {
                sentChallenge = false;

                Game.multiplayer.CancelChallenge(id);
            }

            public void Accept(Button b)
            {
                Game.multiplayer.AcceptChallenge(id);

                // Go to game screen
                ScreenGameWaiting.host = true;
                Game.game.screenManager.TransitionScreen("GameWaiting");
            }

            public void Decline(Button b)
            {
                recievedChallenge = false;

                Game.multiplayer.DeclineChallenge(id);
            }

            public int CompareTo(Player o)
            {
                return name.CompareTo(o.name);
            }
        }
    }
}
