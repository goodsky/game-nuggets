using Microsoft.Xna.Framework;
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
    /// These enumerations store which directions are adjacent to the path
    /// </summary>
    [Flags]
    public enum PathAdjacencies
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3
    }

    /// <summary>
    /// This is a Path. It is a type of campus element that updates what it looks like on the fly.
    /// Also peeps walk on me :(
    /// </summary>
    class Path : ScreenElement
    {
        // Path search arrays. Currently 4-sided adjacencies
        public static int[] dx = { -Constants.TILE_WIDTH / 2, Constants.TILE_WIDTH / 2, Constants.TILE_WIDTH / 2, -Constants.TILE_WIDTH / 2 };
        public static int[] dy = { -Constants.TILE_HEIGHT / 2, -Constants.TILE_HEIGHT / 2, Constants.TILE_HEIGHT / 2, Constants.TILE_HEIGHT / 2 };

        /// <summary>
        /// The path segment 
        /// </summary>
        private PathSegment segment;

        /// <summary>
        /// True when we need to recalculate adjacencies.
        /// It is set by an adjacent path being created or deleted.
        /// </summary>
        public bool DirtyPathBit { get; set; }

        /// <summary>
        /// Creates an instance of a path on the world map
        /// </summary>
        /// <param name="position">Position of the building</param>
        /// <param name="segment">Segment information</param>
        public Path(PathSegment segment) : base(segment.WorldPosition.y)
        {
            this.segment = segment;

            this.Clickable = true;
            this.Position = segment.WorldPosition;
            this.Size = new Pair<int>(Constants.TILE_WIDTH, Constants.TILE_HEIGHT);

            this.DirtyPathBit = true;
            DirtySurroundingBits();
        }

        /// <summary>
        /// Update this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (this.DirtyPathBit)
            {
                this.segment.CheckAdjacencies();
                this.DirtyPathBit = false;
            }
        }

        /// <summary>
        /// Draw the correct section of the path
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            this.segment.Draw(spriteBatch);
        }

        /// <summary>
        /// When a path is right-clicked we should delete it
        /// </summary>
        /// <param name="mousePosition">Position of the mouse while clicking me</param>
        /// <param name="clickType">The type of clicking going no</param>
        public override void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            base.Clicked(mousePosition, clickType);

            if (clickType == LeftOrRight.Right)
            {
                CampusManager.Instance.DeleteElement(this);
                this.DirtySurroundingBits();
                this.Delete();
            }
        }

        /// <summary>
        /// Non-Pixel perfect checking for paths
        /// </summary>
        /// <param name="check">The clicking position</param>
        /// <returns></returns>
        public override bool IsPointHit(Pair<int> check)
        {
            var grid1 = Geometry.ToIsometricGrid(check);
            var grid2 = Geometry.ToIsometricGrid(new Pair<int>(this.Position.x + Constants.TILE_WIDTH/2, this.Position.y + Constants.TILE_HEIGHT/2));
            return grid1.Equals(grid2);
        }

        /// <summary>
        /// Flip the dirty bit for all my neighbors
        /// </summary>
        private void DirtySurroundingBits()
        {
            for (int i = 0; i < Path.dx.Length; ++i)
            {
                int nx = this.segment.WorldPosition.x + Path.dx[i];
                int ny = this.segment.WorldPosition.y + Path.dy[i];

                if (nx < 0 || ny < 0 || nx > GameScreen.GameWidth || ny > GameScreen.GameHeight)
                    continue;

                var path = CampusManager.Instance.ElementAtWorldPosition(nx, ny) as Path;
                if (path != null)
                {
                    path.DirtyPathBit = true;
                }
            }
        }

        /// <summary>
        /// Convert this path to a string for printing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Path {0}- Position: {1}", this.Guid, this.Position);
        }
    }

    /// <summary>
    /// This is a helper class that stores information about paths.
    /// 
    /// NOTE: Paths are weird right now because they are technically just a building. But construction and choosing their image is special and not building-like.
    ///       So this helper class exists to interface and make paths run like buildings a bit easier.
    /// </summary>
    public class PathSegment
    {
        // My data
        public PathData data { get; private set; }

        // The position of this segment in world (used for distance calculation)
        public Pair<int> WorldPosition { get; private set; }

        // Flags enum of where paths are adjacent
        public PathAdjacencies Adjacencies;

        // Tells if the position for building is valid
        public bool IsValidSegment;

        /// <summary>
        /// Create a new path segment
        /// </summary>
        /// <param name="data">The building data</param>
        /// <param name="worldPosition">The position snapped to the isometric grid</param>
        public PathSegment(PathData data, Pair<int> worldPosition)
        {
            this.data = data;
            this.WorldPosition = worldPosition;

            var overlapElement = CampusManager.Instance.ElementAtWorldPosition(worldPosition.x, worldPosition.y);
            this.IsValidSegment = overlapElement == null || overlapElement is Path;
        }

        /// <summary>
        /// Find adjacencies around me
        /// </summary>
        public void CheckAdjacencies()
        {
            this.Adjacencies = PathAdjacencies.None;

            for (int i = 0; i < Path.dx.Length; ++i)
            {
                int nx = this.WorldPosition.x + Path.dx[i];
                int ny = this.WorldPosition.y + Path.dy[i];

                if (nx < 0 || ny < 0 || nx > GameScreen.GameWidth || ny > GameScreen.GameHeight)
                    continue;
                
                var element = CampusManager.Instance.ElementAtWorldPosition(nx, ny);
                if (element != null && element is Path)
                {
                    this.Adjacencies |= (PathAdjacencies)(1 << i);
                }
            }
        }

        /// <summary>
        /// Given a sprite batch and a destination rectangle (which should be the same size as the world isometric block)
        /// Draw the correct path onto the sprite batch
        /// </summary>
        /// <param name="spriteBatch"><Sprite batch to draw on/param>
        /// <param name="alpha">Alpha blending for transparency</param>
        public void Draw(SpriteBatch spriteBatch, float alpha = 1.0f)
        {
            Rectangle destination = new Rectangle(this.WorldPosition.x - Camera.Instance.x, this.WorldPosition.y - Camera.Instance.y, Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            Rectangle source = new Rectangle(0, 0, Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            spriteBatch.Draw(this.data.Image, destination, source, Color.White * alpha);

            for (int i = 0; i < Path.dx.Length; ++i)
            {
                if ((this.Adjacencies & (PathAdjacencies)(1 << i)) > 0)
                {
                    source = new Rectangle(0, (i + 1) * (Constants.TILE_HEIGHT-1), Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
                    spriteBatch.Draw(this.data.Image, destination, source, Color.White * alpha);
                }
            }
        }

        /// <summary>
        /// Draw a red or green cursor for when constructing this path segment
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="alpha"></param>
        public void DrawCursor(SpriteBatch spriteBatch, float alpha = 1.0f)
        {
            var overlapElement = CampusManager.Instance.ElementAtWorldPosition(this.WorldPosition.x, this.WorldPosition.y);
            this.IsValidSegment = overlapElement == null || overlapElement is Path;

            Rectangle destination = new Rectangle(this.WorldPosition.x - Camera.Instance.x, this.WorldPosition.y - Camera.Instance.y, Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            Rectangle source = new Rectangle(0, 0, Constants.TILE_WIDTH, Constants.TILE_HEIGHT);

            spriteBatch.Draw(GameScreen.WhiteCursor, destination, source, this.IsValidSegment ? Color.DarkGreen * 0.75f : Color.Red * 0.5f);
        }
    }
}
