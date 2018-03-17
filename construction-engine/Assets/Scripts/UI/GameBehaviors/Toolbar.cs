using UnityEngine;

namespace UI
{
    /// <summary>
    /// The base class for the UI Toolbar.
    /// Extend this class to populate menus in the 'PopulateMenus' method.
    /// </summary>
    public class Toolbar : MonoBehaviour
    {
        public GameObject SubMenu { get; set; }

        public GameObject Pip { get; set; }

        /// <summary>Sub Menu buttons that are currently active.</summary>
        private GameObject _activeSubMenuButtons;

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
            // TODO: move this to a global selection manager
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Selectable.SelectionManager.UpdateSelection(null);
            }
        }

        /// <summary>
        /// Pop up a sub-menu.
        /// </summary>
        /// <param name="buttonGroup">The button group to populate with the sub-menu.</param>
        public void PopUpSubMenu(GameObject buttonGroup)
        {
            SubMenu.SetActive(true);

            var selected = Selectable.SelectionManager.Selected;
            if (selected != null)
            {
                Pip.transform.SetParent(selected.gameObject.transform, false);
                Pip.transform.SetAsFirstSibling();
                Pip.SetActive(true);
            }

            buttonGroup.SetActive(true);
            _activeSubMenuButtons = buttonGroup;
        }

        /// <summary>
        /// Pop down the sub-menu and its button group.
        /// </summary>
        public void PopDownSubMenu()
        {
            SubMenu.SetActive(false);
            Pip.SetActive(false);

            _activeSubMenuButtons.SetActive(false);
        }
    }
}
