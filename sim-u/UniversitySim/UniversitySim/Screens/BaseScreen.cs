using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim.Screens
{
    /// <summary>
    /// Super class for the screens
    /// </summary>
    public abstract class BaseScreen
    {
        // Transition Style
        public TransitionType TransitionIn = TransitionType.Instant;
        public TransitionType TransitionOut = TransitionType.Instant;

        // Time (in seconds) transitions will take
        public double TransitionInTime = 0.25;
        public double TransitionTime = 0.25;

        // Whether the user can control the screen during transition
        public bool ControlTransIn = false;
        public bool ControlTransOut = false;

        // virtual methods that will be overloaded by actual screens
        public virtual void Initialize() { }
        public virtual void LoadContent(ContentManager contentMan) { }
        public virtual void UnloadContent() { }
        public virtual void Update(GameTime gametime) { }
        public virtual void Draw(GameTime gametime, SpriteBatch spriteBatch) { }
    }
}
