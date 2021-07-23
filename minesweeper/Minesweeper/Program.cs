using Minesweeper.ConsoleUI;
using Minesweeper.Game;
using System;
using System.IO;
using System.Text;

namespace Minesweeper
{
    class Program
    {
        /// <summary>
        /// Cracking the Coding Interview Problem 7.10
        /// Design and implement a text-based Minesweeper game.
        /// </summary>
        static void Main(string[] args) => new GameConsole().Run();
    }
}
