using RoboPoker;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TableRunner
{
    internal class Config
    {
        /// <summary>
        /// Private singleton configuration object
        /// </summary>
        private static Config instance = new Config();

        /// <summary>
        /// Getter for singleton configuration object
        /// </summary>
        public static Config Instance { get { return instance; } }

        /// <summary>
        /// Make the default constructor private
        /// </summary>
        private Config()
        {

        }

        /// <summary>
        /// Read the configuration
        /// </summary>
        /// <param name="configPath">Path to the config file</param>
        /// <returns>True if setup is successful, otherwise false</returns>
        public static bool LoadConfiguration(string configPath)
        {
            try
            {
                using (StreamReader config = new StreamReader(configPath))
                {
                    if (!Instance.LoadConfig(config))
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Error while loading configuration. Exception: {0}", e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read the players configuration file
        /// </summary>
        /// <param name="playersPath">Path to the config file</param>
        /// <returns>True if setup is successful, otherwise false</returns>
        public static bool LoadPlayers(string playersPath, out List<Player> players)
        {
            try
            {
                using (StreamReader config = new StreamReader(playersPath))
                {
                    if (!Instance.LoadPlayers(config, out players))
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                players = null;
                Console.WriteLine("ERROR: Error while loading players. Exception: {0}", e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starting cash
        /// </summary>
        private int startingCash = -1;

        /// <summary>
        /// Getter for starting cash
        /// </summary>
        public int StartingCash { get { return this.startingCash; } }

        /// <summary>
        /// Maximum number of rounds
        /// </summary>
        private int maxRounds = -1;

        /// <summary>
        /// Getter for maximum number of rounds
        /// </summary>
        public int MaxRounds { get { return this.maxRounds; } }

        /// <summary>
        /// Start value for the big blind
        /// </summary>
        private int bigBlindStart = -1;

        /// <summary>
        /// Getter for the start value for the big blind
        /// </summary>
        public int BigBlindStart { get { return this.bigBlindStart; } }

        /// <summary>
        /// [Optional] Big blind growth property
        /// </summary>
        private int bigBlindLinear = 0;

        /// <summary>
        /// Getter for the big blind growth property
        /// </summary>
        public int BigBlindLinear { get { return this.bigBlindLinear; } }

        /// <summary>
        /// [Optional] Big blind growth property
        /// </summary>
        private double bigBlindPercent = 1.0;

        /// <summary>
        /// Getter for the big blind growth property
        /// </summary>
        public double BigBlindPercent { get { return this.bigBlindPercent; } }

        /// <summary>
        /// [Optional] Big blind growth property
        /// </summary>
        private int bigBlindTurnPeriod = -1;

        /// <summary>
        /// Getter for the big blind growth property
        /// </summary>
        public int BigBlindTurnPeriod { get { return this.bigBlindTurnPeriod; } }

        /// <summary>
        /// Load the configuration settings from the given stream
        /// </summary>
        /// <param name="config">The configuration stream</param>
        /// <returns>True if load was successful, otherwise false</returns>
        private bool LoadConfig(StreamReader config)
        {
            while (!config.EndOfStream)
            {
                string configLine = config.ReadLine().Trim();
                if (string.IsNullOrEmpty(configLine) || configLine.StartsWith(";"))
                {
                    continue;
                }

                string[] configFields = configLine.Split('=');
                if (configFields.Length != 2)
                {
                    Console.WriteLine("ERROR: Invalid config file. Line is not in the correct format of <config>=<value>");
                    Console.WriteLine("ERROR: Line: {0}", configLine);
                    return false;
                }

                string configName = configFields[0];
                string configValue = configFields[1];

                switch (configName.ToLower())
                {
                    case "logfile":
                        Regex logFileRgx = new Regex(@"^(?<file>.*?)\((?<filters>.*?)\)$");
                        MatchCollection logfileMatches = logFileRgx.Matches(configValue);

                        if (logfileMatches.Count != 1)
                        {
                            Console.WriteLine("ERROR: Failed to load logFile value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }

                        Match logfileMatch = logfileMatches[0];
                        string logFile = logfileMatch.Groups["file"].Value;
                        string filters = logfileMatch.Groups["filters"].Value;

                        LogLevel levels = LogLevel.None;
                        string[] filtersArray = filters.Split('|');
                        foreach (string filter in filtersArray)
                        {
                            switch (filter.Trim().ToLower())
                            {
                                case "all":
                                    levels |= LogLevel.Error;
                                    levels |= LogLevel.Game;
                                    levels |= LogLevel.Info;
                                    levels |= LogLevel.Warning;
                                    break;
                                case "error":
                                    levels |= LogLevel.Error;
                                    break;
                                case "game":
                                    levels |= LogLevel.Game;
                                    break;
                                case "info":
                                    levels |= LogLevel.Info;
                                    break;
                                case "warning":
                                    levels |= LogLevel.Warning;
                                    break;
                                default:
                                    Console.WriteLine("ERROR: Failed to load logFile value from configuration. Unknown log filter {0}", filter);
                                    return false;
                            }
                        }

                        StreamWriter logStream = null;
                        if (!string.IsNullOrEmpty(logFile) && !logFile.ToUpper().Equals("CONSOLE"))
                        {
                            logStream = new StreamWriter(logFile);
                        }

                        Logger.CreateLogStream(logStream, levels);
                        break;

                    case "startingcash":
                        if (!int.TryParse(configValue, out this.startingCash))
                        {
                            Console.WriteLine("ERROR: Failed to load StartingCash value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }
                        Console.WriteLine("   StartingCash={0}", configValue);
                        break;

                    case "maximumrounds":
                        if (!int.TryParse(configValue, out this.maxRounds))
                        {
                            Console.WriteLine("ERROR: Failed to load MaximumRounds value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }
                        Console.WriteLine("   MaximumRounds={0}", configValue);
                        break;

                    case "bigblindstart":
                        if (!int.TryParse(configValue, out this.bigBlindStart))
                        {
                            Console.WriteLine("ERROR: Failed to load BigBlindStart value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }
                        Console.WriteLine("   BigBlindStart={0}", configValue);
                        break;

                    case "bigblindlinear":
                        if (!int.TryParse(configValue, out this.bigBlindLinear))
                        {
                            Console.WriteLine("ERROR: Failed to load BigBlindLinear value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }
                        Console.WriteLine("   BigBlindLinear={0}", configValue);
                        break;

                    case "bigblindpercent":
                        if (!double.TryParse(configValue, out this.bigBlindPercent))
                        {
                            Console.WriteLine("ERROR: Failed to load BigBlindPercent value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }
                        Console.WriteLine("   BigBlindPercent={0}", configValue);
                        break;

                    case "bigblindturnperiod":
                        if (!int.TryParse(configValue, out this.bigBlindTurnPeriod))
                        {
                            Console.WriteLine("ERROR: Failed to load BigBlindTurnPeriod value from configuration. Invalid Value: {0}", configValue);
                            return false;
                        }
                        Console.WriteLine("   BigBlindTurnPeriod={0}", configValue);
                        break;

                    default:
                        Console.WriteLine("ERROR: Unknown configuration value {0}.", configName);
                        Console.WriteLine("ERROR: Line: {0}", configLine);
                        return false;
                }
            }

            if (this.StartingCash == -1)
            {
                Console.WriteLine("ERROR: Configuration file was missing required value \"StartingCash\". Please add it to your config.");
                return false;
            }

            if (this.MaxRounds == -1)
            {
                Console.WriteLine("ERROR: Configuration file was missing required value \"MaxRounds\". Please add it to your config.");
                return false;
            }

            if (this.BigBlindStart == -1)
            {
                Console.WriteLine("ERROR: Configuration file was missing required value \"BigBlindStart\". Please add it to your config.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load the players from the given stream
        /// </summary>
        /// <param name="playerConfig">The player TSV stream</param>
        /// <returns>True if load was successful, otherwise false</returns>
        private bool LoadPlayers(StreamReader playerConfig, out List<Player> players)
        {
            players = new List<Player>();

            while (!playerConfig.EndOfStream)
            {
                string configLine = playerConfig.ReadLine().Trim();
                if (string.IsNullOrEmpty(configLine) || configLine.StartsWith(";"))
                {
                    continue;
                }

                string[] configFields = configLine.Split('\t');

                if (configFields.Length != 3)
                {
                    Console.WriteLine("ERROR: Invalid player config file. Line is not in the correct format of <Player Name>\\t<Path to Assembly>\\t<Full Type Name>.");
                    Console.WriteLine("ERROR: Line: {0}", configLine);
                    return false;
                }

                string playerName = configFields[0];
                string assemblyPath = configFields[1];
                string typeName = configFields[2];

                foreach (var loadedPlayer in players)
                {
                    if (loadedPlayer.Name == playerName)
                    {
                        Console.WriteLine("ERROR: A player with the name \"{0}\" has already been loaded. Make sure each player has a distinct name.");
                        return false;
                    }
                }

                Assembly assembly;
                try
                {
                    if (!File.Exists(assemblyPath))
                    {
                        throw new IOException(string.Format("ERROR: Could not find assembly path {0} on disk!  Verify that your path is correct.", assemblyPath));
                    }

                    assembly = Assembly.LoadFrom(assemblyPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Failed to load RoboPokerPlayer assembly from file {0}.\nException {1}", assemblyPath, e.ToString());
                    return false;
                }

                object reflectedObject;
                try
                {
                    Type type = assembly.GetType(typeName, true);
                    ConstructorInfo ctor = type.GetConstructor(new Type[] { });
                    reflectedObject = ctor.Invoke(new object[] { });
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Failed to load type {0} from successfully loaded assembly {1}.\nException: {2}", typeName, assembly.FullName, e.ToString());
                    return false;
                }

                IRoboPoker player = reflectedObject as IRoboPoker;
                if (player == null)
                {
                    Console.WriteLine("ERROR: Player type {0} has not implemented the required {1} interface. Make sure you are using an up to date RoboPoker assembly.", typeName, typeof(IRoboPoker).Name);
                    return false;
                }

                Console.WriteLine("   {0} using implementation {1} is ready!", playerName, player.ImplementationName);
                players.Add(new Player(playerName, player));
            }

            if (players.Count < Constants.MIN_PLAYERS)
            {
                Console.WriteLine("ERROR: Too few players have been loaded. At least {0} players must be loaded to play a game of RoboPoker.", Constants.MIN_PLAYERS);
                return false;
            }

            if (players.Count > Constants.MAX_PLAYERS)
            {
                Console.WriteLine("ERROR: Too many players have been loaded. At most {0} players may be loaded to play a game of RoboPoker.", Constants.MAX_PLAYERS);
                return false;
            }

            return true;
        }
    }
}
