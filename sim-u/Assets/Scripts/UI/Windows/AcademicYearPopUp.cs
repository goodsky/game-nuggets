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
        private StudentHistogram _enrollingPopulation;

        public Text TitleText;

        public SliderFilter TuitionSlider;
        public SliderFilter SatSlider;
        public SliderFilter EnrollmentSlider;

        public Button ApproveButton;

        public override List<Button> Buttons => new List<Button> { ApproveButton };

        public override void Open(object data)
        {
            Freeze();

            TitleText.text = "Academic Year " + Accessor.Simulation.Date.Year;

            TuitionSlider.OnValueChanged = _ => SetPendingRecalculation(recalculateSAT: true, recalculateEnrollment: true);
            SatSlider.OnValueChanged = _ => SetPendingRecalculation(recalculateEnrollment: true);
            EnrollmentSlider.OnValueChanged = _ => SetPendingRecalculation();

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
                EnrollmentSlider.Recalculating();
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
            EnrollmentSlider.SetRange(0, _enrollingPopulation.TotalStudentCount);
            EnrollmentSlider.SetValue(EnrollmentSlider.Value, updateSlider: false);

            int studentsToTake = EnrollmentSlider.Value;
            _enrollingPopulation = _enrollingPopulation.Take(studentsToTake);
        }

        private void ConfirmEnrollment()
        {
            GameLogger.Info("Confirmed enrollment. Tuition = ${0:n0}; SAT Filter = {1}; Enrollment Size = {2}; Class = {3}",
                TuitionSlider.Value,
                SatSlider.Value,
                EnrollmentSlider.Value,
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
