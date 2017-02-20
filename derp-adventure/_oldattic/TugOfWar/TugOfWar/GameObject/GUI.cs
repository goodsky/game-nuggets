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
        Texture2D controlRibbon;
        ImageButton MenuButton;

        Texture2D infoBox;
        Texture2D buildingBox;
        Texture2D energyBox;

        // Building Box Buttons
        ImageButton[,] buildboxButtons;
        ButtonArray[,] buildboxArrays;

        static SpriteFont font;

        // Active Menu Box
        int ActiveMenu = 0;
        
        // The information box string (must be parsed to make it fit in the box
        private static String rawInfoText;
        public static String infoText
        {
            get
            {
                return rawInfoText;
            }
            set
            {
                if (value == null)
                {
                    rawInfoText = null;
                    return;
                }

                String line = String.Empty;
                String returnString = String.Empty;
                String[] wordArray = value.Split(' ');

                foreach (String word in wordArray)
                {
                    if (font.MeasureString(line + word).Length() > 175)
                    {
                        returnString = returnString + line + '\n';
                        line = String.Empty;
                    }
        
                    line = line + word + ' ';
                }

                rawInfoText = returnString + line;
            }
        }

        public GUI()
        {
            
        }

        public void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            controlRibbon = Content.Load<Texture2D>(@"GameGUI\ControlRibbon");
            
            infoBox = Content.Load<Texture2D>(@"GameGUI\InfoBox");
            buildingBox = Content.Load<Texture2D>(@"GameGUI\BuildingBox");
            energyBox = Content.Load<Texture2D>(@"GameGUI\EnergyBox");

            // Building Box Buttons
            buildboxButtons = new ImageButton[2, 3];
            ImageButton.OnMouseDown[,] buildboxActions = { { OnButton0_0, OnButton0_1, OnButton0_2 }, { OnButton1_0, OnButton1_1, OnButton1_2 } };
            String[,] infoString = { { "Build Level 1 DNA", "Build Level 2 DNA", "Build Level 3 DNA" }, { "Build Level 1 Factories", "Build Level 2 Factories", "Build Level 3 Factories" } };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Texture2D temp = Content.Load<Texture2D>(@"GameGUI\Buttons\BuildButton" + j + "_" + i);
                    buildboxButtons[j, i] = new ImageButton(temp, 327 + 110 * i, 520 + 35 * j, 100, 30);
                    buildboxButtons[j, i].MouseEvent = buildboxActions[j, i];
                    buildboxButtons[j, i].info = infoString[j, i];

                    if (i != 0)
                    {
                        buildboxButtons[j, i].Active = false;
                        buildboxButtons[j, i].info += " *LOCKED*";
                    }
                }
            }
            // Building Box Button Submenus (Arrays)
            buildboxArrays = new ButtonArray[2, 3];
            int[,] ArraySizes = {{2, 1, 1}, {1, 1, 1}};
            ImageButton.OnMouseDown[, ,] buildboxArrayActions = { { { }, { }, { } }, { { }, { }, { } } };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Texture2D[] temp = new Texture2D[ArraySizes[j, i]];
                    for (int k = 0; k < temp.Length; ++k)
                        temp[k] = Content.Load<Texture2D>(@"GameGUI\Buttons\BuildArray" + j + "_" + i + "_" + k);

                    buildboxArrays[j, i] = new ButtonArray(temp, buildboxButtons[j, i].X + 50, buildboxButtons[j, i].Y);
                }
            }

            Texture2D menuButton = Content.Load<Texture2D>(@"GameGUI\MenuButton");
            MenuButton = new ImageButton(menuButton, 199, 491, 32, 105);
            MenuButton.MouseEvent = OnMenuButton;
            MenuButton.info = "Return to the Main Menu";

            font = Content.Load<SpriteFont>("guiFontSmall");
        }

        public void OnMenuButton()
        {
            Game.game.screenManager.TransitionScreen("Intro");
        }

        #region BuildButtonEvents
        // DNA Level 1
        public void OnButton0_0()
        {
            if (!buildboxButtons[0, 0].Active) return;

            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            buildboxArrays[0, 0].Active = true;
            ActiveMenu = 0;
        }
        // DNA Level 2
        public void OnButton0_1()
        {
            if (!buildboxButtons[0, 1].Active) return;

            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            buildboxArrays[0, 1].Active = true;
            ActiveMenu = 1;
        }
        // DNA Level 3
        public void OnButton0_2()
        {
            if (!buildboxButtons[0, 2].Active) return;

            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            buildboxArrays[0, 2].Active = true;
            ActiveMenu = 2;
        }
        // Factory Level 1
        public void OnButton1_0()
        {
            if (!buildboxButtons[1, 0].Active) return;

            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            buildboxArrays[1, 0].Active = true;
            ActiveMenu = 3;
        }
        // Factory Level 2
        public void OnButton1_1()
        {
            if (!buildboxButtons[1, 1].Active) return;

            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            buildboxArrays[1, 1].Active = true;
            ActiveMenu = 4;
        }
        // Factory Level 3
        public void OnButton1_2()
        {
            if (!buildboxButtons[1, 2].Active) return;

            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            buildboxArrays[1, 2].Active = true;
            ActiveMenu = 5;
        }
        #endregion

        #region BuildButtonArrayEvent

        #endregion

        public void Update(GameTime gameTime)
        {
            // Note: Update from highest precendence down (and do not update lower layers if one is 'moused over'
            bool top = true;
            bool isover;

            // Update the Build Box
            isover = buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Update(top);
            top = top && !isover;

            //foreach (ImageButton b in buildboxButtons)
            for (int i = 0; i < 6; ++i)
            {
                isover = buildboxButtons[i / 3, i % 3].Update(top);
                top = top && !isover;
            }

            // Update the Menu Button
            isover = MenuButton.Update(top);
            top = top && !isover;

            // If we clicked on nothing then clear focus
            if (top && Game.input.MouseLeftKeyClicked())
            {
                infoText = null;
                buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the Control ribbon
            spriteBatch.Draw(controlRibbon, new Rectangle(0, Game.GAME_HEIGHT, controlRibbon.Width, controlRibbon.Height), Color.White);

            // Draw the Info Box
            spriteBatch.Draw(infoBox, new Rectangle(9, 491, infoBox.Width, infoBox.Height), Color.White);
            if (infoText != null)
                spriteBatch.DrawString(font, infoText, new Vector2(14, 496), Color.Black);

            // Draw the Building Box
            spriteBatch.Draw(buildingBox, new Rectangle(234, 491, buildingBox.Width, buildingBox.Height), Color.White);
            //foreach (ImageButton b in buildboxButtons)
            for (int i = 0; i < 6; ++i)
                buildboxButtons[i / 3, i % 3].Draw(spriteBatch);
            buildboxArrays[ActiveMenu / 3, ActiveMenu % 3].Draw(spriteBatch);

            // Draw the Energy Box
            int energyX = 665;
            int energyY = 491;
            spriteBatch.Draw(energyBox, new Rectangle(energyX, energyY, energyBox.Width, energyBox.Height), Color.White);
            spriteBatch.DrawString(font, GameState.state.Energy.ToString(), new Vector2(energyX + 33, energyY + 24), Color.Black);

            spriteBatch.DrawString(font, "+" + GameState.state.EnergyGain.ToString(), new Vector2(energyX + 10, energyY + 50), Color.Green);
            spriteBatch.DrawString(font, "-" + GameState.state.EnergyLoss.ToString(), new Vector2(energyX + 65, energyY + 50), Color.Red);

            // Draw the Menu Button
            MenuButton.Draw(spriteBatch); 
        }
    }
}
