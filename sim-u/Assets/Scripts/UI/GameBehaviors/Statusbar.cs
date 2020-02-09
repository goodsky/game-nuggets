using Campus;
using Common;
using Faculty;
using Simulation;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Behaviour for the status bar at the top of the screen.
    /// </summary>
    public class Statusbar : MonoBehaviour
    {
        private readonly GameAccessor _accessor = new GameAccessor();
        private SimulationManager _simulation;
        private CampusManager _campus;
        private FacultyManager _faculty;

        [Header("Status Bar Texts")]
        public RectTransform RootStatusLayout;
        public Text MoneyText;
        public Text StudentCountText;
        public Text PopularityText;
        public Text AcademicsText;
        public Text ResearchText;
        public Text ScoreText;
        public Text DateText;

        [Header("Selectables to update tool tip")]
        public Common.Selectable StudentCount;

        [Space(5)]
        [Header("Pause Button")]
        public Button PauseButton;
        public Image PauseActiveImage;

        [Header("Play Normal Button")]
        public Button PlayNormalButton;
        public Image PlayNormalActiveImage;

        [Header("Play Fast Button")]
        public Button PlayFastButton;
        public Image PlayFastActiveImage;

        public Button[] Buttons => new[] { PauseButton, PlayNormalButton, PlayFastButton };

        /// <summary>
        /// The Unity start method
        /// </summary>
        protected void Start()
        {
            _simulation = _accessor.Simulation;
            _campus = _accessor.CampusManager;
            _faculty = _accessor.Faculty;

            PauseButton.OnMouseDown = e =>
            {
                _simulation.SetSimulationSpeed(SimulationSpeed.Paused);
            };

            PlayNormalButton.OnMouseDown = e =>
            {
                _simulation.SetSimulationSpeed(SimulationSpeed.Normal);
            };

            PlayFastButton.OnMouseDown = e =>
            {
                _simulation.SetSimulationSpeed(SimulationSpeed.Fast);
            };

            FixStatusLayoutHack(RootStatusLayout);
        }

        /// <summary>
        /// This method is invoked whenever it is time to update the display with new simulation data.
        /// I could use Unit's Update directly... but it would involve pointless updates. So I did this. Is that okay?
        /// </summary>
        public void SimulationUpdateCallback()
        {
            bool anyUpdate = false;

            // Update Money Status
            string moneyColor = _simulation.Score.Money >= 0 ? "green" : "red";
            string moneyStr = string.Format(CultureInfo.CurrentCulture, "<color={1}>{0:C0}</color>", _simulation.Score.Money, moneyColor);
            anyUpdate |= UpdateTextCheckIfChanged(MoneyText, moneyStr);

            // Update Student Count Status
            StudentBody currentStudents = _simulation.CurrentStudentBody();
            string studentCountColor = currentStudents.TotalStudentCount <= _faculty.StaffedClassroomCapacity ? "white" : "red";
            string studentCountStr = $"<color={studentCountColor}>{currentStudents.TotalStudentCount} / {_faculty.StaffedClassroomCapacity}</color>";
            anyUpdate |= UpdateTextCheckIfChanged(StudentCountText, studentCountStr);

            StudentCount.Tooltip =
$@"Student Status:
    - {currentStudents.TotalStudentCount:n0} Students
    - {_campus.TotalConnectedClassroomCount:n0} Available Classrooms
    - {_faculty.UsedClassroomCount:n0} Assigned Faculty";

            // Update Popularity
            string popularityColor = _simulation.Score.Popularity >= 0 ? "white" : "red";
            string popularityStr = $"<color={popularityColor}>{_simulation.Score.Popularity}</color>";
            anyUpdate |= UpdateTextCheckIfChanged(PopularityText, popularityStr);

            // Update Academic Prestige
            string academicColor = _simulation.Score.AcademicPrestige >= 0 ? "white" : "red";
            string academicStr = $"<color={academicColor}>{_simulation.Score.AcademicPrestige}</color>";
            anyUpdate |= UpdateTextCheckIfChanged(AcademicsText, academicStr);

            // Update Research Prestige
            string researchColor = _simulation.Score.ResearchPrestige >= 0 ? "white" : "red";
            string researchStr = $"<color={researchColor}>{_simulation.Score.ResearchPrestige}</color>";
            anyUpdate |= UpdateTextCheckIfChanged(ResearchText, researchStr);

            // Update Campus Score
            string scoreStr = $"{_simulation.Score.AcademicScore + _simulation.Score.ResearchScore}";
            anyUpdate |= UpdateTextCheckIfChanged(ScoreText, scoreStr);

            // Update Current Date
            SimulationDate currentDate = _simulation.Date;
            string weekString = currentDate.Week == SimulationDate.WeeksPerQuarter ? "Finals" : $"Week {currentDate.Week}";
            DateText.text = $"Year {currentDate.Year} / {currentDate.Quarter.ToString()} / {weekString}";

            // Update Simulation Speed Buttons
            PauseActiveImage.enabled = _simulation.Speed == SimulationSpeed.Paused;
            PlayNormalActiveImage.enabled = _simulation.Speed == SimulationSpeed.Normal;
            PlayFastActiveImage.enabled = _simulation.Speed == SimulationSpeed.Fast;

            if (anyUpdate)
            {
                FixStatusLayoutHack(RootStatusLayout);
            }
        }

        /// <summary>
        /// I want to call the fix layout call as few times as possible.
        /// So I'm keeping track of when the values actually change.
        /// </summary>
        private bool UpdateTextCheckIfChanged(Text text, string str)
        {
            if (string.Compare(text.text, str, StringComparison.Ordinal) != 0)
            {
                text.text = str;
                LayoutRebuilder.MarkLayoutForRebuild(text.rectTransform);
                return true;
            }

            return false;
        }

        private void FixStatusLayoutHack(RectTransform transform)
        {
            // Nested Layout Groups don't seem to play nicely.
            // https://forum.unity.com/threads/experiencing-difficulties-with-nested-layout-groups-how-to-display-children-without-overlaping-etc.499920/

            // Why do I call a loop? Because the changes don't percalate down correctly.
            // So I'm calling it once per nested layout group.
            const int NestedLayoutGroupCount = 3;
            for (int i = 0; i < NestedLayoutGroupCount; ++i)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}
