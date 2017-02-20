using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot.HUD
{
    // This is a general Button Class that can be used in the game
    class Button
    {
        // Parameters of a button
        // Note: two modes of positioning a button. Center mode and top-left mode.
        bool centered;
        float width, height, x, y;
        bool mouseOver;

        // image and font media files
        string buttonText;
        Vector2 stringsize;
        SpriteFont font;
        Color mainColor, mouseoverColor;
        Texture2D mainBackground, mouseoverBackground;
        
        // the 'has focus' variables are used when the player uses arrow keys instead of the mouse
        Boolean hasFocus;

        // Delegate to run when clicked
        public delegate void OnMouseDown();
        public OnMouseDown MouseEvent = null;

        // Constructors
        public Button(string buttonText, SpriteFont font, Color mainColor, Color mouseoverColor, float x, float y, bool centered, OnMouseDown MouseEvent)
            : this(buttonText, font, mainColor, mouseoverColor, null, null, x, y, centered, MouseEvent) { }

        public Button(string buttonText, SpriteFont font, float x, float y, bool centered, OnMouseDown MouseEvent)
            : this(buttonText, font, Color.White, Color.White, null, null, x, y, centered, MouseEvent) { }

        public Button(string buttonText, SpriteFont font, Color mainColor, Color mouseoverColor, Texture2D mainBackground, Texture2D mouseoverBackground, float x, float y, bool centered, OnMouseDown MouseEvent)
        {
            this.buttonText = buttonText;
            this.font = font;
            this.mainColor = mainColor;
            this.mouseoverColor = mouseoverColor;
            this.mainBackground = mainBackground;
            this.mouseoverBackground = mouseoverBackground;

            // dynamically discover the width and height of the button
            stringsize = font.MeasureString(buttonText);

            // width is the max of the string or either of the background (if they were selected)
            width = 0;
            width = Math.Max(width, stringsize.X);
            width = Math.Max(width, (mainBackground != null ? mainBackground.Width : 0));
            width = Math.Max(width, (mouseoverBackground != null ? mouseoverBackground.Width : 0));

            // height is the max of the string or either of the background (if they were selected)
            height = 0;
            height = Math.Max(height, stringsize.Y);
            height = Math.Max(height, (mainBackground != null ? mainBackground.Height : 0));
            height = Math.Max(height, (mouseoverBackground != null ? mouseoverBackground.Height : 0));

            // If the button is centered, then the x,y pair points to the center of the button, otherwise it points to the top-left of the button
            this.centered = centered;

            if (centered)
            {
                this.x = x - (stringsize.X/2);
                this.y = y - (stringsize.Y/2);
            }
            else
            {
                this.x = x;
                this.y = y;
            }

            // Mouse Event Delegate
            this.MouseEvent = MouseEvent;

            hasFocus = false;
        }

        // Set the delegate
        public void SetButtonEvent(OnMouseDown onMouseDownDelegate)
        {
            MouseEvent = onMouseDownDelegate;
        }

        // Check if the mouse is over the button
        // Do events if clicked
        public bool Update(GameTime gameTime)
        {
            if (Input.MouseX() >= x && Input.MouseX() <= x + width && Input.MouseY() >= y && Input.MouseY() <= y + height)
                mouseOver = true;
            else
                mouseOver = false;

            if (mouseOver && Input.MouseLeftKeyClicked())
                Clicked();

            return mouseOver;
        }

        // use this when you want to run the delegate corresponding to this button
        public void Clicked()
        {
            if (MouseEvent != null)
                MouseEvent.Invoke();
        }

        public void SetFocus(Boolean focus)
        {
            this.hasFocus = focus;
        }

        // Draw the button
        // This will include the background images if supplied
        public void Draw(SpriteBatch spriteBatch)
        {
            // draw backgrounds
            if (!mouseOver && !hasFocus)
            {
                if (mainBackground != null)
                    spriteBatch.Draw(mainBackground, new Rectangle((int)x, (int)y, mainBackground.Width, mainBackground.Height), Color.White);
            }
            else
            {
                if (mouseoverBackground != null)
                    spriteBatch.Draw(mouseoverBackground, new Rectangle((int)x, (int)y, mouseoverBackground.Width, mouseoverBackground.Height), Color.White);
            }

            // draw string
            if (!mouseOver && !hasFocus)
            {
                if (buttonText != null && !buttonText.Equals(""))
                    spriteBatch.DrawString(font, buttonText, new Vector2(x, y), mainColor);
            }
            else
            {
                if (buttonText != null && !buttonText.Equals(""))
                    spriteBatch.DrawString(font, buttonText, new Vector2(x, y), mouseoverColor);
            }
        }
    }
}
