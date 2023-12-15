using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly string SavedScenarioThumbnailFile = "thumbnail.png";
        private static readonly string SavedScenarioDescriptionFile = "description.txt";

        public static IEnumerable<SavedScenario> GetSavedScenarios()
        {
            var scenarios = new List<SavedScenario>();

            var scenarioRootPath = Path.Combine(Application.streamingAssetsPath, SavedScenarioDirectory);
            IEnumerable<string> dirs = Directory.GetDirectories(scenarioRootPath);
            GameLogger.Info("Found scenarios at '{0}'. [{1}]", scenarioRootPath, string.Join(",", dirs.Select(d => Path.GetFileName(d))));

            foreach (var scenarioDir in dirs)
            {
                var scenarioName = Path.GetFileName(scenarioDir);
                var scenarioFilePath = Path.Combine(scenarioDir, SavedScenarioSaveFile);
                var scenarioThumbnailPath = Path.Combine(scenarioDir, SavedScenarioThumbnailFile);
                var scenarioDescriptionPath = Path.Combine(scenarioDir, SavedScenarioDescriptionFile);

                if (!File.Exists(scenarioFilePath))
                {
                    GameLogger.Error("Could not find save file {0}!", scenarioFilePath);
                    continue;
                }

                Sprite scenarioThumbnail = default;
                if (File.Exists(scenarioThumbnailPath))
                {
                    var bytes = File.ReadAllBytes(scenarioThumbnailPath);
                    Texture2D texture = new Texture2D(100, 100);
                    if (texture.LoadImage(bytes))
                    {
                        scenarioThumbnail = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0f, 0f));
                    }
                    else
                    {
                        GameLogger.Warning("Corrupt image for scenario {0}.", scenarioName);
                    }
                }
                else
                {
                    GameLogger.Warning("No image scenario {0}.", scenarioName);
                }

                TextAsset scenarioDescription = default;
                if (File.Exists(scenarioDescriptionPath))
                {
                    var text = File.ReadAllText(scenarioDescriptionPath);
                    scenarioDescription = new TextAsset(text);
                }
                else
                {
                    GameLogger.Warning("No description for scenario {0}.", scenarioName);
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
