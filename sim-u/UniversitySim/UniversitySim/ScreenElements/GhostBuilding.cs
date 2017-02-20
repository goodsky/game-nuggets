using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.Screens;
using UniversitySim.Utilities;

namespace UniversitySim.ScreenElements
{
    /// <summary>
    /// The translucent ghost of a building that pops up when construction is requested.
    /// This will float around on the cursor until 
    /// </summary>
    class GhostBuilding : GhostConstruction
    {
        /// <summary>
        /// Keeps track of if this is a valid building position
        /// </summary>
        private bool isValidBuilding;

        /// <summary>
        /// The building data for the ghost building
        /// </summary>
        BuildingData data { get { return this.basedata as BuildingData; } }

        /// <summary>
        /// Create a ghost building to build a new type of building
        /// </summary>
        /// <param name="data"></param>
        public GhostBuilding(BuildingData data) : base(data) 
        {
            this.Position = Input.IsoWorld.Clone();
        }

        /// <summary>
        /// Update this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!this.Position.Equals(Input.IsoWorld))
            {
                this.Position = Input.IsoWorld.Clone();

                // Make certain there are no collisions at this location
                this.isValidBuilding = true;
                var buildingIsoPosition = Geometry.ToIsometricGrid(this.Position);
                for (int y = 0; y < this.data.Footprint.Length; ++y)
                {
                    for (int x = 0; x < this.data.Footprint[y].Length; ++x)
                    {
                        if (this.data.Footprint[y][x])
                        {
                            int gridX = buildingIsoPosition.x + x - this.data.FootprintIndexOffsetX;
                            int gridY = buildingIsoPosition.y + y - this.data.FootprintIndexOffsetY;

                            if (CampusManager.Instance.IsBuildingAt(new Pair<int>(gridX, gridY)))
                            {
                                this.isValidBuilding = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (this.isValidBuilding)
            {
                spriteBatch.Draw(this.data.FootprintImage, new Rectangle(Input.IsoWorldX - Camera.Instance.x + this.data.FootprintOffsetX, Input.IsoWorldY - Camera.Instance.y + this.data.FootprintOffsetY, this.data.FootprintImage.Width, this.data.FootprintImage.Height), Color.Green * 0.75f);
            }
            else
            {
                spriteBatch.Draw(this.data.FootprintImage, new Rectangle(Input.IsoWorldX - Camera.Instance.x + this.data.FootprintOffsetX, Input.IsoWorldY - Camera.Instance.y + this.data.FootprintOffsetY, this.data.FootprintImage.Width, this.data.FootprintImage.Height), Color.Red * 0.75f);
            }

            spriteBatch.Draw(this.data.Image, new Rectangle(Input.IsoWorldX - Camera.Instance.x + this.data.ImageOffsetX, Input.IsoWorldY - Camera.Instance.y + this.data.ImageOffsetY, this.data.Image.Width, this.data.Image.Height), Color.White * 0.5f);
        }

        /// <summary>
        /// Create the actual building at this position
        /// </summary>
        /// <param name="mousePosition"></param>
        public override void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            base.Clicked(mousePosition, clickType);

            if (clickType == LeftOrRight.Left)
            {
                if (this.isValidBuilding)
                {
                    CampusManager.Instance.CreateBuilding(this.Position.Clone(), this.Size.Clone(), this.data);

                    // If shift is held then we can build multiple buildings
                    if (!Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && !Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                    {
                        GameScreen.BeginBuilding(null);
                    }
                }
            }
            else if (clickType == LeftOrRight.Right)
            {
                GameScreen.BeginBuilding(null);
            }
        }
    }
}
