using Campus;
using Common;
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

        [Header("Status Bar Texts")]
        public RectTransform RootStatusLayout;
        public Text MoneyText;
        public Text StudentCountText;
        public Text PopularityText;
        public Text AcademicsText;
        public Text ResearchText;
        public Text ScoreText;
        public Text DateText;

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
            string moneyStr = string.Format(CultureInfo.CurrentCulture, "{0:C0}", _simulation.Score.Money);
            anyUpdate |= UpdateTextCheckIfChanged(MoneyText, moneyStr);

            // Update Student Count Status
            StudentBody currentStudents = _simulation.CurrentStudentBody();
            string studentCountStr = $"{currentStudents.TotalStudentCount} / {_campus.TotalConnectedClassroomCount}";
            anyUpdate |= UpdateTextCheckIfChanged(StudentCountText, studentCountStr);

            // Update Popularity
            string popularityStr = $"{_simulation.Score.Popularity}";
            anyUpdate |= UpdateTextCheckIfChanged(PopularityText, popularityStr);

            // Update Academic Prestige
            string academicStr = $"{_simulation.Score.AcademicPrestige}";
            anyUpdate |= UpdateTextCheckIfChanged(AcademicsText, academicStr);

            // Update Research Prestige
            string researchStr = $"{_simulation.Score.ResearchPrestige}";
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
            const int NestedLayoutGroupCount = 4;
            for (int i = 0; i < NestedLayoutGroupCount; ++i)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}
