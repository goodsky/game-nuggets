using Common;
using GameData;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the SaveGameWindow
    /// </summary>
    public class SaveGameWindow : Window
    {
        public Text TitleText;

        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public InputField SaveNameInput;

        public Button SaveButton;

        public Button DeleteButton;

        public Button CancelButton;

        public override List<Button> Buttons => new List<Button> { SaveButton, DeleteButton, CancelButton };

        public override void Open(object data)
        {
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

            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            camera.FreezeCamera();
        }

        public override void Close()
        {
            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            camera.UnfreezeCamera();
        }

        private void SaveGame()
        {
            string saveName = SaveNameInput.text;
            GameLogger.Info("Saving game '{0}'.", saveName);

            GameSaveState state = Accessor.Game.SaveGame();
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
            foreach (string saveName in SavedGameLoader.GetSaveGames())
            {
                RectTransform listItem = Instantiate(ScrollViewRowTemplate, ScrollViewContent);

                Text listText = listItem.GetComponentInChildren<Text>();
                listText.text = saveName;

                Button listButton = listItem.GetComponent<Button>();
                listButton.OnSelect = () => SaveNameInput.text = saveName;
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
