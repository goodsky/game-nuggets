using RoboPoker;
using TableRunner;

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTest
{
    /// <summary>
    /// Class to test utilities
    /// Global functionality that doesn't fit into any other test class
    /// </summary>
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void LoggerTest()
        {
            const string allFile = "./allFile.log";
            const string gameFile = "./gameFile.log";
            const string infoWarningFile = "./infoWarningFile.log";

            LogLevel all = LogLevel.None;
            all |= LogLevel.Error;
            all |= LogLevel.Game;
            all |= LogLevel.Info;
            all |= LogLevel.Warning;

            LogLevel infoWarning = LogLevel.None;
            infoWarning |= LogLevel.Info;
            infoWarning |= LogLevel.Warning;

            StreamWriter allStream = new StreamWriter(allFile);
            Logger.CreateLogStream(allStream, all);

            StreamWriter gameStream = new StreamWriter(gameFile);
            Logger.CreateLogStream(gameStream, LogLevel.Game);

            StreamWriter infoWarningStream = new StreamWriter(infoWarningFile);
            Logger.CreateLogStream(infoWarningStream, infoWarning);

            // Run some test logs
            Logger.Log(LogLevel.Error, "{{{0}}}", "ERROR");
            Logger.Log(LogLevel.Warning, "{{{0}}}", "WARNING");
            Logger.Log(LogLevel.Info, "{{{0}}}", "INFO");
            Logger.Log(LogLevel.Game, "{{{0}}}", "GAME");

            Logger.Flush();
            Logger.Finalize();

            // Verify the logs are correct
            string allText;
            string gameText;
            string infoWarningText;

            using (StreamReader allStreamReader = new StreamReader(allFile))
            {
                allText = allStreamReader.ReadToEnd();
            }

            using (StreamReader gameStreamReader = new StreamReader(gameFile))
            {
                gameText = gameStreamReader.ReadToEnd();
            }

            using (StreamReader infoWarningReader = new StreamReader(infoWarningFile))
            {
                infoWarningText = infoWarningReader.ReadToEnd();
            }

            Assert.IsTrue(allText.Contains("{ERROR}") && allText.Contains("{WARNING}") && allText.Contains("{INFO}") && allText.Contains("{GAME}"), "All text log was broken.");
            Assert.IsTrue(!gameText.Contains("{ERROR}") && !gameText.Contains("{WARNING}") && !gameText.Contains("{INFO}") && gameText.Contains("{GAME}"), "Game log was broken.");
            Assert.IsTrue(!infoWarningText.Contains("{ERROR}") && infoWarningText.Contains("{WARNING}") && infoWarningText.Contains("{INFO}") && !infoWarningText.Contains("{GAME}"), "InfoWarning log was broken.");

            // Clean up
            File.Delete(allFile);
            File.Delete(gameFile);
            File.Delete(infoWarningFile);
        }
    }
}
