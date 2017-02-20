using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TugOfWar.GameObject;
using TugOfWar.GameObject.Derps;
using TugOfWar.GameObject.DNA;

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

            // Set up the random numbers if we haven't already
            if (!Multiplayer.isMult)
            {
                MyRandom.generateSeeds();
            }

            // Try to load the map and show any errors that might pop up
            int error = field.LoadLevel("MapTiny.dat");
            if (error == 1)
                MessageBox.Show("Map Loading Error #1: Improper Header Format");
            else if (error == 2)
                MessageBox.Show("Map Loading Error #2: Invalid Map. Uneven hubs or does not contain path between hubs.");

           
        }

        public override void Initialize()
        {
            field = new Field(64, 10);
            gui = new GUI(); 
        }

        public override void LoadContent()
        {
            ContentManager Content = Game.game.Content;

            ///////////////////////////////////////////////////////////////////////////////////////////
            // Load Block Images and Set Offsets for necessary ones
            // srcOffset is if the image is actually a strip, it tells us which src to get it from
            // positionOffset tell how high off the ground this block is (for overlapping field blocks)
            ///////////////////////////////////////////////////////////////////////////////////////////
            Dictionary<char, Texture2D> blocks = new Dictionary<char, Texture2D>();
            Dictionary<char, Rectangle> srcOffset = new Dictionary<char, Rectangle>();
            Dictionary<char, int> positionOffset = new Dictionary<char, int>();
            
            // 0 Grass
            blocks['0'] = Content.Load<Texture2D>(@"FieldBlocks\Block_Grass");
            srcOffset['0'] = new Rectangle(0, 0, Field.BLOCK_WIDTH, blocks['0'].Height);
            positionOffset['0'] = 0;
            // + Road
            blocks['+'] = Content.Load<Texture2D>(@"FieldBlocks\Block_Road");
            srcOffset['+'] = new Rectangle(0, 0, Field.BLOCK_WIDTH, blocks['+'].Height);
            positionOffset['+'] = 0;
            // - Growing Zone
            blocks['-'] = Content.Load<Texture2D>(@"FieldBlocks\Block_Road");
            srcOffset['-'] = new Rectangle(0, 0, Field.BLOCK_WIDTH, blocks['-'].Height);
            positionOffset['-'] = 0;
            // * Hub Starting Point
            blocks['A'] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerInactive");
            srcOffset['A'] = new Rectangle(0, 0, Field.BLOCK_WIDTH, blocks['A'].Height);
            positionOffset['A'] = 0;

            // Diagonal Blocks
            // 1 bottom-right path
            blocks['1'] = Content.Load<Texture2D>(@"FieldBlocks\Block_GrassCorner");
            srcOffset['1'] = new Rectangle(0, 0, Field.BLOCK_WIDTH, Field.BLOCK_HEIGHT);
            positionOffset['1'] = 0;
            // 1 bottom-right path
            blocks['2'] = Content.Load<Texture2D>(@"FieldBlocks\Block_GrassCorner");
            srcOffset['2'] = new Rectangle(Field.BLOCK_WIDTH, 0, Field.BLOCK_WIDTH, Field.BLOCK_HEIGHT);
            positionOffset['2'] = 0;
            // 1 bottom-right path
            blocks['3'] = Content.Load<Texture2D>(@"FieldBlocks\Block_GrassCorner");
            srcOffset['3'] = new Rectangle(Field.BLOCK_WIDTH*2, 0, Field.BLOCK_WIDTH, Field.BLOCK_HEIGHT);
            positionOffset['3'] = 0;
            // 1 bottom-right path
            blocks['4'] = Content.Load<Texture2D>(@"FieldBlocks\Block_GrassCorner");
            srcOffset['4'] = new Rectangle(Field.BLOCK_WIDTH*3, 0, Field.BLOCK_WIDTH, Field.BLOCK_HEIGHT);
            positionOffset['4'] = 0;

            // Load other images
            Texture2D[] selector = new Texture2D[2];
            // 0 - SelectedBlock
            selector[0] = Content.Load<Texture2D>(@"FieldBlocks\Selector");
            selector[1] = Content.Load<Texture2D>(@"FieldBlocks\Deleter");

            field.InitializeBlocks(blocks, srcOffset, positionOffset, selector);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            // Load the DNA Content
            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            Texture2D[] SpawnerBase = new Texture2D[2];
            Texture2D[] SpawnerBaseEnd = new Texture2D[4];
            Texture2D[] DNATexture = new Texture2D[9];

            SpawnerBase[0] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerInactive");
            SpawnerBase[1] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerActive");

            SpawnerBaseEnd[0] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerInactiveLeft");
            SpawnerBaseEnd[1] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerActiveLeft");
            SpawnerBaseEnd[2] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerInactiveRight");
            SpawnerBaseEnd[3] = Content.Load<Texture2D>(@"FieldBlocks\SpawnerActiveRight");

            DNATexture[0] = Content.Load<Texture2D>(@"FieldBlocks\RedDNA");
            DNATexture[1] = Content.Load<Texture2D>(@"FieldBlocks\BlueDNA");
            DNATexture[2] = Content.Load<Texture2D>(@"FieldBlocks\SolarDNA");

            Spawner.InitializeTextures(SpawnerBase, SpawnerBaseEnd, DNATexture, 0);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            // Load the Derp Spawn
            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            DerpSpawn.InitializeTexture(Content.Load<Texture2D>(@"Derps\DerpSpawn"));

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            // Load the Derp Content
            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            Dictionary<String, Texture2D> baseTextures = new Dictionary<String, Texture2D>();
            Dictionary<String, SpriteInfo> baseOffsets = new Dictionary<String, SpriteInfo>();

            Dictionary<String, Texture2D> accessoryTextures = new Dictionary<String, Texture2D>();
            Dictionary<String, SpriteInfo> accessoryOffsets = new Dictionary<String, SpriteInfo>();

            // Epoch 0
            
            // Medium Sized
            baseTextures.Add("e0s1", Content.Load<Texture2D>(@"Derps\derp_e0_s1"));
            baseOffsets.Add("e0s1", new SpriteInfo(24, 24, 12, 17, 1));

            // Give the media to the Derp class. It will handle which ones to use.
            Derp.InitializeTextures(baseTextures, baseOffsets, accessoryTextures, accessoryOffsets);

            // Load the Attack Sprites
            Dictionary<String, Texture2D> attackTextures = new Dictionary<String, Texture2D>();
            Dictionary<String, SpriteInfo> attackInfo = new Dictionary<String, SpriteInfo>();

            // Small Close-Ranged Attacks
            attackTextures.Add("small-close", Content.Load<Texture2D>(@"Derps\derp_attack_small"));
            attackInfo.Add("small-close", new SpriteInfo(16, 16, 8, 8, 5));

            // Give the media to the DerpAttack class.
            DerpAttack.InitializeTextures(attackTextures, attackInfo);

            // Load the GUI components
            gui.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            gui.Update(gameTime);
            field.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            field.Draw(spriteBatch);
            gui.Draw(spriteBatch);
        }
    }
}
