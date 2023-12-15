using Common;
using GameData;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The box where you can load games from
    /// </summary>
    public class SaveGameBox : Common.Selectable
    {
        private GameAccessor _accessor;

        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public InputField SaveNameInput;

        public Button SaveButton;

        public Button DeleteButton;

        public Button CancelButton;

        public void Initialize(GameAccessor accessor, Action cancelAction)
        {
            _accessor = accessor;

            SaveNameInput.onValueChanged.RemoveAllListeners();
            SaveNameInput.onValueChanged.AddListener(
                newValue =>
                {
                    if (IsValidSaveName(newValue))
                    {
                        SaveButton.Enable();
                        DeleteButton.Enable();
                    }
                    else
                    {
                        SaveButton.Disable();
                        DeleteButton.Disable();
                    }
                });

            SaveButton.OnSelect = SaveGame;

            DeleteButton.OnSelect = DeleteGame;

            CancelButton.OnSelect = () => { SelectionManager.UpdateSelection(null); };

            ClearList();
            PopulateList();
        }

        private void SaveGame()
        {
            string saveName = SaveNameInput.text;
            GameLogger.Info("Saving game '{0}'.", saveName);

            GameSaveState state = _accessor.Game.SaveGame();
            SavedGameLoader.WriteToDisk(saveName, state);
        }

        private void DeleteGame()
        {
            string saveName = SaveNameInput.text;
            GameLogger.Info("Deleting game '{0}'.", saveName);

            SavedGameLoader.DeleteFromDisk(saveName);
        }

        private void PopulateList()
        {
            foreach (SavedGame save in SavedGameLoader.GetSaveGames())
            {
                RectTransform listItem = Instantiate(ScrollViewRowTemplate, ScrollViewContent);

                Text listText = listItem.GetComponentInChildren<Text>();
                listText.text = save.Name;

                Button listButton = listItem.GetComponent<Button>();
                listButton.OnSelect = () => SaveNameInput.text = save.Name;
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

        private bool IsValidSaveName(string saveName)
        {
            var r = new Regex("^[a-zA-Z0-9 _-]*$");
            return !string.IsNullOrEmpty(saveName) && r.IsMatch(saveName);
        }
    }
}