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

namespace TugOfWar.Screens
{
    class ScreenGame : Screen
    {
        // Game Objects
        Field field;
        GUI gui;

        public ScreenGame()
            : base()
        {
            base.InTransitionTime = 1.0;
        }

        public override void TransitionTo()
        {
            base.TransitionTo();

            field.LoadLevel("Map.dat");
        }

        public override void Initialize()
        {
            field = new Field(32, 10);
            gui = new GUI(); 
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            // Load Block Images
            Texture2D[] blocks = new Texture2D[3];
            
            // 0 - Grass
            blocks[0] = Content.Load<Texture2D>(@"GameBlocks\Block_Grass");
            // 1 - Road
            blocks[1] = Content.Load<Texture2D>(@"GameBlocks\Block_Road");
            // 2 - Disabled Hub
            blocks[2] = Content.Load<Texture2D>(@"GameBlocks\Block_X");

            // Load GUI Images
            Texture2D[] selector = new Texture2D[1];
            // 0 - SelectedBlock
            selector[0] = Content.Load<Texture2D>(@"GameBlocks\Selector");

            field.Initialize(blocks, selector);

            // Load the GUI components
            gui.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // Scroll back and forth
            if (Game.input.KeyDown(Keys.A) || Game.input.KeyDown(Keys.Left)) field.SlideView(-2);
            if (Game.input.KeyDown(Keys.D) || Game.input.KeyDown(Keys.Right)) field.SlideView(2);

            field.Update(gameTime);
            gui.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            field.Draw(spriteBatch);
            gui.Draw(spriteBatch);
        }
    }
}
