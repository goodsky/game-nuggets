using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot.HUD
{
    class ButtonList
    {
        // The List of Buttons
        List<Button> buttonList;
        
        // Position
        int x, y, bot_y;

        // Colors
        Color selectColor;
        Color backColor;

        // Font
        SpriteFont font;

        // Current Selected Item in the list
        int cur_selected = -1;

        // Centered or not
        // This centered is different from the button class 'centered'
        // If true all the buttons will be aligned with the center of the x coordinate, otherwise it will be left aligned
        bool centered;

        public ButtonList(int x, int y, Color selectedColor, Color backColor,bool centered)
        {
            this.x = x;
            this.y = bot_y = y;

            this.selectColor = selectedColor;
            this.backColor = backColor;

            this.centered = centered;

            buttonList = new List<Button>();
        }

        public void SetFont(SpriteFont font)
        {
            this.font = font;
        }

        public void AddButton(String text, Button.OnMouseDown clickDelegate)
        {
            buttonList.Add(new Button(text, font, backColor, selectColor, x, bot_y, centered, clickDelegate));
            bot_y += (int)font.MeasureString(text).Y + 10;
        }

        public void Update(GameTime gameTime)
        {
            int id = 0;
            foreach (Button b in buttonList)
            {
                if (b.Update(gameTime) || cur_selected == id)
                {
                    cur_selected = id;
                    b.SetFocus(true);
                }
                else
                {
                    b.SetFocus(false);
                }

                ++id;
            }

            if (Input.KeyClicked(Keys.Enter) && cur_selected != -1)
                buttonList[cur_selected].Clicked();

            if (Input.KeyClicked(Keys.Up) || Input.KeyClicked(Keys.W))
            {
                --cur_selected;
                if (cur_selected < 0) cur_selected = buttonList.Count - 1;
            }

            if (Input.KeyClicked(Keys.Down) || Input.KeyClicked(Keys.S))
            {
                ++cur_selected;
                if (cur_selected >= buttonList.Count) cur_selected = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Button b in buttonList)
            {
                b.Draw(spriteBatch);
            }
        }
    }
}
