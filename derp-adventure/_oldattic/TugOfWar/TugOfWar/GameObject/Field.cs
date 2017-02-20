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

namespace TugOfWar.GameObject
{
    class Field : IGameObject
    {
        public static int BLOCK_WIDTH = 50;
        public static int BLOCK_HEIGHT = 48;

        // The Field HEIGHT and WIDTH in number of blocks
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        private int width = 32;
        private int height = 10;

        // GameGrid represents the Field Elements
        private int[,] gameGrid;

        // BlockTexture stores the image for each block
        private Texture2D[] blockTexture;

        //selectorTexture stores images for selection box
        private Texture2D[] selectorTexture;
    
        // the location of the camera for rendering
        private int camX = 0;
        private int camY = 0;
        private double xVel = 0.0;

        // the location of the current selected box
        public Point selectedBox;

        // derp manager to control ALL THE DERPS
        DerpManager derpManager;

        // ***********************
        // Constructors
        public Field(int w, int h)
        {
            width = w;
            height = h;

            gameGrid = new int[height+1, width+1];

            blockTexture = new Texture2D[1];

            selectorTexture = new Texture2D[1];

            selectedBox = new Point(-1, -1);

            derpManager = new DerpManager();
        }

        public void LoadLevel(string filename)
        {
            // Reset Camera Values
            camX = 0;
            xVel = 0;

            // Load the data file
            using (StreamReader mapData = new StreamReader(filename))
            {
                String[] header = mapData.ReadLine().Split(new char[] { ' ' });

                if (header.Length != 2) return;

                int X = Int32.Parse(header[0]);
                int Y = Int32.Parse(header[1]);

                for (int y = 0; y < Y; ++y)
                {
                    String line = mapData.ReadLine();
                    for (int x = 0; x < X; ++x)
                    {
                        gameGrid[y, x] = (int)(line[x] - '0');
                    }
                }
            }
        }

        public void Initialize(Texture2D[] blocks, Texture2D[] selector)
        {
            // Load each block image
            blockTexture = blocks;

            selectorTexture = selector;
        }

        // Slide the FOV left and right
        public void SlideView(int dx)
        {
            xVel += dx;
        }

        public void Update(GameTime gameTime)
        {
            // Move the Screen with Velocity
            if (xVel < -15) xVel = -15;
            if (xVel > 15) xVel = 15;
            camX += (int)xVel;
            xVel /= 1.13;
            if (Math.Abs(xVel) < 0.9) xVel = 0.0;
            
            // Move the Screen with the mouse
            if (Game.input.MouseY() > 0 && Game.input.MouseY() < Game.GAME_HEIGHT)
            {
                if (/*Game.input.MouseX() > -50 && */Game.input.MouseX() < BLOCK_WIDTH * 2)
                    camX -= ((BLOCK_WIDTH * 2 - Game.input.MouseX()) / 10) <= 15 ? (BLOCK_WIDTH * 2 - Game.input.MouseX()) / 10 : 15;

                if (/*Game.input.MouseX() < Game.GAME_WIDTH + 50 && */Game.input.MouseX() > Game.GAME_WIDTH - BLOCK_WIDTH * 2)
                    camX += ((Game.input.MouseX() - (Game.GAME_WIDTH - BLOCK_WIDTH * 2)) / 10) <= 15 ? ((Game.input.MouseX() - (Game.GAME_WIDTH - BLOCK_WIDTH * 2)) / 10) : 15;
            }

            // Cap the camera position
            if (camX < 0) camX = 0;
            if (camX > width * BLOCK_WIDTH - Game.WINDOW_WIDTH) camX = width * BLOCK_WIDTH - Game.WINDOW_WIDTH;

            // Calculare Selected box
            int selectedX = (camX + Game.input.MouseX()) / BLOCK_WIDTH;
            int selectedY = Game.input.MouseY() / BLOCK_HEIGHT;

            selectedBox = new Point(selectedX, selectedY);

            // Update all the Derps
            derpManager.Update(gameTime);

            //Temp Derping Code
            if (Game.input.MouseLeftKeyClicked())
            {
                derpManager.SpawnDerp(selectedX, selectedY, 1);

            }
            else if (Game.input.MouseRightKeyClicked())
            {
                derpManager.SpawnDerp(selectedX, selectedY, 2);
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background blocks that are in the field of view
            int blockBaseX = camX / BLOCK_WIDTH;
            int blockBaseY = camY / BLOCK_HEIGHT;
            for (int y = 0; y <= Game.GAME_HEIGHT / BLOCK_HEIGHT; ++y)
            {
                for (int x = 0; x <= Game.GAME_WIDTH / BLOCK_WIDTH; ++x)
                {
                    Rectangle blockPosition = new Rectangle((blockBaseX + x)*BLOCK_WIDTH - camX, y*BLOCK_HEIGHT, BLOCK_WIDTH, BLOCK_HEIGHT);
                    spriteBatch.Draw(blockTexture[gameGrid[blockBaseY + y, blockBaseX + x]], blockPosition, Color.White);
                }
            }

            Rectangle camera = new Rectangle(camX, 0, Game.GAME_WIDTH, Game.GAME_HEIGHT);

            // Draw dem derps
            derpManager.Draw(spriteBatch, camera);

            // Draw the selected box over top
            int sx = selectedBox.X - blockBaseX;
            int sy = selectedBox.Y - blockBaseY;
            if (sx >= 0 && sx <= Game.GAME_WIDTH / BLOCK_WIDTH &&
                sy >= 0 && sy <= Game.GAME_HEIGHT / BLOCK_HEIGHT)
            {
                Rectangle blockPosition = new Rectangle((blockBaseX + sx) * BLOCK_WIDTH - camX, sy * BLOCK_HEIGHT, BLOCK_WIDTH, BLOCK_HEIGHT);
                spriteBatch.Draw(selectorTexture[0], blockPosition, Color.White);
            }
        }
    }
}
