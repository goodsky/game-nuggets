namespace Minesweeper.Game
{
    /// <summary>
    /// The data structure for a single cell in the Minefield.
    /// </summary>
    public struct Cell
    {
        public Cell(bool isMined, int adjacentMines)
        {
            AdjacentMines = adjacentMines;
            IsMined = isMined;
            IsRevealed = false;
            IsFlagged = false;
        }

        public Cell(Cell cell, bool isFlagged, bool isRevealed)
        {
            AdjacentMines = cell.AdjacentMines;
            IsMined = cell.IsMined;
            IsFlagged = isFlagged;
            IsRevealed = isRevealed;
        }

        /// <summary>
        /// The number of adjacent mines (value is between 0-8).
        /// </summary>
        public int AdjacentMines { get; }

        /// <summary>
        /// True if the cell contains a mine.
        /// </summary>
        public bool IsMined { get; }

        /// <summary>
        /// True if the cell has been revealed by the player.
        /// </summary>
        public bool IsRevealed { get; }

        /// <summary>
        /// True if the cell bas been flagged by the player.
        /// </summary>
        public bool IsFlagged { get; }
    }
}
