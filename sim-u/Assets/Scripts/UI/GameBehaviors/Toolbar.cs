using Common;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Behaviour for the toolbar at the bottom of the screen.
    /// </summary>
    public class Toolbar : MonoBehaviour
    {
        /// <summary>The second layer toolbar that pops up.</summary>
        public GameObject SubMenu { get; set; }

        /// <summary>The little selection indicator between main and sub menu.</summary>
        public GameObject Pip { get; set; }

        /// <summary>Sub Menu buttons that are currently active.</summary>
        private ButtonGroup _subMenuButtonGroup;

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
            // TODO: move this to a global selection manager
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SelectionManager.UpdateSelection(null);
            }
        }

        /// <summary>
        /// Opens the sub-menu with a set of buttons.
        /// </summary>
        /// <param name="buttonGroup">The button group to populate with the sub-menu.</param>
        public void OpenSubMenu(ButtonGroup buttonGroup)
        {
            if (buttonGroup == _subMenuButtonGroup)
                return;

            CloseSubMenu();

            SubMenu.SetActive(true);

            var selected = SelectionManager.Selected;
            if (selected != null)
            {
                Pip.transform.SetParent(selected.gameObject.transform, false);
                Pip.transform.SetAsFirstSibling();
                Pip.SetActive(true);

                foreach (var button in buttonGroup.Buttons)
                {
                    button.SelectionParent = selected;
                }
            }

            buttonGroup.gameObject.SetActive(true);
            _subMenuButtonGroup = buttonGroup;
        }

        /// <summary>
        /// Closes the sub-menu.
        /// </summary>
        public void CloseSubMenu()
        {
            SubMenu.SetActive(false);
            Pip.SetActive(false);

            if (_subMenuButtonGroup != null)
            {
                _subMenuButtonGroup.gameObject.SetActive(false);
                _subMenuButtonGroup = null;
            }
        }
    }
}
