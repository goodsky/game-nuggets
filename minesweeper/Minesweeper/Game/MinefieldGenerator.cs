using System;

namespace Minesweeper.Game
{
    public static class MinefieldGenerator
    {
        private static readonly int[] deltaRow = new[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] deltaCol = new[] { -1, -1, -1, 0, 0, 1, 1, 1 };

        private static readonly Random rnd = new Random();

        public static Minefield Create(int rows, int columns, int mineCount)
        {
            bool[,] mineLocations = CreateRandomMineLocations(rows, columns, mineCount);
            return InitializeMinefield(mineLocations);
        }

        public static bool[,] CreateRandomMineLocations(int rows, int columns, int mineCount)
        {
            if (rows <= 0 || columns <= 0)
            {
                throw new ArgumentException("Error generating Minefield. Must have positive row and column value.");
            }

            if (mineCount <= 0 || mineCount > rows * columns)
            {
                throw new ArgumentException($"Error generating Minefield. Invalid mine count. {mineCount} is not between [1..{rows * columns}].");
            }

            // O(N^3) brute force implementation
            // For each mine, select a random index based on the number of empty cells that remain.
            // This guarantees that each mine is generated on an empty cell, we don't have to deal
            // with conflicts when two mines are generated on the same cell.
            var mineLocations = new bool[rows, columns];
            int emptyCellsRemaining = rows * columns;

            for (int i = 0; i < mineCount; i++)
            {
                int emptyCellIndex = rnd.Next(emptyCellsRemaining);

                int emptyCellCount = 0;
                int minedCellCount = 0;
                while (emptyCellCount < emptyCellIndex ||
                        CheckMineAt(mineLocations, emptyCellCount + minedCellCount))
                {
                    if (CheckMineAt(mineLocations, emptyCellCount + minedCellCount))
                    {
                        minedCellCount++;
                    }
                    else
                    {
                        emptyCellCount++;
                    }
                }

                int mineIndex = emptyCellCount + minedCellCount;
                int mineX = mineIndex % columns;
                int mineY = mineIndex / columns;
                mineLocations[mineY, mineX] = true;
                emptyCellsRemaining--;
            }

            return mineLocations;
        }

        public static Minefield InitializeMinefield(bool[,] mines)
        {
            int rowCount = mines.GetLength(0);
            int columnCount = mines.GetLength(1);

            var cells = new Cell[rowCount, columnCount];
            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    bool isMined = mines[row, column];
                    int adjacentMines = CountAdjacentMines(mines, row, column);
                    cells[row, column] = new Cell(isMined, adjacentMines);
                }
            }

            return new Minefield(cells);
        }

        private static bool CheckMineAt(bool[,] mines, int index)
        {
            int columns = mines.GetLength(1);
            int x = index % columns;
            int y = index / columns;
            return mines[y, x];
        }

        private static int CountAdjacentMines(bool[,] mines, int row, int column)
        {
            int adjacentMines = 0;
            for (int i = 0; i < deltaRow.Length; i++)
            {
                int checkRow = row + deltaRow[i];
                int checkCol = column + deltaCol[i];
                if (checkRow >= 0 && checkRow < mines.GetLength(0) &&
                    checkCol >= 0 && checkCol < mines.GetLength(1) &&
                    mines[checkRow, checkCol])
                {
                    adjacentMines++;
                }
            }

            return adjacentMines;
        }
    }
}
