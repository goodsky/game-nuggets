using Minesweeper.Game;
using System;

namespace Minesweeper.ConsoleUI
{
    public class MinefieldUI
    {
        private const int MINWIDTH = 30;
        private const int MINHEIGHT = 15;

        private const int HORIZONTALPADDING = 2;
        private const int TOPPADDING = 3;
        private const int BOTTOMPADDING = 6;

        private Minefield _minefield;
        private BufferedConsole _console;

        private bool _initialized = false;

        private int _minefieldTop;
        private int _minefieldLeft;

        private int _cursorRow;
        private int _cursorColumn;

        public MinefieldUI(Minefield minefield)
        {
            _minefield = minefield;
        }

        public void SetCursor(int row, int column)
        {
            _cursorRow = row;
            _cursorColumn = column;
        }

        public void DrawGame()
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            DrawScore();
            DrawMinefield();
            DrawInstructions();
        }

        public void DrawWinScreen()
        {
            _console.WriteString(_minefieldTop - 2, _minefieldLeft, $"CONGRATULATIONS!", ConsoleColor.Green);

            int instructionsRow = _console.Height - BOTTOMPADDING + 1;
            _console.WriteString(instructionsRow, _minefieldLeft, $"               ");
            _console.WriteString(instructionsRow + 1, _minefieldLeft, $"Press Enter");
            _console.WriteString(instructionsRow + 2, _minefieldLeft, $"           ");
            _console.WriteString(instructionsRow + 3, _minefieldLeft, $"           ");
        }

        public void DrawLoseScreen()
        {
            _console.WriteString(_minefieldTop - 2, _minefieldLeft, $"TOO BAD! YOU LOST!", ConsoleColor.Red);

            int instructionsRow = _console.Height - BOTTOMPADDING + 1;
            _console.WriteString(instructionsRow, _minefieldLeft, $"               ");
            _console.WriteString(instructionsRow + 1, _minefieldLeft, $"Press Enter");
            _console.WriteString(instructionsRow + 2, _minefieldLeft, $"           ");
            _console.WriteString(instructionsRow + 3, _minefieldLeft, $"           ");
        }

        private void Initialize()
        {
            int minefieldWidth = _minefield.ColumnCount;
            int minefieldHeight = _minefield.RowCount;

            int consoleWidth, consoleHeight;
            if (minefieldWidth >= MINWIDTH - HORIZONTALPADDING * 2)
            {
                consoleWidth = minefieldWidth + HORIZONTALPADDING * 2;
                _minefieldLeft = HORIZONTALPADDING;
            }
            else
            {
                consoleWidth = MINWIDTH;
                _minefieldLeft = HORIZONTALPADDING + (MINWIDTH - minefieldWidth) / 2;
            }
            
            if (minefieldHeight >= MINHEIGHT - TOPPADDING - BOTTOMPADDING)
            {
                consoleHeight = minefieldHeight + TOPPADDING + BOTTOMPADDING;
                _minefieldTop = TOPPADDING;
            }
            else
            {
                consoleHeight = MINHEIGHT;
                _minefieldTop = TOPPADDING + (MINHEIGHT - minefieldHeight) / 2;
            }

            _console = new BufferedConsole(consoleWidth, consoleHeight);
            _console.InitializeWindow();
        }

        private void DrawScore()
        {
            int remainingCells = _minefield.RemainingUnrevealedCount;
            _console.WriteString(_minefieldTop - 2, _minefieldLeft, $"Remaining: {remainingCells}");
        }

        private void DrawInstructions()
        {
            int instructionsRow = _console.Height - BOTTOMPADDING + 1;
            _console.WriteString(instructionsRow, _minefieldLeft, $"Move: Arrow Keys");
            _console.WriteString(instructionsRow + 1, _minefieldLeft, $"Reveal: Spacebar");
            _console.WriteString(instructionsRow + 2, _minefieldLeft, $"Flag: F");
            _console.WriteString(instructionsRow + 3, _minefieldLeft, $"Quit: Esc");
        }

        private void DrawMinefield()
        {
            for (int row = 0; row < _minefield.RowCount; row++)
            {
                for (int column = 0; column < _minefield.ColumnCount; column++)
                {
                    ConsoleCell consoleCell = GetCellCharacter(row, column);
                    _console.WriteCell(_minefieldTop + row, _minefieldLeft + column, consoleCell);
                }
            }
        }

        private ConsoleCell GetCellCharacter(int row, int column)
        {
            Cell cell = _minefield.GetCell(row, column);

            ConsoleCell consoleCell;
            if (cell.IsFlagged)
            {
                // Triangle Flag
                consoleCell = new ConsoleCell('>', ConsoleColor.White, ConsoleColor.DarkBlue);
            }
            else if (cell.IsRevealed)
            {
                if (cell.IsMined)
                {
                    consoleCell = new ConsoleCell('X', ConsoleColor.Black, ConsoleColor.Red);
                }
                else
                {
                    switch (cell.AdjacentMines)
                    {
                        case 0:
                            // Revealed Gray
                            consoleCell = new ConsoleCell(' ', ConsoleColor.Gray, ConsoleColor.Gray);
                            break;
                        case 1:
                            consoleCell = new ConsoleCell('1', ConsoleColor.DarkBlue, ConsoleColor.Gray);
                            break;
                        case 2:
                            consoleCell = new ConsoleCell('2', ConsoleColor.DarkGreen, ConsoleColor.Gray);
                            break;
                        case 3:
                            consoleCell = new ConsoleCell('3', ConsoleColor.DarkRed, ConsoleColor.Gray);
                            break;
                        case 4:
                            consoleCell = new ConsoleCell('4', ConsoleColor.Blue, ConsoleColor.Gray);
                            break;
                        case 5:
                            consoleCell = new ConsoleCell('5', ConsoleColor.Red, ConsoleColor.Gray);
                            break;
                        case 6:
                            consoleCell = new ConsoleCell('6', ConsoleColor.Cyan, ConsoleColor.Gray);
                            break;
                        case 7:
                            consoleCell = new ConsoleCell('7', ConsoleColor.Black, ConsoleColor.Gray);
                            break;
                        case 8:
                            consoleCell = new ConsoleCell('8', ConsoleColor.Black, ConsoleColor.Gray);
                            break;

                        default:
                            throw new InvalidOperationException($"ERROR: Invalid Adjacent Mines in cell {row},{column}. Count = {cell.AdjacentMines}.");
                    }
                }
            }
            else
            {
                if ((_minefield.IsGameOver || _minefield.RemainingUnrevealedCount == 0) &&
                    cell.IsMined)
                {
                    // Reveal Mines
                    consoleCell = new ConsoleCell('X', ConsoleColor.Black, ConsoleColor.DarkGray);
                }
                else
                {
                    // Unrevealed Gray
                    consoleCell = new ConsoleCell(' ', ConsoleColor.DarkGray, ConsoleColor.DarkGray);
                }
            }

            if (row == _cursorRow && column == _cursorColumn)
            {
                consoleCell.Foreground = ConsoleColor.Yellow;
                consoleCell.Background = ConsoleColor.DarkYellow;
            }

            return consoleCell;
        }

        
    }
}
