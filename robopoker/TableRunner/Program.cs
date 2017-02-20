using System;
using System.Collections.Generic;
using System.IO;

namespace TableRunner
{
    /// <summary>
    /// This program simulates a game of RoboPoker.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Read the command line arguments then launch the TableRunner.
        /// </summary>
        /// <param name="args">Optional and required command line arguments</param>
        static void Main(string[] args)
        {
            Console.WriteLine("******************************************");
            Console.WriteLine("RoboPoker TableRunner console application");
            Console.WriteLine("   author: Skyler Goodell");
            Console.WriteLine("   version: 1.0");
            Console.WriteLine("******************************************");

            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: You must specify a path to the RoboPoker configuration file.");
                PrintUsage();
                return;
            }

            string command = args[0].ToLower();
            if (command == "-?" || command == "-help" || command == "/?" || command == "/help")
            {
                PrintUsage();
                return;
            }

            // read command line arguments
            string configPath = null;
            string playersPath = null;
            bool verbose = false;
            for (int i = 0; i < args.Length; ++i)
            {
                switch (args[i].ToLower())
                {
                    case "-c":
                        configPath = args[++i];
                        Console.WriteLine("   Configuration file = {0}", configPath);
                        continue;

                    case "-p":
                        playersPath = args[++i];
                        Console.WriteLine("   Players file = {0}", playersPath);
                        continue;

                    case "-v":
                        verbose = true;
                        Console.WriteLine("   Verbose output enabled");
                        continue;

                    default:
                        Console.WriteLine("ERROR: Unrecognized command line argument \"{0}\"", args[i]);
                        PrintUsage();
                        return;
                }
            }

            if (configPath == null)
            {
                Console.WriteLine("ERROR: Game configuration file must be provided. Use the \"-c\" argument to specify a config file.");
                PrintUsage();
                return;
            }

            if (playersPath == null)
            {
                Console.WriteLine("ERROR: Players configuration file must be provided. Use the \"-p\" argument to specify a config file.");
                PrintUsage();
                return;
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine("ERROR: Game configuration file \"{0}\" could not be found on disk.", configPath);
                PrintUsage();
                return;
            }

            if (!File.Exists(playersPath))
            {
                Console.WriteLine("ERROR: Players configuration file \"{0}\" could not be found on disk.", playersPath);
                PrintUsage();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Loading Configuration from file: {0}...", configPath);

            if (!Config.LoadConfiguration(configPath))
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Loading Players from file: {0}...", playersPath);

            List<Player> players;
            if (!Config.LoadPlayers(playersPath, out players))
            {
                return;
            }

            TableRunner runner = new TableRunner();
            if (!runner.SetUp(players))
            {
                return;
            }

            Console.WriteLine("Table is ready to go!");
            Console.WriteLine("******************************************");

            // Run the table
            // This method will block until the entire game is complete, or there was an error
            bool result = runner.RunTable();
            Logger.Finalize();

            if (result)
            {
                Console.WriteLine("RoboPoker game complete");
            }
            else
            {
                Console.WriteLine("RoboPoker did not complete successfully! :(");
            }
        }

        /// <summary>
        /// When invalid command line arguments are provided or a user requests it,
        /// print the TableRunner command line usage
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: TableRunner -c ConfigFile -p PlayersFile [-arguments (values)]");
            Console.WriteLine("   ConfigFile is the path to the configuration file listing the rules for the game and the players");
            Console.WriteLine("   PlayersFile is the path to the TSV file specifying players and AIs for this round");
            Console.WriteLine();
            Console.WriteLine("Optional Arguments:");
            Console.WriteLine("   -v: flag indicating you want to see all the output written to console");
        }
    }
}
