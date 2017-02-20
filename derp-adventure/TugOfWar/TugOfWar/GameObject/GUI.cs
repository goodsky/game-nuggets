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

namespace TugOfWar.GameObject
{
    class GUI : IGameObject
    {
        // Textures to create the GUI
        // See the Paint.NET designer file in the Content folder for details about how they all fit together
        Texture2D BottomBar;

        ImageButton Epoch1;
        ImageButton Epoch2;
        ImageButton Epoch3;

        // Evo Blocks
        ImageButton[,] EvoButton = new ImageButton[3, 3];

        // Remove DNA Button
        ImageButton RemoveDNA;

        // Energy Bar
        Texture2D EnergyPointer;

        // Other Misc Buttons
        ImageButton Options;
        ImageButton Quit;

        public static SpriteFont smallFont;
        public static SpriteFont largeFont;

        private static string infoTitle;
        private static string infoText;

        public GUI()
        {
            
        }

        public void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            BottomBar = Content.Load<Texture2D>(@"GameGUI\BottomBar");

            Texture2D EvoBack1 = Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1");
            Texture2D EvoBack2 = Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch2");
            Texture2D EvoBack3 = Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch3");

            // Evo Blocks and Epoch Buttons
            // Epoch 1
            Epoch1 = new ImageButton(Content.Load<Texture2D>(@"GameGUI\Epoch1"), 169, 495, 30, 90);
            Epoch1.SetInfo("Epoch 1", "Epoch 1 is active. You may use these Evo Blocks.");
            Epoch1.MouseEvent = null;
            Epoch1.setMode(2);

