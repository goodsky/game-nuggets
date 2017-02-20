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

using TugOfWar.GameObject.Derps;

namespace TugOfWar.GameObject.DNA
{
    // This class will be the one that controls spawning Derps onto the survivial field.
    // There will be X on each side, and each epoch more will become active. DNA Strands will be rendered and ordered here
    class Spawner
    {
        // Texture Management
        // Will actually be loaded by the ScreenGame, but then it will initialize these parameters for us
        private static Texture2D[] BaseSpawner;
        private static Texture2D[] BaseSpawnerEnd;
        public static Texture2D[] DNATexture;
        private static int BaseSpawnerHeight; // this is the height of the block

        public static void InitializeTextures(Texture2D[] BaseSpawner, Texture2D[] BaseSpawnerEnd, Texture2D[] DNATexture, int height)
        {
            Spawner.BaseSpawner = BaseSpawner;
            Spawner.BaseSpawnerEnd = BaseSpawnerEnd;
            Spawner.DNATexture = DNATexture;
            Spawner.BaseSpawnerHeight = height;
        }

        // Spawner Attributes
        private bool Active = false;
        private int ActiveEpoch;

        // Left of Right Side
        private TEAM team;

        // Position
        private int x, y; //x,y in grid units
        private int worldX, worldY; // x,y in pixel units

        // Max # of DNA you can build
        private int MaxLength;

        // This is used when ghosting the DNA strands
        // This will temporarily skip over, or add a strand at the end of the dna when rendering
        // -1 = no ghosting
        // otherwise give the id of the skipped, or if the index is longer than our current length, the position to skip over
        public int ghostingIndex = -1;

        // My DNA Strand
        List<DNA> strand = new List<DNA>();

        // My list of spawning derps
        LinkedList<DerpSpawn> spawning = new LinkedList<DerpSpawn>();

        // Attributes that guide spawning speed
        DateTime lastSpawn = DateTime.Now;
        double spawningBaseTime = 3.5;
        double spawningVariance = 1.5;

        double nextSpawnTime;

        int spawnMoveSpeed = 2;

        // Constructor
        public Spawner(TEAM team, int x, int y, int BuildSpace, int ActiveEpoch)
        {
            this.team = team;
            this.x = x;
            this.y = y;

            worldX = x * Field.BLOCK_WIDTH;
            worldY = y * Field.BLOCK_HEIGHT;

            this.MaxLength = BuildSpace - 1;

            nextSpawnTime = spawningBaseTime + (MyRandom.NextDouble(team) * spawningVariance * 2 - spawningVariance);

            this.ActiveEpoch = ActiveEpoch;
            if (ActiveEpoch == 1)
                Active = true;
        }

        // Check if the mouse is over one of our dna, or over the end cap where we can build
        public int CheckMouseOver(int mx, int my)
        {
            if (my < worldY || my >= worldY + Field.BLOCK_HEIGHT || !Active)
                return -1;

            int ret = mx - worldX;
            if (team != TEAM.HOME)
                ret *= -1;

            // Snap to grid
            ret /= Field.BLOCK_WIDTH;

            // Zero Index it
            if (team == TEAM.HOME)
                ret -= 1;

            if (ret < 0 || ret > strand.Count || ret > MaxLength)
                return -1;

            // We can't place a 'delete' DNA at the end of the strand
            if (Game.input.constructionState < 0 && ret == strand.Count)
                return -1;

            return ret;
        }

        // Add a new DNA to our strand
        public void AddDNA(int StrandPosition, int DNAID)
        {
            if (StrandPosition == strand.Count)
            {
                strand.Add(new DNA(DNAID));
            }
            else
            {
                strand.RemoveAt(StrandPosition);

                // We can set DNAID to -1 if we simply wish to remove a DNA
                if (DNAID >= 0)
                    strand.Insert(StrandPosition, new DNA(DNAID));
            }
        }

