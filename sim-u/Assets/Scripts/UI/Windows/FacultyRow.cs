using Common;
using Faculty;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A row in the <see cref="FacultyWindow"/>.
    /// </summary>
    public class FacultyRow : MonoBehaviour
    {
        private FacultyWindow _parent;

        private HiredFaculty _faculty;

        public Image Headshot;

        public Text NameText;

        public Text StatsText;

        public Text ClassesCounter;

        public UnityEngine.UI.Button DropButton;

        public UnityEngine.UI.Button ClassesMinusButton;

        public UnityEngine.UI.Button ClassesPlusButton;

        public Text ResearchCounter;

        public UnityEngine.UI.Button ResearchMinusButton;

        public UnityEngine.UI.Button ResearchPlusButton;

        public void Initialize(FacultyWindow parent, HiredFaculty faculty, Sprite headshot, GameAccessor accessor)
        {
            _parent = parent;
            _faculty = faculty;
            Headshot.sprite = headshot;
            NameText.text = faculty.Name;
            UpdateStatsText();
            UpdateClassesCounter();
            UpdateResearchCounter();

            FacultyManager manager = accessor.Faculty;

            DropButton.onClick.AddListener(() =>
            {
                manager.DropFaculty(faculty);
                _parent.UpdateList();
            });

            ClassesMinusButton.onClick.AddListener(() =>
            {
                manager.RemoveTeaching(faculty);
                UpdateClassesCounter();
            });

            ClassesPlusButton.onClick.AddListener(() =>
            {
                manager.AddTeaching(faculty);
                UpdateClassesCounter();
            });

            ResearchMinusButton.onClick.AddListener(() =>
            {
                manager.RemoveResearch(faculty);
                UpdateResearchCounter();
            });

            ResearchPlusButton.onClick.AddListener(() =>
            {
                manager.AddResearch(faculty);
                UpdateResearchCounter();
            });
        }

        private void UpdateStatsText()
        {
            StatsText.text =
                $"Salary: ${_faculty.SalaryPerYear:n0} /yr\n" +
                $"Teaching: {_faculty.TeachingScore}\n" +
                $"Research: {_faculty.ResearchScore}\n";
        }

        private void UpdateClassesCounter()
        {
            ClassesCounter.text = _faculty.TeachingSlots.ToString();
            _parent.UpdateStats();
        }

        private void UpdateResearchCounter()
        {
            ResearchCounter.text = _faculty.ResearchSlots.ToString();
            _parent.UpdateStats();
        }
    }
}
