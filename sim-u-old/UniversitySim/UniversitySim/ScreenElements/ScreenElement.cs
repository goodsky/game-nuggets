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
    /// Types of elements that can be clicked or collided with.
    /// This could become an interface that types inherit if 
    /// we decide that different elements have different method signatures
    /// </summary>
    public enum ElementType
    {
        Other,
        GUI,
        Building,
        Derp
    }

    /// <summary>
    /// Base class for any element that will be drawn on the screen or can be clicked.
    /// </summary>
    public abstract class ScreenElement
    {
        /// <summary>
        /// GUIDs are generated for each screen element to be used for Equals checks
        /// </summary>
        protected static long LastGUID = 0;

        /// <summary>
        /// The ID for this screen element
        /// </summary>
        private long guid = LastGUID++;

        /// <summary>
        /// The depth of the element.
        /// WARNING: updating this private value will NOT automatically update the sorted list.
        /// Use the public property to modify depth.
        /// </summary>
        private int depth;

        /// <summary>
        /// The children and parents of this ScreenElement. This is used when clicks are made, if a child is clicked then all of its parents also keep focus.
        /// </summary>
        public ScreenElement Parent { get; private set; }
        private List<ScreenElement> children = new List<ScreenElement>();

        /// <summary>
        /// The type of this element. e.g. GUI element, building, etc.
        /// </summary>
        public ElementType Type { get; set; }

        /// <summary>
        /// Set this value to make the element visible to collision checks
        /// (collision = game elements interacting with game elements)
        /// </summary>
        public bool Collidable { get; set; }

        /// <summary>
        /// Set this value to make the element visible to click checks
        /// (click = user clicking on the screen)
        /// </summary>
        public bool Clickable { get; set; }

        /// <summary>
        /// This value is set by the ScreenElementManager.
        /// Don't modify this!!!
        /// </summary>
        public bool IsMouseOver { get; set; }

        /// <summary>
        /// If the mouse has hovered over this element for the accepted duration.
        /// This value is set by the ScreenElementManager.
        /// Don't modify this!!!
        /// </summary>
        public bool IsMouseHovering { get; set; }

        /// <summary>
        /// Use this value to determine if this element has screen focus. i.e. has been clicked on.
        /// </summary>
        public bool HasFocus { get; set; }

        /// <summary>
        /// This value can enable or disable an element. Elements are by default enabled.
        /// Disabled elements are not drawn or interactable. Basically I skip them in the ScreenElementManager.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The Draw / HitBox depth of this Screen Element.
        /// Updating this value will update the position of the element in the manager's list.
        /// Try to update this as little as possible.
        /// </summary>
        public int Depth
        {
            get
            {
                return this.depth;
            }
            set
            {
                if (value != this.depth)
                {
                    ScreenElementManager.Instance.Remove(this);
                    this.depth = value;
                    ScreenElementManager.Instance.Add(this);
                }
            }
        }

        /// <summary>
        /// Unique ID for this Screen Element.
        /// Breaks depth ties when sorting.
        /// </summary>
        public long Guid
        {
            get
            {
                return this.guid;
            }
        }

        /// <summary>
        /// X, Y pair for this element's position on the screen.
        /// This position is in WORLD UNITS
        /// </summary>
        public Pair<int> Position { get; set; }

        /// <summary>
        /// Width, Height pair for this elements hitbox
        /// Note: This is not necessarily the same thing as the collision area.
        /// </summary>
        public Pair<int> Size { get; set; }

        /// <summary>
        /// Start this screen element inside the Manager's List.
        /// </summary>
        /// <param name="depth"></param>
        public ScreenElement(int depth)
        {
            this.depth = depth;
            this.Enabled = true;
            ScreenElementManager.Instance.Add(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ScreenElement()
        {
            this.Delete();
        }

        /// <summary>
        /// Add a child to a screen element. These two elements will be considered the same for screen focus and collision.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(ScreenElement child)
        {
            this.children.Add(child);
            child.Parent = this;
        }

        /// <summary>
        /// Remove this element from the screen and any families. It's like it never existed :'(
        /// </summary>
        public virtual void Delete()
        {
            ScreenElementManager.Instance.RemoveFocus(this);

            if (this.Parent != null)
            {
                this.Parent.children.Remove(this);
            }
            this.Parent = null; // I have no parents :( just like batman

            if (ScreenElementManager.Instance.Exists(this))
            {
                ScreenElementManager.Instance.Remove(this);
            }
        }

        /// <summary>
        /// Load any content needed for this element.
        /// This method is only virtual, not abstract, because some elements will not need to Load Context.
        /// E.g. building screen elements.
        /// </summary>
        /// <param name="contentMan"></param>
        public virtual void LoadContent(ContentManager contentMan) { }

        /// <summary>
        /// Update this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Draw this Game Element
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        /// <summary>
        /// This method is called if the element is clickable and has been clicked.
        /// It will be executed before the Update call on the tick.
        /// </summary>
        /// <param name="mousePosition">Mouse world position that has been clicked</param>
        /// <param name="clickType">Tells if the click was with left or right mouse key</param>
        public virtual void Clicked(Pair<int> mousePosition, LeftOrRight clickType)
        {
            if (clickType == LeftOrRight.Left || clickType == LeftOrRight.Neither)
            {
                this.GiveFocus();
            }
        }

        /// <summary>
        /// This method will be called if the element previously had focus, but focus has been shifted.
        /// </summary>
        /// <param name="other">The new screen element that has focus</param>
        public virtual void UnClicked(ScreenElement other)
        {
            this.TakeFocus();
        }

        /// <summary>
        /// Check for a hit over this element.
        /// Default method returns if the point is in the hitbox.
        /// Can be overriden for more specific collision checks.
        /// 
        /// Assume GUI elements are screen-relative in their position
        /// and other elements are world-units in the position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <returns></returns>
        public virtual bool IsPointHit(Pair<int> check)
        {
            bool ret;
            if (this.Type == ElementType.GUI)
            {
                ret = Utilities.Geometry.IsInRectangle(check, new Pair<int>(Camera.Instance.x, Camera.Instance.y), this.Position.x, this.Position.y, this.Size.x, this.Size.y);
            }
            else
            {
                ret = Utilities.Geometry.IsInRectangle(check, this.Position.x, this.Position.y, this.Size.x, this.Size.y);
            }

            return ret;
        }

        /// <summary>
        /// Compare by just guid.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScreenElement other)
        {
            return other == null ? false : this.guid.Equals(other.guid);
        }

        /// <summary>
        /// Convert this element to a string for printing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ScreenElement {0}", this.guid);
        }

        // -------------------------------------------------------------------------
        // private methods (shhhhhh)
        // -------------------------------------------------------------------------

        /// <summary>
        /// Give the focus up the parental tree
        /// </summary>
        private void GiveFocus()
        {
            this.HasFocus = true;

            if (this.Parent != null)
            {
                this.Parent.GiveFocus();
            }
        }

        /// <summary>
        /// Take the focus from the parental tree.
        /// </summary>
        /// <returns></returns>
        private void TakeFocus()
        {
            this.HasFocus = false;

            if (this.Parent != null)
            {
                this.Parent.TakeFocus();
            }
        }
    }
}
