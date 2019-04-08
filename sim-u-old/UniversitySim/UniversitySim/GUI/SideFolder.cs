using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.GUI.ScrollBars;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.GUI
{
    /// <summary>
    /// The main folder to the side of the screen
    /// </summary>
    class SideFolder : ScreenElement
    {
        public const int WIDTH = 177;

        // Folder base color
        private Texture2D folderTexture;
        private Rectangle folderTextureRect;

        // Folder buttons
        BuildTab buildTab;

        // The ScrollBar menu with items
        ScrollBar buildScrollBar;

        // Folder position control
        internal int folderPosition;

        // Speed the folder moves back and forth
        private const int moveSpeed = 8;

        /// <summary>
        /// Create the side folder
        /// Create side folder buttons from each building in the building catalog
        /// </summary>
        public SideFolder(CampusCatalog catalog) : base(Constants.GUI_DEPTH) 
        {
            this.Clickable = true;
            this.Type = ElementType.GUI;
            this.folderPosition = -WIDTH; 

            this.buildTab = new BuildTab(this);
            this.AddChild(this.buildTab);

            this.buildScrollBar = new ScrollBar(new Pair<int>(5 - WIDTH, 20), new Pair<int>(155, 440), int.MaxValue - 2);
            this.AddChild(this.buildScrollBar);

            foreach (var element in catalog.GetCatalog())
            {
                if (element.InToolbox)
                {
                    this.buildScrollBar.AddElement(
                        new SideFolderButton(
                            element,
                            new Pair<int>(155, 80),
                            this.buildScrollBar,
                            this.Depth + 1));
                }
            }
        }

        /// <summary>
        /// Load the side folder's content.
        /// </summary>
        /// <param name="contentMan"></param>
        public override void LoadContent(ContentManager contentMan)
        {
            this.folderTexture = contentMan.Load<Texture2D>(@"GUI\FolderBase");
            this.folderTextureRect = new Rectangle(this.folderPosition, 0, 177, Constants.WINDOW_HEIGHT);

            this.buildTab.LoadContent(contentMan);
            this.buildScrollBar.LoadContent(contentMan);
        }

        /// <summary>
        /// Update the folder's position depending on its state.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Check for movement
            if (this.HasFocus && this.folderPosition < 0)
            {
                this.folderPosition += moveSpeed;

                if (folderPosition > 0)
                {
                    this.folderPosition = 0;
                }

                this.buildScrollBar.SetPosition(this.folderPosition + 5, 20);
            }
            else if (!this.HasFocus && this.folderPosition > -WIDTH)
            {
                this.folderPosition -= moveSpeed;

                if (this.folderPosition < -WIDTH)
                {
                    this.folderPosition = -WIDTH;
                }

                this.buildScrollBar.SetPosition(this.folderPosition + 5, 20);
            }

            this.folderTextureRect.X = this.folderPosition;
            this.buildTab.buildTabRect.X = this.folderPosition + WIDTH;
        }

        /// <summary>
        /// Draw the folder!
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.folderTexture, this.folderTextureRect, Color.White);
        }

        /// <summary>
        /// Check if the mouse is inside the GUI position
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public override bool IsPointHit(Pair<int> check)
        {
            return Utilities.Geometry.IsInRectangle(check, new Pair<int>(Camera.Instance.x, Camera.Instance.y), this.folderTextureRect);
        }

        // --------------------------------------------------------------------------------------
        // BUILD TABS
        // --------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        internal class BuildTab : ScreenElement
        {
            // Building folder tab
            private Texture2D buildTab;
            internal Rectangle buildTabRect;

            // Parent FolderTab
            SideFolder parent;

            /// <summary>
            /// Construct yo
            /// </summary>
            internal BuildTab(SideFolder parent) : base(parent.Depth) 
            {
                this.parent = parent;
                this.Clickable = true;
                this.Type = ElementType.GUI;
            }

            /// <summary>
            /// Load the side folder's content.
            /// </summary>
            /// <param name="contentMan"></param>
            public override void LoadContent(ContentManager contentMan)
            {
                this.buildTab = contentMan.Load<Texture2D>(@"GUI\BuildTab");
                this.buildTabRect = new Rectangle(this.parent.folderPosition + WIDTH, 10, 45, 52);
            }

            /// <summary>
            /// The parent folder controls the Update method for this tab.
            /// This was done originally to allow focus to remain in the parent object when this button is clicked.
            /// </summary>
            /// <param name="gameTime"></param>
            public override void Update(GameTime gameTime) { }

            /// <summary>
            /// Draw the folder!
            /// </summary>
            /// <param name="gameTime"></param>
            /// <param name="spriteBatch"></param>
            public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(this.buildTab, this.buildTabRect, Color.White);
            }

            /// <summary>
            /// Check if the mouse is inside the GUI position
            /// </summary>
            /// <param name="check"></param>
            /// <returns></returns>
            public override bool IsPointHit(Pair<int> check)
            {
                return Utilities.Geometry.IsInRectangle(check, new Pair<int>(Camera.Instance.x, Camera.Instance.y), this.buildTabRect);
            }
        }
    }
}
