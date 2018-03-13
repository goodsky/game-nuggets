using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Common
{
    [Flags]
    internal enum LogLevel
    {
        Info =      1 << 1,     // Verbose information
        Warning =   1 << 2,     // Abnormal behavior
        Error =     1 << 3,     // Critical errors
        All = Info | Warning | Error,
    }

    /// <summary>
    /// Class to write logs out to a file or the Unity editor.
    /// Based on the setup configuration, the logs will be swallowed, printed to console, or printed to different files.
    /// </summary>
    static class GameLogger
    {
        private static List<LogStream> Streams = new List<LogStream>();

        /// <summary>
        /// Create a stream in the My Documents directory of a Windows machine.
        /// </summary>
        /// <param name="fileprefix">File prefix</param>
        /// <param name="levels">Logging levels to print</param>
        public static void CreateMyDocumentsStream(string fileprefix, LogLevel levels)
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var myFolder = Path.Combine(myDocuments, "ConstructionEngine");

            if (!Directory.Exists(myFolder))
            {
                Directory.CreateDirectory(myFolder);
            }

            var files = Directory.GetFiles(myFolder, fileprefix + "*.log");
            Array.Sort(files);

            int logCount = files.Length;
            for (int i = logCount; i >= 3; --i) // Keep at most 3 log files
            {
                // I'm making an assumption that files are sorted by DateTime in their filename
                File.Delete(files[logCount - i]);
            }

            var logPath = Path.Combine(myFolder, string.Format("{0}_{1}.log", fileprefix, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
            var stream = new StreamWriter(logPath);

            CreateLogStream(stream, levels);
        }

        /// <summary>
        /// Create a log stream to the Unity Editor debug window.
        /// </summary>
        /// <param name="levels">Logging levels to print.</param>
        public static void CreateUnityLogger(LogLevel levels)
        {
            CreateLogStream(null, levels);
        }

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
        /// Write a log message.
        /// </summary>
        /// <param name="level">The level we are logging at</param>
        /// <param name="message">The message to log</param>
        /// <param name="args">The arguments to the message</param>
        public static void Log(LogLevel level, string message, params object[] args)
        {
            foreach (LogStream stream in Streams)
            {
                if ((stream.levels & level) != 0)
                {
                    stream.Log(message, args);
                }
            }
        }

        /// <summary>
        /// Write a log message at Info level.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">The arguments to the message</param>
        public static void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        /// <summary>
        /// Write a log message at Warning level.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">The arguments to the message</param>
        public static void Warning(string message, params object[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// Write a log message at Error level.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">The arguments to the message</param>
        public static void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Flush all logs.
        /// </summary>
        public static void Flush()
        {
            foreach (LogStream stream in Streams)
            {
                stream.Flush();
            }
        }

        /// <summary>
        /// Close all logs.
        /// </summary>
        public static void Close()
        {
            foreach (LogStream stream in Streams)
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Keep track of the streams we are logging to.
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
                    Debug.Log(string.Format(message, args));
                }
            }
        }
    }
}
