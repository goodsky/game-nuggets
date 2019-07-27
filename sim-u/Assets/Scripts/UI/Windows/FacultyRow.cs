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

        public UnityEngine.UI.Button ClassesMinusButton;

        public UnityEngine.UI.Button ClassesPlusButton;

        public Text ResearchCounter;

        public UnityEngine.UI.Button ResearchMinusButton;

        public UnityEngine.UI.Button ResearchPlusButton;

        public void Initialize(FacultyWindow parent, HiredFaculty faculty, Sprite headshot)
        {
            _parent = parent;
            _faculty = faculty;
            Headshot.sprite = headshot;
            NameText.text = faculty.Name;
            UpdateStatsText();
            UpdateClassesCounter();
            UpdateResearchCounter();

            ClassesMinusButton.onClick.AddListener(() =>
            {
                _faculty.RemoveTeaching();
                UpdateClassesCounter();
            });

            ClassesPlusButton.onClick.AddListener(() =>
            {
                _faculty.AddTeaching();
                UpdateClassesCounter();
            });

            ResearchMinusButton.onClick.AddListener(() =>
            {
                _faculty.RemoveResearch();
                UpdateResearchCounter();
            });

            ResearchPlusButton.onClick.AddListener(() =>
            {
                _faculty.AddResearch();
                UpdateResearchCounter();
            });
        }

        private void UpdateStatsText()
        {
            StatsText.text =
                $"Salary: ${_faculty.SalaryPerYear:n0} /yr" +
                $"Teaching: {_faculty.TeachingScore}" +
                $"Research: {_faculty.ResearchScore}";
        }

        private void UpdateClassesCounter()
        {
            ClassesCounter.text = _faculty.TeachingSlots.ToString();
        }

        private void UpdateResearchCounter()
        {
            ResearchCounter.text = _faculty.ResearchSlots.ToString();
        }
    }
}
