using Common;
using System.Collections.Generic;
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

        public SaveGameBox SaveGameBox;

        public override List<Button> Buttons => new List<Button> { SaveGameBox.SaveButton, SaveGameBox.DeleteButton, SaveGameBox.CancelButton };

        public override void Open(object data)
        {
            SaveGameBox.Initialize(Accessor, () => { SelectionManager.UpdateSelection(null); });

            Accessor.Camera.FreezeCamera();
        }

        public override void Close()
        {
            Accessor.Camera?.UnfreezeCamera();
        }
    }
}
