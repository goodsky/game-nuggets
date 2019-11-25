using System;
using System.Linq;
using UI;

namespace Common
{
    /// <summary>
    /// Static tracker of the object that is selected.
    /// </summary>
    public static class SelectionManager
    {
        public static Selectable Selected { get { return globalSelection; } }

        private static Selectable globalSelection = null;

        private static object globalSelectionLock = new object();

        private static bool _frozen = false;
        private static Selectable[] _exceptions;

        /// <summary>
        /// Make it so you can't select anything.
        /// Useful if something has taken command of the full screen.
        /// </summary>
        public static void FreezeSelection(params Selectable[] exceptions)
        {
            lock (globalSelectionLock)
            {
                // We get into a weird state if we don't unselect whatever you're doing
                UpdateSelection(null);

                _frozen = true;
                _exceptions = exceptions;
            }
        }

        /// <summary>
        /// Enable selecting things again.
        /// </summary>
        public static void UnfreezeSelection()
        {
            lock (globalSelectionLock)
            {
                _frozen = false;
                _exceptions = Array.Empty<Selectable>();
            }
        }

        /// <summary>
        /// Updates which object is currently selected.
        /// </summary>
        /// <param name="selection">The Selectable component of the new selected GameObject.</param>
        public static void UpdateSelection(Selectable selection)
        {
            lock (globalSelectionLock)
            {
                if (_frozen &&
                    !_exceptions.Contains(selection))
                {
                    return;
                }

                // NB: If you select the same item twice the Deselect() event will not be fired.
                //    However, the Select() event will be fired again.
                var oldSelection = globalSelection;
                globalSelection = selection;

                if (oldSelection != null &&
                    oldSelection != selection)
                {
                    try
                    {
                        oldSelection.Deselect();
                    }
                    catch (Exception e)
                    {
                        GameLogger.Warning("Exception during Deselect. Object = {0}. Ex = {1}.", oldSelection.GetType().Name, e);
                    }
                }

                if (globalSelection != null)
                {
                    try
                    {
                        globalSelection.Select();
                    }
                    catch (Exception e)
                    {
                        GameLogger.Warning("Exception during Select. Object = {0}. Ex = {1}.", globalSelection.GetType().Name, e);
                    }
                }

                TooltipManager.PopDown();
            }
        }
    }
}
