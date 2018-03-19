using UnityEngine;

namespace UI
{
    public abstract class Window : Selectable
    {
        /// <summary>
        /// Open the window and display the data.
        /// </summary>
        /// <param name="data">The data backing this window.</param>
        public abstract void Open(object data);
    }
}
