using Minesweeper.Game;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MinesweeperTests
{
    public class MinefieldTests
    {
        [Fact]
        public void CanToggleFlag()
        {
            // Arrange
            int rowCount = 10;
            int columnCount = 10;
            int mineCount = 1;

            Minefield minefield = MinefieldGenerator.Create(rowCount, columnCount, mineCount);

            // Act & Assert - Add Flag
            Assert.False(minefield.GetCell(0, 0).IsFlagged);
            Assert.False(minefield.GetCell(0, 0).IsRevealed);
            Assert.Equal(0, minefield.FlagCount);

            minefield.ToggleFlag(0, 0);

            Assert.True(minefield.GetCell(0, 0).IsFlagged);
            Assert.False(minefield.GetCell(0, 0).IsRevealed);
            Assert.Equal(1, minefield.FlagCount);

            // Act & Assert - Remove Flag
            minefield.ToggleFlag(0, 0);

            Assert.False(minefield.GetCell(0, 0).IsFlagged);
            Assert.False(minefield.GetCell(0, 0).IsRevealed);
            Assert.Equal(0, minefield.FlagCount);
        }

        [Fact]
        public void CannotSetMoreFlagsThanMines()
        {
            // Arrange
            int rowCount = 10;
            int columnCount = 10;
            int mineCount = 5; // NB: Assumed to be smaller than rowCount

            Minefield minefield = MinefieldGenerator.Create(rowCount, columnCount, mineCount);

            // Act
            for (int row = 0; row < mineCount + 1; row++)
            {
                minefield.ToggleFlag(row, 0);
            }

            // Assert
            Assert.Equal(mineCount, minefield.FlagCount);
            for (int row = 0; row < mineCount; row++)
            {
                Assert.True(minefield.GetCell(row, 0).IsFlagged);
            }

            Assert.False(minefield.GetCell(mineCount, 0).IsFlagged);
        }

        public static IEnumerable<object[]> RevealCellsData =>
            new[]
            {
                new object[]
                {
                    new bool[,]
                    {
                        { true, false, false },
                        { false, false, false },
                        { false, false, false },
                    },
                    0, 0,
                    new bool[,]
                    {
                        { true, false, false },
                        { false, false, false },
                        { false, false, false },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true, false, false },
                        { false, false, false },
                        { false, false, false },
                    },
                    1, 1,
                    new bool[,]
                    {
                        { false, false, false },
                        { false, true, false },
                        { false, false, false },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true, false, false },
                        { false, false, false },
                        { false, false, false },
                    },
                    2, 2,
                    new bool[,]
                    {
                        { false, true, true },
                        { true, true, true },
                        { true, true, true },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true, false, false, false, false },
                    },
                    0, 4,
                    new bool[,]
                    {
                        { false, true, true, true, true },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { true },
                        { false },
                        { false },
                        { false },
                        { false },
                    },
                    4, 0,
                    new bool[,]
                    {
                        { false },
                        { true },
                        { true },
                        { true },
                        { true },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                        { true, false, false, false, true },
                        { false, false, false, false, false },
                        { true, false, false, false, true },
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                    },
                    6, 2,
                    new bool[,]
                    {
                        { true, true, true, true, true },
                        { true, true, true, true, true },
                        { false, true, true, true, false },
                        { false, true, true, true, false },
                        { false, true, true, true, false },
                        { true, true, true, true, true },
                        { true, true, true, true, true },
                    },
                },
                new object[]
                {
                    new bool[,]
                    {
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                        { true, false, false, false, true },
                        { false, false, true, false, false },
                        { true, false, false, false, true },
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                    },
                    6, 2,
                    new bool[,]
                    {
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                        { false, false, false, false, false },
                        { false, true, true, true, false },
                        { true, true, true, true, true },
                        { true, true, true, true, true },
                    },
                },
            };

        [Theory]
        [MemberData(nameof(RevealCellsData))]
        public void CanRevealCells(bool[,] mineLocations, int revealRow, int revealColumn, bool[,] expectedRevealed)
        {
            // Arrange
            Minefield minefield = MinefieldGenerator.InitializeMinefield(mineLocations);

            // Act
            int startingUnrevealedCount = minefield.RemainingUnrevealedCount;
            minefield.RevealCell(revealRow, revealColumn);
            int endingUnrevealedCount = minefield.RemainingUnrevealedCount;

            // Assert
            int numUnminedCells = 0;
            int numUnrevealedCells = 0;
            for (int row = 0; row < mineLocations.GetLength(0); row++)
            {
                for (int column = 0; column < mineLocations.GetLength(1); column++)
                {
                    Cell cell = minefield.GetCell(row, column);
                    if (!cell.IsMined)
                    {
                        numUnminedCells++;
                    }

                    if (!cell.IsMined && !expectedRevealed[row, column])
                    {
                        numUnrevealedCells++;
                    }

                    Assert.Equal(expectedRevealed[row, column], cell.IsRevealed);
                }
            }

            Assert.Equal(numUnminedCells, startingUnrevealedCount);
            Assert.Equal(numUnrevealedCells, endingUnrevealedCount);
        }

        [Fact]
        public void CannotRevealFlaggedCells()
        {
            // Arrange
            int rowCount = 3;
            int columnCount = 3;
            int mineCount = 1;
            Minefield minefield = MinefieldGenerator.Create(rowCount, columnCount, mineCount);

            // Act
            minefield.ToggleFlag(0, 0);
            minefield.RevealCell(0, 0);

            // Assert
            Assert.True(minefield.GetCell(0, 0).IsFlagged);
            Assert.False(minefield.GetCell(0, 0).IsRevealed);
            Assert.Equal(rowCount * columnCount - mineCount, minefield.RemainingUnrevealedCount);
        }

        [Fact]
        public void RevealingMineCausesGameOver()
        {
            // Arrange
            bool[,] mineLocations = new bool[,]
            {
                { true, false }
            };
            Minefield minefield = MinefieldGenerator.InitializeMinefield(mineLocations);

            // Act
            minefield.RevealCell(0, 0);

            // Assert
            Assert.True(minefield.GetCell(0, 0).IsMined);
            Assert.True(minefield.GetCell(0, 0).IsRevealed);
            Assert.True(minefield.IsGameOver);
        }
    }
}
