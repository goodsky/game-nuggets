using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.GUI.ScrollBars
{
    /// <summary>
    /// This GUI class is used to create a scroll bar of tabs anywhere
    /// It has customizable size and any number of elements
    /// </summary>
    class ScrollBar : ScreenElement
    {
        // constant size of the scroll bar buttons and width of the bar
        private readonly int scrollWidth = 12;

        // The scroll base textures
        private static Texture2D ScrollBaseImage = null;
        private static Rectangle srcTopRect;
        private static Rectangle srcMidRect;
        private static Rectangle srcBotRect;

        // The scroll bar textures
        private static Texture2D ScrollBarImage = null;

        // The actual scroll
        public Scroll scroll;

        // The scroll elements
        private List<ScrollElement> elements;

        // Height of all the scroll elements combined
        private int elementsHeight = 0;

        // The scroll bar position
        public int ScrollPosition
        {
            get
            {
                return this.scroll.ScrollBarY;
            }
        }

        /// <summary>
        /// Create an empty instance of a scroll bar
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public ScrollBar(Pair<int> position, Pair<int> size, int depth) : base(depth)
        {
            this.Position = position;
            this.Size = size;

            this.elements = new List<ScrollElement>();

            this.scroll = new Scroll(this, depth);
            this.AddChild(this.scroll);
        }

        /// <summary>
        /// Load references to the scroll bar. Only do this once per scroll bar however.
        /// </summary>
        /// <param name="contentMan"></param>
        public override void LoadContent(ContentManager contentMan)
        {
            this.scroll.LoadContent(contentMan);

            foreach (var element in this.elements)
            {
                element.LoadContent(contentMan);
            }
        }

        /// <summary>
        /// Update the scroll bar. Allow the scroll bar to be moved by mouse wheel.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) 
        {
            if (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.Position.x, this.Position.y, this.Size.x, this.Size.y) && Input.MouseWheelMove() != 0)
            {
                this.SetScrollHeight(this.scroll.ScrollBarY + (Input.MouseWheelMove() / 10));
            }
        }

        /// <summary>
        /// Draw the scroll bar
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }

        /// <summary>
        /// Set the position of the Scroll Bar in screen units
        /// Also updates all the child elements.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPosition(int x, int y)
        {
            this.Position.x = x;
            this.Position.y = y;

            this.RecalculatePosition();
        }

        /// <summary>
        /// Set the scroll bar to a certain absolute height
        /// Also updates all the child elements
        /// </summary>
        /// <param name="height"></param>
        public void SetScrollHeight(int height)
        {
            this.scroll.ScrollBarY = height;

            this.RecalculatePosition();
        }

        /// <summary>
        /// Add to list
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(ScrollElement element)
        {
            this.elements.Add(element);
            this.AddChild(element);

            this.RecalculateSize();
            this.RecalculatePosition();
        }

        /// <summary>
        /// Remove from list
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(ScrollElement element)
        {
            this.elements.Remove(element);

            this.RecalculateSize();
        }

        /// <summary>
        /// Call this after any change to the size or number of elements
        /// </summary>
        private void RecalculateSize()
        {
            // Check total size of elements in the list
            this.elementsHeight = 0;
            foreach (var element in this.elements)
            {
                this.elementsHeight += element.Size.y;
            }

            // Check if we actually need to draw the scroll bar
            if (this.elementsHeight > this.Size.y)
            {
                this.scroll.ShowScrollBar = true;

                this.scroll.ScrollScale = (this.Size.y - this.scrollWidth * 2) / (double)this.elementsHeight;
                this.scroll.ScrollBarHeight = (int)((this.Size.y - this.scrollWidth * 2) * this.scroll.ScrollScale);
                this.scroll.ScrollBarY = this.scroll.ScrollBarY; // refilter the position to makes sure it's still valid
            }
            else
            {
                this.scroll.ShowScrollBar = false;
            }
        }
        
        /// <summary>
        /// Call this after any change to the position of the entire element of the scroll bar
        /// </summary>
        private void RecalculatePosition()
        {
            // Update the position of the scroll bar and all sub-elements
            this.scroll.UpdateDestRectangles();
            int heightSum = 0;
            foreach (var element in this.elements)
            {
                element.Position.y = this.Position.y + heightSum - this.scroll.ScrollBarY;
                element.Position.x = this.Position.x;
                heightSum += element.Size.y;
            }
        }

        /// <summary>
        /// The actual scroll bar
        /// </summary>
        internal class Scroll : ScreenElement
        {
            // The parent scroll bar
            private ScrollBar parent;

            // Tell if the bar has been clicked and is being updated
            private bool wasClickedOn = false;
            private bool isScrolling = false;

            // The position you clicked the bar when you started scrolling
            private int isScrollingOffset;

            // Underlying field for the property wrapper
            private int scrollBarY = 0;

            // Rectangles for drawing
            private Rectangle destTopRect;
            private Rectangle destMidRect;
            private Rectangle destBotRect;
            private Rectangle destBarRect;

            // If the scroll bar should be shown. It is only shown if there are enough elements for it to be necessary.
            public bool ShowScrollBar = false;

            // The scale between the ScrollBarY (which is in world units) and the Y position we need to render the actual bar at
            public double ScrollScale { get; set; }

            // The height of the scroll bar
            public int ScrollBarHeight { get; set; }

            // The Y position of the scroll bar
            public int ScrollBarY
            {
                get
                {
                    return this.scrollBarY;
                }
                set
                {
                    if (!this.ShowScrollBar)
                    {
                        return;
                    }

                    if (value < 0)
                    {
                        this.scrollBarY = 0;
                    }
                    else if ((value * this.ScrollScale) + this.ScrollBarHeight  > this.parent.Size.y - this.parent.scrollWidth * 2)
                    {
                        this.scrollBarY = (int)((this.parent.Size.y - this.parent.scrollWidth * 2 - this.ScrollBarHeight) / this.ScrollScale);
                    }
                    else
                    {
                        this.scrollBarY = value;
                    }
                }
            }

            /// <summary>
            /// The actual scroll bar on the right hand side of the bar
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="depth"></param>
            public Scroll(ScrollBar parent, int depth) : base(depth)
            {
                this.Clickable = true;
                this.Type = ElementType.GUI;

                this.parent = parent;
                this.UpdateDestRectangles();
            }

            /// <summary>
            /// Load the scroll bar images
            /// </summary>
            /// <param name="contentMan"></param>
            public override void LoadContent(ContentManager contentMan)
            {
                if (ScrollBarImage == null)
                {
                    ScrollBarImage = contentMan.Load<Texture2D>(@"GUI\ScrollBar");
                }

                if (ScrollBaseImage == null)
                {
                    ScrollBaseImage = contentMan.Load<Texture2D>(@"GUI\ScrollBase");
                    srcTopRect = new Rectangle(0, 0, this.parent.scrollWidth, this.parent.scrollWidth);
                    srcMidRect = new Rectangle(0, this.parent.scrollWidth, this.parent.scrollWidth, 1);
                    srcBotRect = new Rectangle(0, this.parent.scrollWidth + 1, this.parent.scrollWidth, this.parent.scrollWidth);
                }
            }

            /// <summary>
            /// Update the destination rectangles of the scroll bar
            /// </summary>
            public void UpdateDestRectangles()
            {
                this.destTopRect = new Rectangle(this.parent.Position.x + this.parent.Size.x, this.parent.Position.y, this.parent.scrollWidth, this.parent.scrollWidth);
                this.destMidRect = new Rectangle(this.parent.Position.x + this.parent.Size.x, this.parent.Position.y + this.parent.scrollWidth, this.parent.scrollWidth, this.parent.Size.y - this.parent.scrollWidth * 2);
                this.destBotRect = new Rectangle(this.parent.Position.x + this.parent.Size.x, this.parent.Position.y + this.parent.Size.y - this.parent.scrollWidth, this.parent.scrollWidth, this.parent.scrollWidth);

                this.destBarRect = new Rectangle(this.parent.Position.x + this.parent.Size.x, this.parent.Position.y + this.parent.scrollWidth + (int)(this.ScrollBarY * this.ScrollScale), this.parent.scrollWidth, this.ScrollBarHeight);
            }

            // Check a click and drag for the position update
            public override void Update(GameTime gameTime)
            {
                // click and hold on the scroll bar ends
                if (this.ShowScrollBar)
                {
                    if (Input.MouseLeftKeyClicked())
                    {
                        if (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destTopRect) ||
                             Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destMidRect) ||
                             Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destBotRect))
                        {
                            // give artificial focus without the click event
                            this.wasClickedOn = true;
                        }

                        if (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destBarRect))
                        {
                            this.isScrolling = true;
                            this.isScrollingOffset = Input.MousePosition.y - this.destBarRect.Y;
                        }
                    }
                    else if (Input.MouseLeftKeyDown() && this.wasClickedOn)
                    {
                        if (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destTopRect))
                        {
                            this.parent.SetScrollHeight(this.parent.ScrollPosition - Constants.SCROLLBAR_CLICK_SPEED);
                        }
                        else if (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destBotRect))
                        {
                            this.parent.SetScrollHeight(this.parent.ScrollPosition + Constants.SCROLLBAR_CLICK_SPEED);
                        }
                        else if (this.isScrolling)
                        {
                            this.parent.SetScrollHeight((int)((Input.MousePosition.y - this.parent.Position.y - this.parent.scrollWidth - this.isScrollingOffset) / this.ScrollScale));
                        }
                        // clicked above or below the bar
                        else if (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destMidRect)) 
                        {
                            if (Input.MousePosition.y < this.destBarRect.Y)
                            {
                                this.parent.SetScrollHeight(this.parent.ScrollPosition - Constants.SCROLLBAR_CLICK_SPEED);
                            }

                            if (Input.MousePosition.y > this.destBarRect.Y + this.destBarRect.Height)
                            {
                                this.parent.SetScrollHeight(this.parent.ScrollPosition + Constants.SCROLLBAR_CLICK_SPEED);
                            }
                        }
                    }
                    else
                    {
                        this.isScrolling = false;
                        this.wasClickedOn = false;
                    }
                }
            }

            /// <summary>
            /// Draw the scroll bar if necessary
            /// </summary>
            /// <param name="gameTime"></param>
            /// <param name="spriteBatch"></param>
            public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
            {
                if (this.ShowScrollBar)
                {
                    spriteBatch.Draw(ScrollBaseImage, this.destTopRect, srcTopRect, Color.White);
                    spriteBatch.Draw(ScrollBaseImage, this.destMidRect, srcMidRect, Color.White);
                    spriteBatch.Draw(ScrollBaseImage, this.destBotRect, srcBotRect, Color.White);

                    spriteBatch.Draw(ScrollBarImage, this.destBarRect, Color.White);
                }
            }

            /// <summary>
            /// Check for a hit anywhere along the scroll bar's position
            /// </summary>
            /// <param name="check"></param>
            /// <returns></returns>
            public override bool IsPointHit(Pair<int> check)
            {
                return this.ShowScrollBar &&
                    (Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destTopRect) ||
                     Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destMidRect) ||
                     Utilities.Geometry.IsInRectangle(Input.MousePosition, this.destBotRect));
            }
        }
    }
}
