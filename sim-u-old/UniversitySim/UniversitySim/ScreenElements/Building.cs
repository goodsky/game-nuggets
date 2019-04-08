using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.Utilities;

namespace UniversitySim.ScreenElements
{
    /// <summary>
    /// This is a building. It has a location, and stats.
    /// It creates the backbone of the university state.
    /// </summary>
    class Building : ScreenElement
    {
        /// <summary>
        /// The data about this particular building
        /// </summary>
        protected BuildingData data;

        /// <summary>
        /// The button for deleting this building
        /// </summary>
        private DeleteBuildingButton deleteButton;

        /// <summary>
        /// The button for editing this building
        /// </summary>
        private EditBuildingButton editButton;

        /// <summary>
        /// Gets the name of this building according to the BuildingData
        /// </summary>
        public string Name { get { return this.data.Name; } }

        /// <summary>
        /// Gets the footprint boolean array of this building
        /// </summary>
        public bool[][] Footprint { get { return this.data.Footprint; } }

        /// <summary>
        /// Gets the footprint index offset to the center of the building
        /// </summary>
        public Pair<int> FootprintIndexOffsets { get { return new Pair<int>(this.data.FootprintIndexOffsetX, this.data.FootprintIndexOffsetY); } }

        /// <summary>
        /// Create an instance of a building ScreenElement
        /// </summary>
        public Building(Pair<int> position, Pair<int> size, BuildingData data) : base(position.y)
        {
            this.Clickable = true;
            this.Position = position;
            this.Size = size;
            this.data = data;

            this.deleteButton = new DeleteBuildingButton(this);
            this.deleteButton.Position = new Pair<int>(this.Position.x + this.Size.x + this.data.ImageOffsetX - (2 * this.deleteButton.Size.x / 3), this.Position.y + this.Size.y + this.data.ImageOffsetY - this.deleteButton.Size.y);
            this.deleteButton.Enabled = false;

            this.editButton = new EditBuildingButton(this);
            this.editButton.Position = new Pair<int>(this.Position.x + this.data.ImageOffsetX - (this.deleteButton.Size.x / 3), this.Position.y + this.Size.y + this.data.ImageOffsetY - this.deleteButton.Size.y);
            this.editButton.Enabled = false;
        }

        /// <summary>
        /// Update this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// Draw this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // If the building is within the screen, then draw it
            if (Geometry.IsOnScreen(this))
            {
                spriteBatch.Draw(this.data.Image, 
                    new Rectangle(this.Position.x - Camera.Instance.x + this.data.ImageOffsetX, this.Position.y - Camera.Instance.y + this.data.ImageOffsetY, this.Size.x, this.Size.y), 
                    Color.White);
            }
        }

        /// <summary>
        /// Collision checking for a building
        /// Pixel-perfect clicking-collision
        /// </summary>
        /// <param name="check">The clicking position</param>
        /// <returns></returns>
        public override bool IsPointHit(Pair<int> check)
        {
            int imageX = check.x - this.Position.x - this.data.ImageOffsetX;
            int imageY = check.y - this.Position.y - this.data.ImageOffsetY;

            if (imageX >= 0 && imageX < this.data.Image.Width && imageY >= 0 && imageY < this.data.Image.Height)
            {
                return this.data.ImageData[imageX + imageY * data.Image.Width] != Color.Transparent;
            }

            return false;
        }

        /// <summary>
        /// When a building is clicked we should bring up two buttons
        /// 1) Edit building properties box
        /// 2) Delete building
        /// </summary>
        /// <param name="mousePosition">Position of the mouse while clicking me</param>
        /// <param name="clickType">The type of clicking going no</param>
        public override void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            this.editButton.Enabled = true;
            this.deleteButton.Enabled = true;

            base.Clicked(mousePosition, clickType);
        }

        /// <summary>
        /// When a different building gains focus close edit and delete buttons
        /// </summary>
        /// <param name="other"></param>
        public override void UnClicked(ScreenElement other)
        {
            this.editButton.Enabled = false;
            this.deleteButton.Enabled = false;

            base.UnClicked(other);
        }

        /// <summary>
        /// Be sure to delete my children when deleting me
        /// </summary>
        public override void Delete()
        {
            this.editButton.Delete();
            this.deleteButton.Delete();

            base.Delete();
        }

        /// <summary>
        /// Convert this building to a string for printing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Building {0}- Name: {1} Position: {2}", this.Guid, this.Name, this.Position);
        }
    }

    /// <summary>
    /// The button that pops up when you gain focus on a building that lets you edit its settings
    /// </summary>
    class EditBuildingButton : ScreenElement
    {
        /// <summary>
        /// The image for this button
        /// </summary>
        Texture2D image;

        /// <summary>
        /// Default constructor for this button
        /// </summary>
        public EditBuildingButton(Building parent) : base(Constants.GUI_DEPTH - 1)
        {
            this.image = ImageCatalog.Instance.Get("ScreenElements/EditBuildingButton");
            this.Size = new Pair<int>(this.image.Width, this.image.Height);
            this.Clickable = true;

            parent.AddChild(this);
        }

        /// <summary>
        /// Don't do anything when updating
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) { }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.image,
                    new Rectangle(this.Position.x - Camera.Instance.x, this.Position.y - Camera.Instance.y, this.Size.x, this.Size.y),
                    Color.White);
        }
    }

    /// <summary>
    /// The button that pops up when you gain focus on a building that lets you delete the building
    /// </summary>
    class DeleteBuildingButton : ScreenElement
    {
        /// <summary>
        /// The image for this button
        /// </summary>
        Texture2D image;

        /// <summary>
        /// Default constructor for this button
        /// </summary>
        public DeleteBuildingButton(Building parent) : base(Constants.GUI_DEPTH - 1)
        {
            this.image = ImageCatalog.Instance.Get("ScreenElements/DeleteBuildingButton");
            this.Size = new Pair<int>(this.image.Width, this.image.Height);
            this.Clickable = true;

            parent.AddChild(this);
        }

        /// <summary>
        /// Don't do anything when updating
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) { }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.image,
                    new Rectangle(this.Position.x - Camera.Instance.x, this.Position.y - Camera.Instance.y, this.Size.x, this.Size.y),
                    Color.White);
        }

        /// <summary>
        /// Delete the parent.
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <param name="clickType"></param>
        public override void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            CampusManager.Instance.DeleteElement((Building)this.Parent);
            this.Parent.Delete();
            base.Clicked(mousePosition, clickType);
        }
    }
}
