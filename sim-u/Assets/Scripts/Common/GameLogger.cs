using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Common
{
    internal enum LogLevel
    {
        Info = 0,       // Verbose information
        Warning,        // Abnormal behavior
        Error,          // Critical errors
                        // It is important to order these enums from most verbose to least verbose!
    }

    /// <summary>
    /// Class to write logs out to a file or the Unity editor.
    /// Based on the setup configuration, the logs will be swallowed, printed to console, or printed to different files.
    /// </summary>
    internal class GameLogger : MonoBehaviour
    {
        private static readonly int NumberOfFilesToKeepInMyDocuments = 10;
        private static readonly string LogsFolder = "SimU";

        private static List<LogStream> Streams = new List<LogStream>();
        private static string[] LogLevelStrings = Enum.GetNames(typeof(LogLevel));

        /// <summary>
        /// Ensure the singleton game logger gameobject exists.
        /// </summary>
        public static void EnsureSingletonExists()
        {
            GameLogger[] existingLoggers = FindObjectsOfType<GameLogger>();

            if (existingLoggers.Length == 0)
            {
                // Create the singleton GameLogger.
                var logger = new GameObject(nameof(GameLogger));
                logger.AddComponent<GameLogger>();

                if (Application.isEditor)
                {
                    GameLogger.CreateUnityLogger(LogLevel.Info);
                }
                GameLogger.CreateMyDocumentsStream("debug", LogLevel.Info);
            }
            else if (existingLoggers.Length == 1)
            {
                GameLogger.Info("GameLogger already exists in scene.");
            }
            else if (existingLoggers.Length > 1)
            {
                GameLogger.FatalError("More than one GameLogger exists in the scene!");
            }
        }

        /// <summary>
        /// Unity ctor (kindof)
        /// </summary>
        protected void Awake()
        {
            Application.logMessageReceived += HandleUnityLog;
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Unity dtor (kindof)
        /// </summary>
        protected void OnApplicationQuit()
        {
            GameLogger.Info("Application is quiting.");
            GameLogger.Close();
        }

        /// <summary>
        /// Route the Unity Exception and Assert feed to the log.
        /// </summary>
        /// <param name="logString">The Unity log string.</param>
        /// <param name="stackTrace">The Unity stack trace.</param>
        /// <param name="type">The type of log.</param>
        private static void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Assert)
            {
                Error("Unity Exception: {0} {1}", logString, stackTrace);
            }
        }

        /// <summary>
        /// Create a stream in the My Documents directory of a Windows machine.
        /// </summary>
        /// <param name="fileprefix">File prefix</param>
        /// <param name="levels">Logging levels to print</param>
        public static void CreateMyDocumentsStream(string fileprefix, LogLevel levels)
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var myFolder = Path.Combine(myDocuments, LogsFolder);

            if (!Directory.Exists(myFolder))
            {
                Directory.CreateDirectory(myFolder);
            }

            var files = Directory.GetFiles(myFolder, fileprefix + "*.log");
            Array.Sort(files);

            int logCount = files.Length;
            for (int i = logCount; i >= NumberOfFilesToKeepInMyDocuments; --i)
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
                if (level >= stream._level)
                {
                    stream.Log(level, message, args);
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
        /// Write a log message at Error level and then quit the application.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">The arguments to the message</param>
        public static void FatalError(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
            Log(LogLevel.Error, "FATAL ERROR: ABORTING.");

            Application.Quit();

            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.isPaused = true;
            }
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
            public StreamWriter _stream { get; private set; }

            public LogLevel _level { get; private set; }

            public LogStream(StreamWriter stream, LogLevel levels)
            {
                _stream = stream;
                _level = levels;
            }

            public void Flush()
            {
                if (_stream != null)
                {
                    _stream.Flush();
                }
            }

            public void Close()
            {
                if (_stream != null)
                {
                    _stream.Close();
                    _stream = null;
                }
            }

            public void Log(LogLevel level, string message, params object[] args)
            {
                if (_stream != null)
                {
                    var formattedMessage = string.Format("{0}\t{1}\t{2}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.f"), LogLevelStrings[(int)level], message);
                    _stream.WriteLine(formattedMessage, args);
                }
                else
                {
                    switch (level)
                    {
                        case LogLevel.Info:
                            Debug.LogFormat(message, args);
                            break;
                        case LogLevel.Warning:
                            Debug.LogWarningFormat(message, args);
                            break;
                        case LogLevel.Error:
                            Debug.LogErrorFormat(message, args);
                            break;
                    }
                }
            }
        }
    }
}
