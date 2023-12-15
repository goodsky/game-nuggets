using Campus;
using Common;
using Faculty;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the FacultyWindow
    /// </summary>
    public class FacultyWindow : Window
    {
        public Text TitleText;

        public RectTransform ScrollViewContent;

        public RectTransform ScrollViewRowTemplate;

        public Button CancelButton;

        public Text ClassroomUsageText;

        public Text LabUsageText;

        public Text TeachingScoreText;

        public Text ResearchScoreText;

        public Text PlaceholderText;

        public override List<Button> Buttons => new List<Button> { CancelButton };

        public override void Open(object data)
        {
            CancelButton.OnSelect = () => { SelectionManager.UpdateSelection(null); };

            ClearList();
            PopulateList();

            UpdateStats();

            Accessor.Camera.FreezeCamera();
        }

        public override void Close()
        {
            Accessor.Camera?.UnfreezeCamera();
        }

        public void UpdateStats()
        {
            FacultyManager manager = Accessor.Faculty;
            ClassroomUsageText.text = $"{manager.UsedClassroomCount}/{manager.TotalClassroomCount}";
            LabUsageText.text = $"{manager.UsedLabsCount}/{manager.TotalLabsCount}";
            TeachingScoreText.text = manager.UniversityTeachingScore.ToString();
            ResearchScoreText.text = manager.UniversityResearchScore.ToString();
        }

        public void UpdateList()
        {
            ClearList();
            PopulateList();
        }

        private void PopulateList()
        {
            bool anyFacultyExists = false;
            foreach (HiredFaculty faculty in Accessor.Faculty.HiredFaculty)
            {
                anyFacultyExists = true;

                RectTransform listItem = Instantiate(ScrollViewRowTemplate, ScrollViewContent);
                FacultyRow facultyRow = listItem.GetComponent<FacultyRow>();

                Sprite facultyHeadshot = Accessor.Faculty.GetHeadshotForFaculty(faculty.Id);
                facultyRow.Initialize(this, faculty, facultyHeadshot, Accessor);
                
                listItem.gameObject.SetActive(true);
            }

            PlaceholderText.enabled = !anyFacultyExists;
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
