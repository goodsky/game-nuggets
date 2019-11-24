using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The box where you can load games from
    /// </summary>
    public class NewGameBox : Common.Selectable
    {
        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public Image ScenarioImage;

        public Text ScenarioDescription;

        public Button StartButton;

        public Button CancelButton;

        private SavedScenario _selectedScenario = null;

        public void Initialize(Action cancelAction)
        {
            StartButton.OnSelect = StartGame;
            CancelButton.OnSelect = cancelAction;

            ClearList();
            PopulateList();
        }

        private void StartGame()
        {
            string savePath = _selectedScenario.SavePath;
            Game.SavedGameInfo = new SaveInfo(savePath);

            // Reload the game scene to force the game load.
            GameLogger.Info("Setting global game save variable to '{0}'. Will be picked up upon scene reload.", savePath);
            SceneManager.LoadScene(Constant.GameSceneName);
        }

        private void PopulateList()
        {
            foreach (SavedScenario scenario in SavedScenarioLoader.GetSavedScenarios())
            {
                RectTransform listItem = Instantiate(ScrollViewRowTemplate, ScrollViewContent);

                Text listText = listItem.GetComponentInChildren<Text>();
                listText.text = scenario.Name;

                Button listButton = listItem.GetComponent<Button>();
                listButton.OnSelect = () =>
                {
                    StartButton.Enable();
                    _selectedScenario = scenario;

                    ScenarioImage.sprite = scenario.Thumbnail;
                    ScenarioDescription.text = scenario.Description;
                };
                listButton.SelectionParent = this;

                listItem.gameObject.SetActive(true);
            }
        }

        private void ClearList()
        {
            // StartButton.Disable();

            foreach (RectTransform listItem in ScrollViewContent)
            {
                if (listItem != ScrollViewRowTemplate)
                {
                    Destroy(listItem.gameObject);
                }
            }
        }
    }
}