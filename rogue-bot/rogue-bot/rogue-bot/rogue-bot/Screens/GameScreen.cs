using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using rogue_bot.GameObjects;

namespace rogue_bot.Screens
{
    class GameScreen : Screen
    {
        public static int TILE_SIZE = 50;
        public static int FIELD_WIDTH = 13;
        public static int FIELD_HEIGHT = 12;

        int[,] background = new int[FIELD_HEIGHT, FIELD_WIDTH];

        Texture2D tileset;
        SpriteFont font;

        // Create Skyler's Test Game Object
        RogueBot rogueBot;
        GameObject box;

        public GameScreen()
            : base()
        {
            base.InTransitionTime = 1.0;
            base.OutTransitionTime = 0.25;
        }

        public override void Initialize()
        {
            base.Initialize();
			
            //Debug map generation frim mapTest.dat (This is not how the map will work later)
            //Filling outer walls
            for (int i = 0; i < FIELD_WIDTH; i++)
            {
                background[0, i] = 2;
                background[FIELD_HEIGHT - 1, i] = 2;
            }
            for (int i = 1; i < FIELD_HEIGHT - 1; i++)
            {
                background[i, 0] = 2;
                background[i, FIELD_WIDTH - 1] = 2;
            }
            //Reading map file
            try
            {
                using (StreamReader sr = new StreamReader(@"MapFiles/mapTest.dat"))
                {
                    //For each row of the field
                    for (int i = 1; i < FIELD_HEIGHT - 1; i++)
                    {
                        String line = sr.ReadLine();
                        //For each character in the row
                        for (int j = 0; j < line.Length; j++)
                        {
                            //The row is shifted right one to fit within the outer walls
                            background[i, j + 1] = line[j] - '0';
                        }
                    }
                    
                }
            } catch (Exception e)
            {
                //Later, load a default map (empty?) when map data is missing
                Console.WriteLine("The map data could not be read:");
                Console.WriteLine(e.Message);
            }
            // Create test game object
            rogueBot = new RogueBot(100, 100);
            box = new GameObject(200, 250, 50, 50);
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            tileset = Content.Load<Texture2D>(@"Tileset");
            font = Content.Load<SpriteFont>(@"Fonts/MenuFont");

            // load test game object media
            rogueBot.LoadContent();

            box.AddSprite("base", Content.Load<Texture2D>(@"Sprites/black_box"), 0, 50, 50);
        }

        public override void Update(GameTime gameTime)
        {
            rogueBot.Update(gameTime);

            // collision detect
            if (box.Overlap(rogueBot))
                box.SetFrame(1);
            else
                box.SetFrame(0);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < FIELD_HEIGHT; i++)
            {
                for (int j = 0; j < FIELD_WIDTH; j++)
                {
                    spriteBatch.Draw(tileset,
                        new Rectangle(j * TILE_SIZE, i * TILE_SIZE, TILE_SIZE, TILE_SIZE),
                        new Rectangle(background[i, j] * TILE_SIZE, 0, TILE_SIZE, TILE_SIZE),
                        Color.White);
                }
            }

            // Draw the Sprite Object
            rogueBot.Draw(spriteBatch);
            box.Draw(spriteBatch);
        }
    }
}
