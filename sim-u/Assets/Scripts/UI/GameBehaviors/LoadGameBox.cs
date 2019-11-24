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
    public class LoadGameBox : Common.Selectable
    {
        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public InputField LoadNameInput;

        public Button LoadButton;

        public Button DeleteButton;

        public Button CancelButton;

        private IEnumerable<SavedGame> _savedGames;

        public void Initialize(Action cancelAction)
        {
            LoadNameInput.onValueChanged.RemoveAllListeners();
            LoadNameInput.onValueChanged.AddListener(
                newValue =>
                {
                    if (IsValidLoadName(newValue))
                    {
                        LoadButton.Enable();
                        DeleteButton.Enable();
                    }
                    else
                    {
                        LoadButton.Disable();
                        DeleteButton.Disable();
                    }
                });

            LoadButton.OnSelect = LoadGame;

            DeleteButton.OnSelect = DeleteGame;

            CancelButton.OnSelect = cancelAction;

            ClearList();
            PopulateList();
        }

        private void LoadGame()
        {
            string savePath = GetSaveGamePath(LoadNameInput.text);
            Game.SavedGameInfo = new SaveInfo(savePath);

            // Reload the game scene to force the game load.
            GameLogger.Info("Setting global game save variable to '{0}'. Will be picked up upon scene reload.", savePath);
            SceneManager.LoadScene(Constant.GameSceneName);
        }

        private void DeleteGame()
        {
            string savePath = GetSaveGamePath(LoadNameInput.text);
            GameLogger.Info("Deleting game '{0}'.", savePath);

            SavedGameLoader.DeleteFromDisk(savePath);
        }

        private void PopulateList()
        {
            _savedGames = SavedGameLoader.GetSaveGames();
            foreach (string saveName in _savedGames.Select(save => save.Name))
            {
                RectTransform listItem = Instantiate(ScrollViewRowTemplate, ScrollViewContent);

                Text listText = listItem.GetComponentInChildren<Text>();
                listText.text = saveName;

                Button listButton = listItem.GetComponent<Button>();
                listButton.OnSelect = () => LoadNameInput.text = saveName;
                listButton.SelectionParent = this;

                listItem.gameObject.SetActive(true);
            }
        }

        private void ClearList()
        {
            foreach (RectTransform listItem in ScrollViewContent)
            {
                if (listItem != ScrollViewRowTemplate)
                {
                    Destroy(listItem.gameObject);
                }
            }
        }

        private bool IsValidLoadName(string loadName)
        {
            return _savedGames.SingleOrDefault(save => save.Name == loadName) != null;
        }

        private string GetSaveGamePath(string loadName)
        {
            return _savedGames.Single(save => save.Name == loadName).SavePath;
        }
    }
}