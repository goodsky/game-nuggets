using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniversitySim.Utilities
{
    /// <summary>
    /// Static logging class.
    /// It will log to text files
    /// </summary>
    public class Logger
    {
        // The log stream
        private static StreamWriter log = null;

        /// <summary>
        /// Start logging and create a stream
        /// </summary>
        /// <param name="path">Path to the root directory where logs are stored</param>
        /// <param name="keepStored">Number of old logs to keep stored</param>
        public static void Initialize(string path, int keepStored)
        {
            if (log != null)
            {
                Log(LogLevel.Warning, "Logger", "Attempted to open Logger again.");
            }
            else
            {
                var files = Directory.GetFiles(path, Constants.LOG_FILE + "*.log");
                if (files.Length >= keepStored )
                {
                    // I'm making an assumption that files are sorted basically by DateTime in their filename
                    Array.Sort(files);
                    File.Delete(files[0]);
                }

                string logPath = Path.Combine(path, string.Format("{0}_{1}.log", Constants.LOG_FILE,DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                log = new StreamWriter(logPath);

                Log(LogLevel.Info, "Logger", "Logging Started!!!");
            }
        }

        /// <summary>
        /// Stop logging and close the stream
        /// </summary>
        public static void Close()
        {
            if (log != null)
            {
                Log(LogLevel.Info, "Logger", "Log closed.");
                log.Close();
            }
        }

        /// <summary>
        /// Add a line to the log
        /// Flush after errors
        /// </summary>
        /// <param name="level">Level of the message. Info, Warning or Error</param>
        /// <param name="message">Text of the message</param>
        public static void Log(LogLevel level, string title, string message)
        {
            if (log == null)
            {
                return;
            }

            log.WriteLine("{0}\t{1}\t{2}\t{3}", DateTime.Now.ToString(), level.ToString(), title, message);

            if (level == LogLevel.Error)
            {
                log.Flush();
            }
        }
    }
}
