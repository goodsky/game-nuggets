using Common;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// Load scenarios so we can have nice starting points for games.
    /// </summary>
    public static class SavedScenarioLoader
    {
        private static readonly string SavedScenarioDirectory = "Scenarios";
        private static readonly string SavedScenarioSaveFile = "save.simu";
        private static readonly string SavedScenarioThumbnailFile = "thumbnail";
        private static readonly string SavedScenarioDescriptionFile = "description";

        public static IEnumerable<SavedScenario> GetSavedScenarios()
        {
            var scenarios = new List<SavedScenario>();

            IEnumerable<string> dirs = Directory.GetDirectories(Path.Combine(Application.dataPath, SavedScenarioDirectory));
            GameLogger.Info("Found scenarios: {0}", string.Join(",", dirs));

            foreach (var scenarioDir in dirs)
            {
                var scenarioName = Path.GetFileName(scenarioDir);
                var scenarioFile = Path.Combine(Application.dataPath, SavedScenarioDirectory, scenarioName, SavedScenarioSaveFile);

                var scenarioThumbnail = Resources.Load<Sprite>(Path.Combine(SavedScenarioDirectory, scenarioName, SavedScenarioThumbnailFile));
                var scenarioDescription = Resources.Load<TextAsset>(Path.Combine(SavedScenarioDirectory, scenarioName, SavedScenarioDescriptionFile));

                if (!File.Exists(scenarioFile))
                {
                    GameLogger.Error("Could not find save file {0}!", scenarioFile);
                    continue;
                }

                if (scenarioThumbnail == null)
                {
                    GameLogger.Warning("Failed to load image for scenario {0}.", scenarioName);
                }

                if (scenarioDescription == null)
                {
                    GameLogger.Warning("Failed to load description for scenario {0}.", scenarioName);
                }

                scenarios.Add(
                    new SavedScenario
                    {
                        Name = scenarioName,
                        SavePath = scenarioFile,
                        Thumbnail = scenarioThumbnail,
                        Description = scenarioDescription?.text ?? "No description.",
                    });
            }

            return scenarios;
        }
    }

    public class SavedScenario
    {
        public string Name { get; set; }

        public string SavePath { get; set; }

        public Sprite Thumbnail { get; set; }

        public string Description { get; set; }
    }
}
