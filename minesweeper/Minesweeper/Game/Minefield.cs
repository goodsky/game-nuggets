using System;

namespace Minesweeper.Game
{
    public class Minefield
    {
        private readonly Cell[,] _cells;

        public Minefield(Cell[,] minefield)
        {
            _cells = minefield;

            RowCount = minefield.GetLength(0);
            ColumnCount = minefield.GetLength(1);

            int initialMineCount = 0;
            foreach (Cell cell in minefield)
            {
                if (cell.IsMined)
                {
                    initialMineCount++;
                }
            }

            MineCount = initialMineCount;
            RemainingUnrevealedCount = RowCount * ColumnCount - MineCount;
        }

        public int RowCount { get; }
        public int ColumnCount { get; }
        public int MineCount { get; }

        public int FlagCount { get; private set; }
        public int RemainingUnrevealedCount { get; private set; }

        public bool IsGameOver { get; private set; }

        public Cell GetCell(int row, int column)
        {
            CheckIfValidCell(row, column);

            return _cells[row, column];
        }

        public void ToggleFlag(int row, int column)
        {
            CheckIfValidCell(row, column);

            Cell cell = _cells[row, column];
            if (cell.IsRevealed)
            {
                return;
            }

            bool isFlagged = !cell.IsFlagged;
            if (isFlagged)
            {
                if (FlagCount >= MineCount)
                {
                    return;
                }

                FlagCount++;
            }
            else
            {
                FlagCount--;
            }

            _cells[row, column] = new Cell(cell, isFlagged: isFlagged, isRevealed: cell.IsRevealed);
        }

        public void RevealCell(int row, int column)
        {
            CheckIfValidCell(row, column);

            Cell cell = _cells[row, column];
            if (cell.IsFlagged)
            {
                return;
            }

            if (cell.IsMined)
            {
                IsGameOver = true;
                _cells[row, column] = new Cell(cell, isFlagged: false, isRevealed: true);
                return;
            }

            var seen = new bool[RowCount, ColumnCount];
            seen[row, column] = true;

            int revealedCells = DepthFirstReveal(row, column, seen);
            RemainingUnrevealedCount -= revealedCells;
        }

        private void CheckIfValidCell(int row, int column)
        {
            if (column < 0 || column >= ColumnCount ||
                row < 0 || row >= RowCount)
            {
                throw new ArgumentException($"Cell is out of range. ({column}, {row}) is not within {ColumnCount}x{RowCount}.");
            }
        }

        private static int[] dfsDeltaRow = new[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static int[] dfsDeltaCol = new[] { -1, -1, -1, 0, 0, 1, 1, 1};
        private int DepthFirstReveal(int atRow, int atColumn, bool[,] seen)
        {
            Cell atCell = _cells[atRow, atColumn];
            if (atCell.IsRevealed)
            {
                return 0;
            }

            if (atCell.IsFlagged)
            {
                FlagCount -= 1;
            }

            if (atCell.IsMined)
            {
                throw new InvalidOperationException($"Fatal error while revealing cells! Unexpected mine at {atRow}, {atColumn}.");
            }

            int revealed = 1;
            if (atCell.AdjacentMines == 0)
            {
                for (int i = 0; i < dfsDeltaRow.Length; i++)
                {
                    int nextRow = atRow + dfsDeltaRow[i];
                    int nextColumn = atColumn + dfsDeltaCol[i];

                    if (nextRow >= 0 && nextRow < RowCount &&
                        nextColumn >= 0 && nextColumn < ColumnCount &&
                        !seen[nextRow, nextColumn])
                    {
                        seen[nextRow, nextColumn] = true;
                        revealed += DepthFirstReveal(nextRow, nextColumn, seen);
                    }
                }
            }

            _cells[atRow, atColumn] = new Cell(atCell, isFlagged: false, isRevealed: true);
            return revealed;
        }
    }
}
