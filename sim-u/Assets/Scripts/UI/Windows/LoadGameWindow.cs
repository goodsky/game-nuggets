using Common;
using GameData;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the LoadGameWindow
    /// </summary>
    public class LoadGameWindow : Window
    {
        public Text TitleText;

        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public InputField LoadNameInput;

        public Button LoadButton;

        public Button DeleteButton;

        public Button CancelButton;

        public override List<Button> Buttons => new List<Button> { LoadButton, DeleteButton, CancelButton };

        public override void Open(object data)
        {
            LoadNameInput.onValueChanged.RemoveAllListeners();
            LoadNameInput.onValueChanged.AddListener(
                newValue => 
                {
                    LoadButton.enabled = IsValidLoadName(newValue);
                    DeleteButton.enabled = IsValidLoadName(newValue);
                });

            LoadButton.enabled = false;
            LoadButton.OnSelect = LoadGame;

            DeleteButton.enabled = false;
            DeleteButton.OnSelect = DeleteGame;

            CancelButton.OnSelect = () => { SelectionManager.UpdateSelection(null); };

            ClearList();
            PopulateList();
        }

        public override void Close()
        {
        }

        private void LoadGame()
        {
            string saveName = LoadNameInput.text;
            Game.SavedGameName = saveName;

            // Reload the game scene to force the game load.
            GameLogger.Info("Setting global game save variable to '{0}'. Will be picked up upon scene reload.", saveName);
            SceneManager.LoadScene(Constant.GameSceneName);
        }

        private void DeleteGame()
        {
            string saveName = LoadNameInput.text;
            GameLogger.Info("Deleting game '{0}'.", saveName);

            SavedGameLoader.DeleteFromDisk(saveName);
        }

        private void PopulateList()
        {
            foreach (string saveName in SavedGameLoader.GetSaveGames())
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
            var r = new Regex("^[a-zA-Z0-9 _-]*$");
            return !string.IsNullOrEmpty(loadName) && r.IsMatch(loadName);
        }
    }
}
