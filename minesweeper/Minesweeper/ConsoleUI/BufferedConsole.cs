using System;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper.ConsoleUI
{
    public struct ConsoleCell : IEquatable<ConsoleCell>
    {
        public ConsoleCell(char c, ConsoleColor foreground, ConsoleColor background)
        {
            Char = c;
            Foreground = foreground;
            Background = background;
        }

        public char Char { get; set; }

        public ConsoleColor Foreground { get; set; }

        public ConsoleColor Background { get; set; }

        public bool Equals(ConsoleCell other)
        {
            return other.Char == Char &&
                other.Foreground == Foreground &&
                other.Background == Background;
        }
    }

    public class BufferedConsole
    {
        private ConsoleCell[,] _console;

        public BufferedConsole(int width, int height)
        {
            _console = new ConsoleCell[height, width];
        }

        public int Width => _console.GetLength(1);
        public int Height => _console.GetLength(0);

        public bool InitializeWindow()
        {
            int windowWidth = _console.GetLength(1);
            int windowHeight = _console.GetLength(0);

            try
            {
                Console.Clear();
                Console.CursorVisible = false;
                Console.SetWindowSize(windowWidth, windowHeight);
                Console.SetBufferSize(windowWidth, windowHeight);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine("ERROR: {0}", ex.ToString());
                return false;
            }

            return true;
        }

        public void WriteCell(int row, int column, ConsoleCell cell)
        {
            ConsoleCell existingCell = _console[row, column];
            if (!existingCell.Equals(cell))
            {
                _console[row, column] = cell;
                Console.SetCursorPosition(column, row);
                Console.ForegroundColor = cell.Foreground;
                Console.BackgroundColor = cell.Background;
                Console.Write(cell.Char);
            }
        }

        public void WriteString(int row, int column, string s, ConsoleColor foreground = ConsoleColor.White)
        {
            Console.SetCursorPosition(column, row);
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(s + "     "); // hack: extra whitespace to clear characters
        }
    }
}
