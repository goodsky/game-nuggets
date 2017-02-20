using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim
{
    // define some common delegates for the game
    // is this a normal thing to do? oh well, it seems convenient right now
    public delegate void VoidDelegate();
    public delegate void ObjectDelegate(object obj);

    /// <summary>
    /// Enumeration of stats that university buildnigs can have.
    /// Each of these stats will have to have more details that drive the algorithm.
    /// </summary>
    public enum BuildingStat
    {
        Education,
        Athletics,
        Revenue,
        Research,
        Arts,
        Spirit
    }

    /// <summary>
    /// Is it left or is it right? OR is it neither?!
    /// I have mixed feelings about this enum. BUT it is useful when clicking on things.
    /// </summary>
    public enum LeftOrRight
    {
        Left,
        Right,
        Neither
    }

    /// <summary>
    /// Enumeration of warning levels for logging and events (if events ever happen)
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Define hard-coded game constants in this class.
    /// They should all be static readonly.
    /// </summary>
    public class Constants
    {
        // window size and resolution
        public static readonly int WINDOW_WIDTH = 853;
        public static readonly int WINDOW_HEIGHT = 480;

        // tile information
        public static readonly int TILE_WIDTH = 64;
        public static readonly int TILE_HEIGHT = 32;

        // gameplay tweaks
        public static readonly int CLICK_MILLISECONDS = 500;
        public static readonly int CLICK_PIXEL_DISTANCE = 5;
        public static readonly int MOUSE_HOVER_MILLISECONDS = 1000;
        public static readonly int SCROLLBAR_CLICK_SPEED = 5;

        public static readonly int GUI_DEPTH = Int32.MaxValue - 10;

        // config and logging constants
        public static readonly string DEFAULT = "Default";
        public static readonly string LOG_FILE = "UniversitySimLog";
    }
}
