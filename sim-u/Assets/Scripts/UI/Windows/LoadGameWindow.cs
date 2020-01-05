using Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the LoadGameWindow
    /// </summary>
    public class LoadGameWindow : Window
    {
        public Text TitleText;

        public LoadGameBox LoadGameBox;

        public override List<Button> Buttons => new List<Button> { LoadGameBox.LoadButton, LoadGameBox.DeleteButton, LoadGameBox.CancelButton };

        private IEnumerable<string> validSaveNames;

        public override void Open(object data)
        {
            LoadGameBox.Initialize(() => { SelectionManager.UpdateSelection(null); });

            Accessor.Camera.FreezeCamera();
        }

        public override void Close()
        {
            Accessor.Camera?.UnfreezeCamera();
        }
    }
}
