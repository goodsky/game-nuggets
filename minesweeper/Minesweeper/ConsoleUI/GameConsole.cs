using Minesweeper.Game;
using System;

namespace Minesweeper.ConsoleUI
{
    public class GameConsole
    {
        private const int MINROWS = 3;
        private const int MAXROWS = 80;

        private const int MINCOLUMNS = 3;
        private const int MAXCOLUMNS = 160;

        private int _saveBufferWidth;
        private int _saveBufferHeight;
        private int _saveWindowWidth;
        private int _saveWindowHeight;
        private bool _saveCursorVisible;
        ConsoleColor _saveForegroundColor;
        ConsoleColor _saveBackgroundColor;

        public void Run()
        {
            _saveBufferWidth = Console.BufferWidth;
            _saveBufferHeight = Console.BufferHeight;
            _saveWindowWidth = Console.WindowWidth;
            _saveWindowHeight = Console.WindowHeight;
            _saveCursorVisible = Console.CursorVisible;
            _saveForegroundColor = Console.ForegroundColor;
            _saveBackgroundColor = Console.BackgroundColor;

            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => { RestoreConsole(); Environment.Exit(0); };

            try
            {
                bool isRunning = true;
                do
                {
                    (int rows, int columns, int mines) = RunMenu();
                    RunGame(rows, columns, mines);
                } while (isRunning);
            }
            finally
            {
                RestoreConsole();
            }
        }

        public (int rows, int columns, int mines) RunMenu()
        {
            RestoreConsole();
            Console.WriteLine("*******************************************************");
            Console.WriteLine("*          WELCOME TO MINESWEEPER CONSOLE             *");
            Console.WriteLine("*******************************************************");
            Console.Write("[[ Press any key to start ]]");
            Console.ReadKey(true);
            Console.CursorLeft = 0;

            Console.WriteLine("Select game difficulty:                ");
            int columns = ReadInteger("Board Size (Width): ", MINCOLUMNS, MAXCOLUMNS);
            int rows = ReadInteger("Board Size (Height): ", MINROWS, MAXROWS);
            int mines = ReadInteger("Number of Mines: ", 1, rows * columns - 1);

            return (rows, columns, mines);
        }

        public void RunGame(int rows, int columns, int mines)
        {
            Minefield minefield = MinefieldGenerator.Create(rows, columns, mines);
            MinefieldUI ui = new MinefieldUI(minefield);

            int cursorRow = minefield.RowCount / 2;
            int cursorColumn = minefield.ColumnCount / 2;
            ui.SetCursor(cursorRow, cursorColumn);
            ui.DrawGame();

            ConsoleWindowUtils.DisableConsoleWindowResize();

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.LeftArrow:
                        cursorColumn -= 1;
                        break;

                    case ConsoleKey.RightArrow:
                        cursorColumn += 1;
                        break;

                    case ConsoleKey.UpArrow:
                        cursorRow -= 1;
                        break;

                    case ConsoleKey.DownArrow:
                        cursorRow += 1;
                        break;

                    case ConsoleKey.Spacebar:
                        minefield.RevealCell(cursorRow, cursorColumn);
                        break;

                    case ConsoleKey.F:
                        minefield.ToggleFlag(cursorRow, cursorColumn);
                        break;
                }

                if (cursorRow < 0) cursorRow = 0;
                if (cursorRow >= minefield.RowCount) cursorRow = minefield.RowCount - 1;
                if (cursorColumn < 0) cursorColumn = 0;
                if (cursorColumn >= minefield.ColumnCount) cursorColumn = minefield.ColumnCount - 1;

                ui.SetCursor(cursorRow, cursorColumn);
                ui.DrawGame();

                if (minefield.RemainingUnrevealedCount == 0)
                {
                    ui.DrawWinScreen();
                    do { cki = Console.ReadKey(true); } while (cki.Key != ConsoleKey.Enter);
                    break;
                }

                if (minefield.IsGameOver)
                {
                    ui.DrawLoseScreen();
                    do { cki = Console.ReadKey(true); } while (cki.Key != ConsoleKey.Enter);
                    break;
                }


            } while (cki.Key != ConsoleKey.Escape);
        }

        private int ReadInteger(string prompt, int minValue, int maxValue)
        {
            do
            {
                Console.Write(prompt);
                string line = Console.ReadLine();

                if (!int.TryParse(line, out int value))
                {
                    Console.WriteLine("Nope! Value must be an integer.");
                }
                else if (value < minValue || value > maxValue)
                {
                    Console.WriteLine("Nope! Value must be between [{0}, {1}].", minValue, maxValue);
                }
                else
                {
                    return value;
                }
            } while (true);
        }

        private void RestoreConsole()
        {
            Console.Clear();
            Console.SetWindowSize(_saveWindowWidth, _saveWindowHeight);
            Console.SetBufferSize(_saveBufferWidth, _saveBufferHeight);
            Console.CursorVisible = _saveCursorVisible;
            Console.ForegroundColor = _saveForegroundColor;
            Console.BackgroundColor = _saveBackgroundColor;
        }
    }
}
