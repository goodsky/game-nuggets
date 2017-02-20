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
    /// The ghost of the path that will help you draw your paths on the screen
    /// 
    /// NOTE: on 12/6/2014 I decided to not render the path while placing it, instead we will draw a green or red highlight
    ///         to let you know if the position is valid. This also makes the "dynamic updating" for path images less of a headache,
    ///         since we only need to update the adjacencies when construction is confirmed.
    /// </summary>
    class GhostPath : GhostConstruction
    {
        /// <summary>
        /// The path data for the ghost path
        /// </summary>
        PathData data { get { return this.basedata as PathData; } }

        // Where the click started
        // NOTE: the position is snapped to the isometric grid but in world units
        Pair<int> startClickLocation;

        // The list of segments that make up the path
        List<PathSegment> segments;

        // Checks if all the segments are green
        public bool segmentsValid;

        // Just draw a dummy cursor so we know we are building something
        PathSegment ghostCursor;

        /// <summary>
        /// Create a ghost path to build a new section of path
        /// </summary>
        /// <param name="data"></param>
        public GhostPath(PathData data) : base(data) 
        {
            this.startClickLocation = null;
            this.Position = Input.IsoWorld.Clone();

            this.segments = new List<PathSegment>();
            this.ghostCursor = new PathSegment(this.data, Input.IsoWorld);

            this.Position = Input.IsoWorld.Clone();
        }

        /// <summary>
        /// Update this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Grab the start of a click
            if (Input.MouseLeftKeyClicked())
            {
                this.startClickLocation = Input.IsoWorld.Clone();
                this.segments = this.GetPathSegments();
            }

            // Update the segments if we have moved position out of the last grid
            if (!this.Position.Equals(Input.IsoWorld))
            {
                this.Position = Input.IsoWorld.Clone();

                if (this.startClickLocation != null)
                {
                    if (Input.MouseLeftKeyDown())
                    {
                        this.segments = this.GetPathSegments();
                    }
                }
            }

            // Key released
            if (this.startClickLocation != null && !Input.MouseLeftKeyDown())
            {
                if (this.segments.Count > 0 && this.segmentsValid)
                {
                    CampusManager.Instance.CreatePath(this.segments);

                    // If shift is held then we can build multiple buildings
                    if (!Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && !Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                    {
                        GameScreen.BeginBuilding(null);
                    }
                }

                this.startClickLocation = null;
                this.segments = new List<PathSegment>();
            }
        }

        /// <summary>
        /// Create the actual building at this position
        /// </summary>
        /// <param name="mousePosition"></param>
        public override void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            base.Clicked(mousePosition, clickType);

            if (clickType == LeftOrRight.Right)
            {
                if (this.startClickLocation != null)
                {
                    this.startClickLocation = null;
                    this.segments = new List<PathSegment>();
                }
                else
                {
                    GameScreen.BeginBuilding(null);
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
            this.ghostCursor.DrawCursor(spriteBatch, 0.5f);

            foreach (var segment in this.segments)
            {
                segment.DrawCursor(spriteBatch, 0.5f);
            }
        }

        /// <summary>
        /// Get the list of path segments between two points
        /// This is not fast, it may need to be optimized later.
        /// </summary>
        /// <returns></returns>
        private List<PathSegment> GetPathSegments()
        {
            List<PathSegment> ret = new List<PathSegment>();
            var curSeg = new PathSegment(this.data, Input.IsoWorld.Clone());
            ret.Add(curSeg);

            this.segmentsValid = curSeg.IsValidSegment;

            if (Math.Abs(curSeg.WorldPosition.x - this.startClickLocation.x) % Path.dx[2] != 0 || Math.Abs(curSeg.WorldPosition.y - this.startClickLocation.y) % Path.dy[1] != 0)
            {
                Logger.Log(LogLevel.Error, "Bad snapping coordinates", string.Format("Very bad. Somehow the coordinates are not snapped to a grid! {0} -> {1}", curSeg.WorldPosition, this.startClickLocation));
            }

            while (!curSeg.WorldPosition.Equals(this.startClickLocation))
            {
                PathSegment nextSegment = null;
                double currentDist = Double.MaxValue; // Geometry.Dist(curSeg.WorldPosition, this.startClickLocation);

                for (int i = 0; i < Path.dx.Length; ++i)
                {
                    var newGridPosition = new Pair<int>(curSeg.WorldPosition.x + Path.dx[i], curSeg.WorldPosition.y + Path.dy[i]);
                    var newGridDist = Geometry.Dist(newGridPosition, this.startClickLocation);

                    if (newGridDist < currentDist)
                    {
                        nextSegment = new PathSegment(this.data, newGridPosition);
                        currentDist = newGridDist;
                    }
                }

                curSeg = nextSegment;
                ret.Add(curSeg);

                this.segmentsValid = this.segmentsValid && curSeg.IsValidSegment;
            }

            return ret;
        }
    }
}
