using Common;
using Simulation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AcademicYearPopUp : Window
    {
        private bool _pendingRecalculation = false;
        private long _nextRecalulationTimeInTicks;
        private GraduationResults _graduatingPopulation;
        private StudentHistogram _enrollingPopulation;

        public Text TitleText;

        public SliderFilter TuitionSlider;
        public SliderFilter SatSlider;

        public Text SummaryText;
        public Button ApproveButton;

        public override List<Button> Buttons => new List<Button> { ApproveButton };

        public override void Open(object data)
        {
            Freeze();

            TitleText.text = "Academic Year " + Accessor.Simulation.Date.Year;

            _graduatingPopulation = data as GraduationResults;
            if (_graduatingPopulation == null)
            {
                // BUGBUG: This can be null when this pop up interupts another window.
                GameLogger.Error("Unexpected graduation result! Type = {0}", data?.GetType());
            }

            TuitionSlider.OnValueChanged = _ => SetPendingRecalculation(recalculateSAT: true, recalculateEnrollment: true);
            SatSlider.OnValueChanged = _ => SetPendingRecalculation(recalculateEnrollment: true);

            ApproveButton.OnSelect = ConfirmEnrollment;

            RecalculateStudentPopulation();
        }

        public override void Close()
        {
            Unfreeze();
        }

        /// <summary>
        /// Unity's update method.
        /// </summary>
        protected override void Update()
        {
            if (_pendingRecalculation &&
                DateTime.Now.Ticks > _nextRecalulationTimeInTicks)
            {
                _pendingRecalculation = false;
                RecalculateStudentPopulation();
            }

            base.Update();
        }

        private void SetPendingRecalculation(bool recalculateSAT = false, bool recalculateEnrollment = false)
        {
            _nextRecalulationTimeInTicks = DateTime.Now.Ticks + TimeSpan.FromMilliseconds(500).Ticks;
            _pendingRecalculation = true;

            if (recalculateSAT)
            {
                SatSlider.Recalculating();
            }

            if (recalculateEnrollment)
            {
                UpdateSummary(recalculating: true);
            }
        }

        private void RecalculateStudentPopulation()
        {
            (int minTuition, int maxTuition) = Accessor.Simulation.GenerateTuitionRange();
            TuitionSlider.SetRange(minTuition, maxTuition);
            TuitionSlider.SetValue(TuitionSlider.Value, updateSlider: false);

            int tuitionValue = TuitionSlider.Value;
            _enrollingPopulation = Accessor.Simulation.GenerateStudentPopulation(tuitionValue);

            (int minAcademicScore, int maxAcademicScore) = _enrollingPopulation.GetScoreRange();
            int minSAT = Accessor.Simulation.ConvertAcademicScoreToSATScore(minAcademicScore);
            int maxSAT = Accessor.Simulation.ConvertAcademicScoreToSATScore(maxAcademicScore);
            SatSlider.SetRange(minSAT, maxSAT);
            SatSlider.SetValue(SatSlider.Value, updateSlider: false);

            int satCutoff = SatSlider.Value;
            int academicScoreCutoff = Accessor.Simulation.ConvertSATScoreToAcademicScore(satCutoff);
            _enrollingPopulation = _enrollingPopulation.Split(academicScoreCutoff);

            UpdateSummary();
        }

        private void UpdateSummary(bool recalculating = false)
        {
            const string SummaryTextFormat =
@"Graduating Student Count: {0:n0}
Enrolling Student Count: {1:n0}
New Total Student Count: {2:n0}
Total Classroom Capacity: {3:n0}";

            if (recalculating)
            {
                SummaryText.text = string.Format(SummaryTextFormat,
                    _graduatingPopulation.GraduatedStudents.TotalStudentCount,
                    "...",
                    "...",
                    Accessor.CampusManager.TotalConnectedClassroomCount);
            }
            else
            {
                SummaryText.text = string.Format(SummaryTextFormat,
                    _graduatingPopulation.GraduatedStudents.TotalStudentCount,
                    _enrollingPopulation.TotalStudentCount,
                    Accessor.Simulation.CurrentStudentBody().TotalStudentCount + _enrollingPopulation.TotalStudentCount,
                    Accessor.CampusManager.TotalConnectedClassroomCount);
            }
        }

        private void ConfirmEnrollment()
        {
            GameLogger.Info("Confirmed enrollment. Tuition = ${0:n0}; SAT Filter = {1}; Enrollment Size = {2}; Class = {3}",
                TuitionSlider.Value,
                SatSlider.Value,
                _enrollingPopulation.TotalStudentCount,
                _enrollingPopulation.ToString());

            Accessor.Simulation.EnrollStudents(_enrollingPopulation);

            // NB: Hack. Re-opening a window will close it...
            Accessor.UiManager.OpenWindow(this.name, null);
        }

        private void Freeze()
        {
            Accessor.Simulation.SetSimulationFreeze(freeze: true);
            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            camera.FreezeCamera();
            SelectionManager.FreezeSelection(ApproveButton);
        }

        private void Unfreeze()
        {
            Accessor.Simulation.SetSimulationFreeze(freeze: false);
            var camera = Camera.main.GetComponent<OrthoPanningCamera>();
            camera.UnfreezeCamera();
            SelectionManager.UnfreezeSelection();
        }
    }
}
