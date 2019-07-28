using Campus;
using Common;
using Faculty;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the FacultyHiringWindow
    /// </summary>
    public class FacultyHiringWindow : Window
    {
        public Text TitleText;

        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public Button CancelButton;

        public override List<Button> Buttons => new List<Button> { CancelButton };

        public override void Open(object data)
        {
            CancelButton.OnSelect = () => { SelectionManager.UpdateSelection(null); };

            UpdateList();

            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            camera.FreezeCamera();
        }

        public override void Close()
        {
            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            camera.UnfreezeCamera();
        }

        public void UpdateList()
        {
            ClearList();
            PopulateList();
        }

        private void PopulateList()
        {
            foreach (GeneratedFaculty faculty in Accessor.Faculty.AvailableFaculty)
            {
                RectTransform listItem = Instantiate(ScrollViewRowTemplate, ScrollViewContent);
                FacultyHiringRow facultyRow = listItem.GetComponent<FacultyHiringRow>();

                Sprite facultyHeadshot = Accessor.Faculty.GetHeadshotForFaculty(faculty.Id);
                facultyRow.Initialize(this, faculty, facultyHeadshot, Accessor);
                
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
    }
}