            EvoButton[0, 0] = new ImageButton(EvoBack1, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 203, 495, 67, 28);
            EvoButton[0, 0].SetInfo("ATK DNA", "Increase Attack Power of Derps.", 1);
            EvoButton[0, 0].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            EvoButton[0, 0].setMode(1);
            EvoButton[0, 1] = new ImageButton(EvoBack1, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_BlueDNA"), 203, 526, 67, 28);
            EvoButton[0, 1].SetInfo("DEF DNA", "Increase Defense of Derps.", 2);
            EvoButton[0, 1].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            EvoButton[0, 1].setMode(1);
            EvoButton[0, 2] = new ImageButton(EvoBack1, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_YellowDNA"), 203, 557, 67, 28);
            EvoButton[0, 2].SetInfo("Solar DNA", "Energy harvester.", 3);
            EvoButton[0, 2].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            EvoButton[0, 2].setMode(1);

            // Epoch 2
            Epoch2 = new ImageButton(Content.Load<Texture2D>(@"GameGUI\Epoch2"), 278, 495, 30, 90);
            Epoch2.SetInfo("Epoch 2", "Epoch 2 is locked. Build more energy to unlock it.");
            Epoch2.MouseEvent = null;

            EvoButton[1, 0] = new ImageButton(EvoBack2, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 312, 495, 67, 28);
            EvoButton[1, 0].SetInfo("MELEE DNA", "Epoch 2 is locked. Build more energy to unlock it.", 4);
            EvoButton[1, 0].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            EvoButton[1, 1] = new ImageButton(EvoBack2, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 312, 526, 67, 28);
            EvoButton[1, 1].SetInfo("RANGE DNA", "Epoch 2 is locked. Build more energy to unlock it.", 5);
            EvoButton[1, 1].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            EvoButton[1, 2] = new ImageButton(EvoBack2, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 312, 557, 67, 28);
            EvoButton[1, 2].SetInfo("SPEED DNA", "Epoch 2 is locked. Build more energy to unlock it.", 6);
            EvoButton[1, 2].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;

            // Epoch 3
            Epoch3 = new ImageButton(Content.Load<Texture2D>(@"GameGUI\Epoch2"), 387, 495, 30, 90);
            Epoch3.SetInfo("Epoch 3", "Epoch 3 is locked. Build more energy to unlock it.", 6);
            Epoch3.MouseEvent = null;

            EvoButton[2, 0] = new ImageButton(EvoBack3, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 421, 495, 67, 28);
            EvoButton[2, 0].SetInfo("MUTATE DNA", "Epoch 3 is locked. Build more energy to unlock it.", 7);
            EvoButton[2, 0].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            EvoButton[2, 1] = new ImageButton(EvoBack3, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 421, 526, 67, 28);
            EvoButton[2, 1].SetInfo("UNIFY DNA", "Epoch 3 is locked. Build more energy to unlock it.", 8);
            EvoButton[2, 1].MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;
            //EvoButton[2, 2] = new ImageButton(EvoBack3, Content.Load<Texture2D>(@"GameGUI\EvoBlockEpoch1_RedDNA"), 421, 557, 67, 28);
            //EvoButton[2, 2].SetInfo("Epoch 1", "Epoch 1 is active. You may use these Evo Blocks.");
            //EvoButton[2, 2].MouseEvent = null;

            // Remove DNA Button
            RemoveDNA = new ImageButton(Content.Load<Texture2D>(@"GameGUI\RemoveDNA"), 364, 465, 125, 28);
            RemoveDNA.SetInfo("Remove DNA", "Click this then click the DNA to remove.", -1);
            RemoveDNA.MouseEvent = ButtonClickEvents.EvoBlockBuildEvent;

            // Energy Stuff
            EnergyPointer = Content.Load<Texture2D>(@"GameGUI\EnergyMeter");

            // Menu Bar Buttons
            Options = new ImageButton(Content.Load<Texture2D>(@"GameGUI\Options"), 716, 505, 67, 28);
            Options.SetInfo("Options", "CURRENTLY NOT IMPLEMENTED");
            Options.MouseEvent = null;

            Quit = new ImageButton(Content.Load<Texture2D>(@"GameGUI\Quit"), 716, 543, 67, 28);
            Quit.SetInfo("Quit", "Quit the current session.");
            Quit.MouseEvent = OnQuitButton;
           
            smallFont = Content.Load<SpriteFont>("smallGUI");
            largeFont = Content.Load<SpriteFont>("largeGUI");
        }

        // Update the Info Box
        // The newline characters must be manually added
        // I catch every change to the string and automatically add the newlines as needed
        public static void ShowInfo(string title, string info)
        {
            if (title != null)
            {
                String line = String.Empty;
                String[] tmp = title.Split(' ');
                infoTitle = String.Empty;

                foreach (String word in tmp)
                {
                    if (largeFont.MeasureString(line + word).X > 140)
                    {
                        infoTitle = infoTitle + line + '\n';
                        line = String.Empty;
                    }

                    line = line + word + ' ';
                }

                infoTitle = infoTitle + line;
            }

            if (info != null)
            {
                String line = String.Empty;
                String[] tmp = info.Split(' ');
                infoText = String.Empty;

                foreach (String word in tmp)
                {
                    if (smallFont.MeasureString(line + word).X > 138)
                    {
                        infoText = infoText + line + '\n';
                        line = String.Empty;
                    }

                    line = line + word + ' ';
                }

                infoText = infoText + line;
            }
        }

        // Delegate for Quit Button Press
        public void OnQuitButton(Button button)
        {
            Game.game.screenManager.TransitionScreen("Intro");
        }

        public void Update(GameTime gameTime)
        {
            // Update Buttons
            Epoch1.Update(true);
            Epoch2.Update(true);
            Epoch3.Update(true);

            foreach (ImageButton ib in EvoButton)
                if (ib != null) ib.Update(true);

            RemoveDNA.Update(true);

            Options.Update(true);
            Quit.Update(true);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the Bottom Background Bar
            spriteBatch.Draw(BottomBar, new Rectangle(0, Game.GAME_HEIGHT - 15, BottomBar.Width, BottomBar.Height), Color.White);

            // Draw the Info Box
            if (infoTitle != null)
                spriteBatch.DrawString(largeFont, infoTitle, new Vector2(10, 494), Color.Black);

            if (infoText != null)
                spriteBatch.DrawString(smallFont, infoText, new Vector2(10, 519), Color.Black);

            // Draw the Evo Blocks Area
            Epoch1.Draw(spriteBatch);
            Epoch2.Draw(spriteBatch);
            Epoch3.Draw(spriteBatch);

            foreach (ImageButton ib in EvoButton)
                if (ib != null) ib.Draw(spriteBatch);

            RemoveDNA.Draw(spriteBatch);

            // Draw the Energy Box
            spriteBatch.Draw(EnergyPointer, new Rectangle(590, 563, EnergyPointer.Width, EnergyPointer.Height), Color.White);

            spriteBatch.DrawString(smallFont, "Current Status:", new Vector2(510, 495), Color.Black);

            // These values are somewhat arbitrary
            int net = GameState.state.EnergyGain - GameState.state.EnergyLoss;
            if (net <= -5)
                spriteBatch.DrawString(smallFont, "Dying", new Vector2(615, 495), Color.Red);
            else if (net >= 5)
                spriteBatch.DrawString(smallFont, "Growing", new Vector2(615, 495), Color.Green);
            else
                spriteBatch.DrawString(smallFont, "Stable", new Vector2(615, 495), Color.Black);

            spriteBatch.DrawString(smallFont, "-" + GameState.state.EnergyLoss.ToString(), new Vector2(520, 526), Color.Red);
            spriteBatch.DrawString(smallFont, "+" + GameState.state.EnergyGain.ToString(), new Vector2(662, 526), Color.Green);

            // Draw the Menu Butons
            Options.Draw(spriteBatch);
            Quit.Draw(spriteBatch);
        }
    }
}