        // Per-Cycle Update
        public void Update(GameTime gameTime)
        {
            if (!Active)
                return;

            // Look if we need to add a new spawn
            if (DateTime.Now.Subtract(lastSpawn).TotalSeconds > nextSpawnTime && (spawning.Count == 0 || Math.Abs(spawning.Last.Value.x - worldX) > DerpSpawn.width))
            {
                spawning.AddLast(new DerpSpawn(worldX + (team == TEAM.AWAY ? Field.BLOCK_WIDTH : 0), worldY));

                lastSpawn = DateTime.Now;
                nextSpawnTime = spawningBaseTime + (MyRandom.NextDouble(team) * spawningVariance * 2 - spawningVariance);
            }

            // Update all of the exists derp spawns we are in charge of
            LinkedListNode<DerpSpawn> cur = spawning.First;
            while (cur != null)
            {
                // See if we are at the end and need to create a derp
                int distanceFromStart;
                if (team == TEAM.HOME)
                {
                    distanceFromStart = cur.Value.x - worldX;
                }
                else
                {
                    distanceFromStart = worldX - cur.Value.x;
                }

                if (distanceFromStart >= strand.Count*Field.BLOCK_WIDTH + (team == TEAM.HOME ? Field.BLOCK_WIDTH : 0))
                {
                    // Generate stats for the new derp
                    int hp = 5 + MyRandom.Next(team, 10);
                    int spd = 25 + MyRandom.Next(team, 50);
                    int atk = 3 + MyRandom.Next(team, 4);
                    int aspd = 25 + MyRandom.Next(team, 50);
                    int rng = 2 + MyRandom.Next(team, 30);

                    DerpStats stats = new DerpStats(hp, spd, atk, aspd, rng, 16, 16);

                    // Make sure there is no obstruction
                    int newDerpX, newDerpY;

                    newDerpX = worldX;
                    if (team == TEAM.HOME)
                        newDerpX += (strand.Count + 1) * Field.BLOCK_WIDTH;
                    else
                        newDerpX -= strand.Count * Field.BLOCK_WIDTH;

                    newDerpY = worldY + (Field.BLOCK_WIDTH/2);
          

                    if (Field.derpManager.SearchForDerp(newDerpX, newDerpY, stats.width) == null)
                    {
                        Field.derpManager.SpawnDerp(newDerpX, newDerpY, team, stats);
                        spawning.RemoveFirst();
                    }
                }
                // Otherwise move us forward
                else
                {
                    // Make sure there is no obstruction
                    if (cur.Next == null || Math.Abs(cur.Next.Value.x - cur.Value.x) > DerpSpawn.width + spawnMoveSpeed)
                    {
                        cur.Value.Step(team == TEAM.HOME ? spawnMoveSpeed : -spawnMoveSpeed);
                    }
                }

                cur = cur.Next;
            }
        }

        // Draw the Spawner and Strand
        public void Draw(SpriteBatch spriteBatch, Rectangle camera)
        {
            // These indices choose which texture to draw
            int activeIndex = Active ? 1 : 0;
            int sideIndex = (team == TEAM.HOME) ? 0 : 1;

            // Draw Base (I'm using a helper method in this class for kicks)
            TryDrawInCamera(spriteBatch, camera, BaseSpawner[activeIndex], worldX, worldY - BaseSpawnerHeight);

            // Draw the DNA
            for (int i = 0; i < strand.Count; ++i)
            {
                if (i == ghostingIndex)
                    continue;

                TryDrawInCamera(spriteBatch, camera, DNATexture[strand[i].TextureID], worldX + ((i+1) * Field.BLOCK_WIDTH * (team == TEAM.HOME ? 1 : -1)), worldY);
            }

            // Draw the Spawns
            foreach (DerpSpawn ds in spawning)
            {
                ds.Draw(spriteBatch, camera);
            }

            // Draw the Base End Cap
            // Move the endcap over one if we are ghosting a new DNA in the end position (this will make us effectively move it twice for the end cap)
            int mvX = strand.Count;
            if (ghostingIndex == mvX)
                mvX++;

            TryDrawInCamera(spriteBatch, camera, BaseSpawnerEnd[sideIndex * 2 + activeIndex], worldX + ((mvX+1) * Field.BLOCK_WIDTH * (team == TEAM.HOME ? 1 : -1)), worldY - BaseSpawnerHeight);
        }

        // This will draw the sprite only when it is within the camera, and at the correct location
        private void TryDrawInCamera(SpriteBatch spriteBatch, Rectangle camera, Texture2D image, int x, int y)
        {
            if (x + image.Width >= camera.X && x <= camera.X + camera.Width)
            {
                spriteBatch.Draw(image, new Rectangle(x - camera.X, y, image.Width, image.Height), Color.White);
            }
        }
    }
}
