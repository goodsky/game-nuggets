using System.Collections.Generic;

namespace UI
{
    public abstract class Window : Selectable
    {
        /// <summary>Window buttons. Allows UI Factories to set the color scheme.</summary>
        public abstract List<Button> Buttons { get; }

        /// <summary>
        /// Open the window and display the data.
        /// </summary>
        /// <param name="data">The data backing this window.</param>
        public abstract void Open(object data);
    }
}
