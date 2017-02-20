using System;
using System.Collections.Generic;
using System.IO;

namespace TableRunner
{
    /// <summary>
    /// Types of log messages
    /// </summary>
    [Flags]
    internal enum LogLevel
    {
        None    = 0,       // You should probably never set a log to this value
        Game    = 1<<1,       // Game critical steps that can be used to see just game steps
        Info    = 1<<2,       // Extra verbose information to trace along each step
        Warning = 1<<3,    // Warning information
        Error   = 1<<4       // Critical errors! Things you can't ignore
    }

    /// <summary>
    /// Class to write logs out to a file.
    /// Based on the setup configuration, the logs will be swallowed, printed to console, or printed to different files
    /// </summary>
    static class Logger
    {
        private static List<LogStream> Streams = new List<LogStream>();

        /// <summary>
        /// Add a log stream to output to.
        /// </summary>
        /// <param name="stream">The stream to output to. If null, will write to Console.</param>
        /// <param name="levels">The levels that should be output to this file</param>
        public static void CreateLogStream(StreamWriter stream, LogLevel levels)
        {
            Streams.Add(new LogStream(stream, levels));
        }

        /// <summary>
        /// Write a log message
        /// </summary>
        /// <param name="level">The level we are logging at</param>
        /// <param name="message">The message to log</param>
        /// <param name="args">The arguments to the message</param>
        public static void Log(LogLevel level, string message, params object[] args)
        {
            foreach (LogStream stream in Streams)
            {
                if (stream.levels.HasFlag(level))
                {
                    stream.Log(message, args);
                }
            }
        }

        /// <summary>
        /// Flush all logs
        /// </summary>
        public static void Flush()
        {
            foreach (LogStream stream in Streams)
            {
                stream.Flush();
            }
        }

        /// <summary>
        /// Close all logs
        /// </summary>
        public static void Finalize()
        {
            foreach (LogStream stream in Streams)
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Keep track of the streams we are logging to
        /// </summary>
        private class LogStream
        {
            public StreamWriter stream { get; private set; }

            public LogLevel levels { get; private set; }

            public LogStream(StreamWriter stream, LogLevel levels)
            {
                this.stream = stream;
                this.levels = levels;
            }

            public void Flush()
            {
                if (this.stream != null)
                {
                    this.stream.Flush();
                }
            }

            public void Close()
            {
                if (this.stream != null)
                {
                    this.stream.Close();
                }
            }

            public void Log(string message, params object[] args)
            {
                if (this.stream != null)
                {
                    this.stream.WriteLine(message, args);
                }
                else
                {
                    Console.WriteLine(message, args);
                }
            }
        }
    }
}
