using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.Framework
{
    /// <summary>
    /// Manage screen elements.
    /// Check for clicks/collisions and draw elements.
    /// </summary>
    class ScreenElementManager
    {
        /// <summary>
        /// The screen element that currently has the world focus.
        /// It's like the current celebrity of screen elements!
        /// </summary>
        private ScreenElement screenFocus = null;

        /// <summary>
        /// Sorted Set of all screen elements sorted in increasing order
        /// Used for drawing elements in reverse depth order
        /// </summary>
        private SortedSet<ScreenElement> elementsInc = new SortedSet<ScreenElement>(new SortInc());

        /// <summary>
        /// Sorted Set of all screen elements sorted in descending order
        /// Used for checking for hits or collision in depth order
        /// </summary>
        private SortedSet<ScreenElement> elementsDes = new SortedSet<ScreenElement>(new SortDes());

        /// <summary>
        /// Queue of elements to add or remove.
        /// Keep this instead of modifying the lists while enumerating them
        /// </summary>
        private Queue<KeyValuePair<ScreenElement, bool>> modifiedQueue = new Queue<KeyValuePair<ScreenElement, bool>>();

        /// <summary>
        /// Keeping track of who the mouse is currently over
        /// This is used for setting the IsHover Attribute
        /// </summary>
        private ScreenElement lastOver = null;
        private DateTime lastOverStartTime;

        /// <summary>
        /// Singleton Element Manager
        /// </summary>
        public static ScreenElementManager Instance
        {
            get
            {
                return ScreenElementManager.instance;
            }
        }

        /// <summary>
        /// Private field
        /// </summary>
        private static ScreenElementManager instance = new ScreenElementManager();

        /// <summary>
        /// Called by the GameScreen when checking the mouse's position.
        /// This will calculate all clicking on elements
        /// </summary>
        /// <returns>The screen element the mouse is over</returns>
        public ScreenElement CheckMouseState(Pair<int> mousePosition)
        {
            ScreenElement ret = null;

            foreach (var element in elementsDes)
            {
                if (!element.Enabled)
                {
                    continue;
                }

                // Check the mouse position
                if (ret == null && element.Clickable && element.IsPointHit(mousePosition))
                {
                    element.IsMouseOver = true;
                    ret = element;

                    if (lastOver == null || !this.lastOver.Equals(ret))
                    {
                        this.lastOverStartTime = DateTime.Now;
                    }

                    if (DateTime.Now.Subtract(this.lastOverStartTime).TotalMilliseconds > Constants.MOUSE_HOVER_MILLISECONDS)
                    {
                        element.IsMouseHovering = true;
                    }
                    else
                    {
                        element.IsMouseHovering = false;
                    }
                }
                else
                {
                    element.IsMouseOver = false;
                    element.IsMouseHovering = false;
                }
            }

            return this.lastOver = ret;
        }

        /// <summary>
        /// Update all Elements. This can be done in any order.
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateElements(GameTime gameTime)
        {
            this.UpdateModifiedQueue();

            // Update the elements in decreasing depth
            foreach (var element in elementsDes)
            {
                if (!element.Enabled)
                {
                    continue;
                }

                element.Update(gameTime);
            }
        }

        /// <summary>
        /// Draw the Elements in depth order
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void DrawElements(GameTime gameTime, SpriteBatch spriteBatch)
        {
            this.UpdateModifiedQueue();

            // Draw the elements in increasing depth
            foreach (var element in elementsInc)
            {
                if (!element.Enabled)
                {
                    continue;
                }

                element.Draw(gameTime, spriteBatch);
            }
        }

        /// <summary>
        /// Return the element at world position
        /// </summary>
        /// <param name="worldPosition">X,Y coordinate pair in WORLD POSITION</param>
        /// <returns>The top-most screen element at that position or else null</returns>
        public ScreenElement ClickableElementAt(Pair<int> worldPosition)
        {
            foreach (var element in elementsDes)
            {
                if (element.Enabled && element.Clickable && element.IsPointHit(worldPosition))
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary>
        /// There is a new challenger for screen focus! 
        /// Do the logic for giving focus to this new element.
        /// 
        /// Note: There should only be a single "Click" per update step.
        ///         So the "Neither" enumeration is added to the LeftOrRight type.
        ///         "Neither" clicks should fire focus changes, but not click changes.
        ///         There are still problems with this method. Be careful with inadvertant recursion!
        /// </summary>
        /// <param name="element">The new element to get focus</param>
        /// <param name="clickType">Left of right click</param>
        public void SetNewFocus(ScreenElement element, LeftOrRight clickType)
        {
            // If nothing new is getting focus, then remove focus
            if (element == null)
            {
                if (this.screenFocus != null)
                {
                    this.screenFocus.UnClicked(null);
                    this.screenFocus = null;
                }
                return;
            }

            // Trigger Clicked and UnClicked events
            if (this.screenFocus == null)
            {
                this.screenFocus = element;
                element.Clicked(Input.MouseWorldPosition, clickType);
            }
            else if (this.screenFocus.Equals(element))
            {
                element.Clicked(Input.MouseWorldPosition, clickType);
            }
            else
            {
                screenFocus.UnClicked(element);
                this.screenFocus = element;
                element.Clicked(Input.MouseWorldPosition, clickType);
            }
        }

        /// <summary>
        /// This is a scrubbing call when an element is being deleted.
        /// In case it has focus, make sure that power is handed down gracefully.
        /// 
        /// Awkward power voids where a de-referenced element has focus is awkward. 
        /// I know. Trust me.
        /// </summary>
        /// <param name="element">The element that is losing focus (if it actually has focus)</param>
        public void RemoveFocus(ScreenElement element)
        {
            if (element.Equals(this.screenFocus))
            {
                var parent = this.screenFocus.Parent;
                this.screenFocus.UnClicked(parent);
                this.screenFocus = parent;

                if (parent != null)
                {
                    parent.Clicked(Input.MousePosition, LeftOrRight.Neither);
                }
            }
        }

        // ///////////////////////////////////////////////
        // CRUD classes
        // edit: update has to be done by the element we 
        //       have no power to update its depth here
        // ///////////////////////////////////////////////

        /// <summary>
        /// Add a new Screen Element
        /// </summary>
        /// <param name="element"></param>
        public void Add(ScreenElement element)
        {
            lock (this.modifiedQueue)
            {
                this.modifiedQueue.Enqueue(new KeyValuePair<ScreenElement, bool>(element, true));
            }
        }

        /// <summary>
        /// Remove an element from the sorted lists
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public void Remove(ScreenElement element)
        {
            lock (this.modifiedQueue)
            {
                this.modifiedQueue.Enqueue(new KeyValuePair<ScreenElement, bool>(element, false));
            }
        }

        /// <summary>
        /// Update the requests from the modified queue before iterating.
        /// Add new elements, remove old elements
        /// </summary>
        private void UpdateModifiedQueue()
        {
            // Cycle through the modified queue
            // Add new elements, remove old elements
            lock (this.modifiedQueue)
            {
                while (this.modifiedQueue.Count > 0)
                {
                    var pair = this.modifiedQueue.Dequeue();

                    if (pair.Value)
                    {
                        this.elementsInc.Add(pair.Key);
                        this.elementsDes.Add(pair.Key);
                    }
                    else
                    {
                        this.elementsInc.Remove(pair.Key);
                        this.elementsDes.Remove(pair.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the given element exists in the manager
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool Exists(ScreenElement element)
        {
            return this.elementsInc.Contains(element) && this.elementsDes.Contains(element);
        }

        // ////////////////////////////////////////////////
        // Comparer classes to keep the elements sorted
        // ////////////////////////////////////////////////
        internal class SortInc : IComparer<ScreenElement>
        {
            /// <summary>
            /// Compare the two elements. Sort in Increasing order.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>negative number if x's depth is less than y's</returns>
            public int Compare(ScreenElement x, ScreenElement y)
            {
                int cDepth = x.Depth.CompareTo(y.Depth);

                if (cDepth != 0)
                {
                    return cDepth;
                }
                else
                {
                    return x.Guid.CompareTo(y.Guid);
                }
            }
        }

        internal class SortDes : IComparer<ScreenElement>
        {
            /// <summary>
            /// Compare the two elements. Sort in Decreasing order.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>negative number if x's depth is greater than y's</returns>
            public int Compare(ScreenElement x, ScreenElement y)
            {
                int cDepth = y.Depth.CompareTo(x.Depth);

                if (cDepth != 0)
                {
                    return cDepth;
                }
                else
                {
                    return x.Guid.CompareTo(y.Guid);
                }
            }
        }
    }
}
