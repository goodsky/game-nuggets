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
        private static readonly string StreamingAssetsDirectory = "StreamingAssets";
        private static readonly string SavedScenarioDirectory = "Scenarios";
        private static readonly string SavedScenarioSaveFile = "save.simu";
        private static readonly string SavedScenarioThumbnailFile = "thumbnail";
        private static readonly string SavedScenarioDescriptionFile = "description";

        public static IEnumerable<SavedScenario> GetSavedScenarios()
        {
            var scenarios = new List<SavedScenario>();

            IEnumerable<string> dirs = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, SavedScenarioDirectory));
            GameLogger.Info("Found scenarios: {0}", string.Join(",", dirs));

            foreach (var scenarioDir in dirs)
            {
                var scenarioName = Path.GetFileName(scenarioDir);
                var scenarioFilePath = Path.Combine(scenarioDir, SavedScenarioSaveFile);
                var scenarioThumbnailPath = Path.Combine(StreamingAssetsDirectory, SavedScenarioDirectory, scenarioName, SavedScenarioThumbnailFile);
                var scenarioDescriptionPath = Path.Combine(StreamingAssetsDirectory, SavedScenarioDirectory, scenarioName, SavedScenarioDescriptionFile);

                var scenarioThumbnail = Resources.Load<Sprite>(scenarioThumbnailPath);
                var scenarioDescription = Resources.Load<TextAsset>(scenarioDescriptionPath);

                if (!File.Exists(scenarioFilePath))
                {
                    GameLogger.Error("Could not find save file {0}!", scenarioFilePath);
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
                        SavePath = scenarioFilePath,
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
