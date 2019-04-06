namespace Common
{
    public static class SelectableExtensions
    {
        /// <summary>
        /// Gets the 'main menu' level selectable parent.
        /// </summary>
        /// <param name="selectable">The selectable to roll up.</param>
        /// <returns>The selectable that has no other parent.</returns>
        public static Selectable ToMainMenu(this Selectable selectable)
        {
            if (selectable == null)
                return null;

            while (selectable.SelectionParent != null)
                selectable = selectable.SelectionParent;

            return selectable;
        }

        /// <summary>
        /// Gets the 'sub menu' level selectable parent.
        /// </summary>
        /// <param name="selectable">The selectable to roll up.</param>
        /// <returns>The selectable whose parent has no other parent.</returns>
        public static Selectable ToSubMenu(this Selectable selectable)
        {
            if (selectable == null || selectable.SelectionParent == null)
                return null;

            while (selectable.SelectionParent.SelectionParent != null)
                selectable = selectable.SelectionParent;

            return selectable;
        }
    }
}
