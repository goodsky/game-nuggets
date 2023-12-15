using Minesweeper.Game;
using System;
using System.Collections.Generic;
using Xunit;

namespace MinesweeperTests
{
    public class MinefieldGeneratorTests
    {
        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(5, 5, 1)]
        [InlineData(5, 5, 5)]
        [InlineData(5, 5, 25)]
        [InlineData(1, 10, 1)]
        [InlineData(1, 10, 10)]
        [InlineData(10, 1, 1)]
        [InlineData(10, 1, 10)]
        [InlineData(15, 25, 100)]
        [InlineData(26, 26, 1)]
        [InlineData(26, 26, 338)]
        [InlineData(26, 26, 676)]
        [InlineData(100, 100, 1)]
        [InlineData(100, 100, 5000)]
        [InlineData(100, 100, 10000)]
        public void CanCreate(int rows, int columns, int mineCount)
        {
            MinefieldGenerator.Create(rows, columns, mineCount);
        }

        [Theory]
        [InlineData(-1, -1, -1)]
        [InlineData(-1, 1, -1)]
        [InlineData(1, -1, -1)]
        [InlineData(1, 1, 0)]
        [InlineData(1, 1, 2)]
        [InlineData(10, 10, 101)]
        public void InvalidCreateThrows(int rows, int columns, int mineCount)
        {
            Assert.Throws<ArgumentException>(() => MinefieldGenerator.Create(rows, columns, mineCount));
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(5, 5, 1)]
        [InlineData(5, 5, 5)]
        [InlineData(5, 5, 25)]
        [InlineData(1, 10, 1)]
        [InlineData(1, 10, 10)]
        [InlineData(10, 1, 1)]
        [InlineData(10, 1, 10)]
        [InlineData(15, 25, 100)]
        [InlineData(26, 26, 1)]
        [InlineData(26, 26, 338)]
        [InlineData(26, 26, 676)]
        [InlineData(100, 100, 1)]
        [InlineData(100, 100, 5000)]
        [InlineData(100, 100, 10000)]
        public void CanCreateRandomMineLocations(int rows, int columns, int mineCount)
        {
            // Act
            bool[,] mineLocations = MinefieldGenerator.CreateRandomMineLocations(rows, columns, mineCount);

            // Assert
            Assert.Equal(rows, mineLocations.GetLength(0));
            Assert.Equal(columns, mineLocations.GetLength(1));

            int actualMineCount = 0;
            for (int row = 0; row < mineLocations.GetLength(0); row++)
            {
                for (int column = 0; column < mineLocations.GetLength(1); column++)
                {
                    if (mineLocations[row, column])
                    {
                        actualMineCount++;
                    }
                }
            }
            Assert.Equal(mineCount, actualMineCount);
        }

        public static IEnumerable<object[]> InitializeMinefieldData =>
            new[]
            {
                new object[]
                {
                    new bool[,]
                    {
                        { false, false, false },
                        { false, true, false },
                        { false, false, false },
                    },
                    new int[,]
                    {
                        { 1, 1, 1 },
                        { 1, 0, 1 },
                        { 1, 1, 1 },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true, true, true },
                        { true, false, true },
                        { true, true, true },
                    },
                    new int[,]
                    {
                        { 2, 4, 2 },
                        { 4, 8, 4 },
                        { 2, 4, 2 },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { false },
                    },
                    new int[,]
                    {
                        { 0 }
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true, false, false, true },
                        { false, false, true, false },
                        { false, true, false, false },
                        { true, false, false, true },
                    },
                    new int[,]
                    {
                        { 0, 2, 2, 1 },
                        { 2, 3, 2, 2 },
                        { 2, 2, 3, 2 },
                        { 1, 2, 2, 0 },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true, true, true, true },
                    },
                    new int[,]
                    {
                        { 1, 2, 2, 1 },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true },
                        { true },
                        { true },
                        { true },
                    },
                    new int[,]
                    {
                        { 1 },
                        { 2 },
                        { 2 },
                        { 1 },
                    },
                },
            };

        [Theory]
        [MemberData(nameof(InitializeMinefieldData))]
        public void CanInitializeMinefield(bool[,] mineLocations, int[,] expectedAdjacent)
        {
            // Act
            Minefield minefield = MinefieldGenerator.InitializeMinefield(mineLocations);

            // Assert
            for (int row = 0; row < expectedAdjacent.GetLength(0); row++)
            {
                for (int column = 0; column < expectedAdjacent.GetLength(1); column++)
                {
                    Cell cell = minefield.GetCell(row, column);

                    Assert.Equal(expectedAdjacent[row, column], cell.AdjacentMines);
                    Assert.Equal(mineLocations[row, column], cell.IsMined);
                    Assert.False(cell.IsFlagged);
                    Assert.False(cell.IsRevealed);
                }
            }
        }
    }
}
