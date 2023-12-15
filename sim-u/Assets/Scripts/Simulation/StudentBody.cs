using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation
{
    public enum StudentBodyYear
    {
        Freshman = 0,
        Sophmore = 1,
        Junior = 2,
        Senior = 3,
        Senior1 = 4,
        Senior2 = 5,

        // Be careful changing this enum.
        // It WILL break save games.
        MaxYearsToGraduate
    }

    /// <summary>
    /// Data structure that represents the entire student body.
    /// The StudentBody is built up of 'Classes' (which is a 'batch' of students that enrolled at the same year).
    /// </summary>
    public class StudentBody : IGameStateSaver<StudentBodySaveState>
    {
        private readonly SimulationData _config;
        private readonly StudentHistogramGenerator _generator;

        /// <summary>
        /// The academic score of students bucketed by the class.
        /// </summary>
        private StudentHistogram[] _activeStudents = new StudentHistogram[(int)StudentBodyYear.MaxYearsToGraduate];

        /// <summary>
        /// The academic score of every student we have graduated.
        /// </summary>
        private List<GraduationResults> _graduatedStudents = new List<GraduationResults>();

        public StudentBody(SimulationData config, StudentHistogramGenerator generator)
        {
            _config = config;
            _generator = generator;

            for (int i = 0; i < (int)StudentBodyYear.MaxYearsToGraduate; ++i)
            {
                _activeStudents[i] = generator.GenerateEmpty();
            }
        }

        public int TotalStudentCount => _activeStudents.Sum(students => students.TotalStudentCount);

        /// <summary>
        /// Apply a delta to all students.
        /// Done regularly to update student academics.
        /// </summary>
        /// <param name="delta">The deltas to apply to all students.</param>
        public void UpdateStudents(StudentHistogram[] delta)
        {
            if (delta.Length != (int)StudentBodyYear.MaxYearsToGraduate)
            {
                GameLogger.FatalError("[UpdateStudents] Unexpected class size delta. Delta length = {0}", delta.Length);
            }

            for (int i = 0; i < _activeStudents.Length; ++i)
            {
                if (delta[i].HasValues)
                {
                    GameLogger.Debug("[UpdateStudents] Class: {0}; Delta: {1}", ((StudentBodyYear)i).ToString(), delta[i]);

                    _activeStudents[i].Add(delta[i]);
                }
            }
        }

        /// <summary>
        /// Execute the graduation ceremony!
        /// This method must only be called once per academic year
        /// otherwise you will graduate students faster than expected!
        /// </summary>
        /// <returns>The results of the graduation.</returns>
        public GraduationResults GraduateStudents(SimulationDate date)
        {
            Dictionary<StudentBodyYear, List<GraduationRateBucket>> graduationRates =
                _config.GraduationRateBuckets
                    .GroupBy(bucket => bucket.Year)
                    .ToDictionary(g => g.Key, g => g.ToList());

            StudentHistogram graduated = _generator.GenerateEmpty();
            StudentHistogram droppedOut = _generator.GenerateEmpty();

            for (int i = (int)StudentBodyYear.Freshman; i < (int)StudentBodyYear.MaxYearsToGraduate; ++i)
            {
                StudentHistogram students = _activeStudents[i];

                (StudentHistogram grads, StudentHistogram dropouts) =
                    CalculateGraduationStep(students, (StudentBodyYear)i);

                students.Subtract(grads);
                students.Subtract(dropouts);

                GameLogger.Info("Graduation Class [{0}]: Graduated {1}; DropOuts: {2}; Remaining: {3};",
                    ((StudentBodyYear)i).ToString(),
                    grads,
                    dropouts,
                    students);

                graduated.Add(grads);
                droppedOut.Add(dropouts);

                if (i == (int)StudentBodyYear.MaxYearsToGraduate - 1)
                {
                    // failed the last chance.
                    droppedOut.Add(students);
                    _activeStudents[i] = _generator.GenerateEmpty();
                }
                else
                {
                    _activeStudents[i] = students;
                }
            }

            var result = new GraduationResults
            {
                GraduationDate = date,
                GraduatedStudents = graduated,
                DropOuts = droppedOut,
            };

            _graduatedStudents.Add(result);
            return result;
        }

        /// <summary>
        /// Enroll a new class of students.
        /// </summary>
        /// <param name="academicScores">The histogram of student academic scores.</param>
        public void EnrollClass(StudentHistogram academicScores)
        {
            int maxStudentBodyIndex = (int)StudentBodyYear.MaxYearsToGraduate - 1;
            if (_activeStudents[maxStudentBodyIndex].TotalStudentCount != 0)
            {
                GameLogger.Error("Attempting to enroll class before graduation has been completed! {0} Students dropped.",
                    _activeStudents[maxStudentBodyIndex].TotalStudentCount);
            }

            for (int i = maxStudentBodyIndex; i > 0; --i)
            {
                _activeStudents[i] = _activeStudents[i - 1];
            }

            _activeStudents[(int)StudentBodyYear.Freshman] = academicScores;
        }

        /// <summary>
        /// Gets the academic score of a certain class.
        /// </summary>
        public StudentHistogram GetClassAcademicScores(StudentBodyYear year)
        {
            return _activeStudents[(int)year];
        }

        /// <summary>
        /// This calculation takes into account the historic graduation results in addition
        /// to some of the current student body to estimate the current "academic prestige".
        /// </summary>
        /// <param name="minLookbackYears">Minimum number of years to look back for results</param>
        /// <param name="minHistoricStudents">Minimum number of students to look back</param>
        /// <param name="defaultAcademicScore">Default academic score for students.</param>
        /// <param name="dropOutAcademicScore">Academic score to use for drop outs.</param>
        public (int currentAcademicPrestige, ScoreTrend trend) GetCurrentAcademicPrestige(
            int minLookbackYears,
            int minHistoricStudents,
            int dropOutAcademicScore)
        {
            long activeSum = 0;
            long activeCount = 0;
            for (int i = (int)StudentBodyYear.Senior; i < (int)StudentBodyYear.MaxYearsToGraduate; ++i)
            {
                activeSum += _activeStudents[i].GetTotalSum();
                activeCount += _activeStudents[i].TotalStudentCount; 
            }

            long historicSum = 0;
            long historicCount = 0;
            for (int i = 1; i <= _graduatedStudents.Count; ++i)
            {
                GraduationResults historicResults = _graduatedStudents[_graduatedStudents.Count - i];

                historicSum += historicResults.GraduatedStudents.GetTotalSum();
                historicCount += historicResults.GraduatedStudents.TotalStudentCount;

                // Drop outs have their own value to add to the academic score.
                // Usually this is the minimum value or a little lower
                int dropOutCount = historicResults.DropOuts.TotalStudentCount;
                historicSum += dropOutCount * dropOutAcademicScore;
                historicCount += dropOutCount;

                if (historicCount > minHistoricStudents &&
                    i > minLookbackYears)
                {
                    break;
                }
            }

            int defaultAcademicScore = (int)Math.Round(
                SimulationUtils.LinearMapping(
                    value: _config.AcademicPrestige.DefaultValue,
                    minInput: _config.AcademicPrestige.MinValue,
                    maxInput: _config.AcademicPrestige.MaxValue,
                    minOutput: _config.StudentAcademicScore.MinValue,
                    maxOutput: _config.StudentAcademicScore.MaxValue));

            while (historicCount < minHistoricStudents)
            {
                // Fill in the backlog with default values
                historicSum += defaultAcademicScore;
                ++historicCount;
            }

            // Current implementation: mean value of all active and historic students
            double totalMean = ((activeSum + historicSum) / (double)(activeCount + historicCount));

            int academicPrestige = (int)Math.Round(
                SimulationUtils.LinearMapping(
                    value: totalMean,
                    minInput: _config.StudentAcademicScore.MinValue,
                    maxInput: _config.StudentAcademicScore.MaxValue,
                    minOutput: _config.AcademicPrestige.MinValue,
                    maxOutput: _config.AcademicPrestige.MaxValue));

            academicPrestige = Utils.Clamp(
                academicPrestige,
                _config.AcademicPrestige.MinValue,
                _config.AcademicPrestige.MaxValue);

            // Estimate the trend based on how the active student body looks
            double activeMean = activeCount == 0 ? defaultAcademicScore : activeSum / (double)activeCount;
            double historicMean = historicCount == 0 ? defaultAcademicScore : historicSum / (double)historicCount;

            // Project the likely outcome of the students who are close to graduating
            // Simulate the remaining years of every student.
            long projectedSum = 0;
            long projectedCount = 0;
            for (int i = (int)StudentBodyYear.Freshman; i < (int)StudentBodyYear.MaxYearsToGraduate; ++i)
            {
                StudentHistogram students = _activeStudents[i].Clone();

                for (int j = i; j < (int)StudentBodyYear.MaxYearsToGraduate; ++j)
                {
                    (StudentHistogram hypotheticalGrads, StudentHistogram hypotheticalDropouts) =
                        CalculateGraduationStep(students, (StudentBodyYear)j);

                    projectedSum += hypotheticalGrads.GetTotalSum();
                    projectedSum += hypotheticalDropouts.TotalStudentCount * dropOutAcademicScore;

                    projectedCount += hypotheticalGrads.TotalStudentCount + hypotheticalDropouts.TotalStudentCount;

                    students.Subtract(hypotheticalGrads);
                    students.Subtract(hypotheticalDropouts);
                }
            }

            double projectedMean = projectedCount == 0 ? historicMean : projectedSum / (double)projectedCount;

            ScoreTrend trend = ScoreTrend.Neutral;
            double trendDelta = projectedMean - historicMean;
            if (trendDelta >  1.0)
            {
                trend = ScoreTrend.Up;
            }
            else if (trendDelta < -1.0)
            {
                trend = ScoreTrend.Down;
            }

            GameLogger.Debug("Calculated Academic Prestige = {0}; Total Mean: {1:0.00}; Active Mean: {2:0.00}; Active Count: {3:n0}; Historic Mean: {4:0.00}; Historic Count: {5:n0}; Projected Mean: {6:0.00}",
                academicPrestige,
                totalMean,
                activeMean,
                activeCount,
                historicMean,
                historicCount,
                projectedMean);

            return (academicPrestige, trend);
        }

        /// <summary>
        /// Calculate what should happen to the student body.
        /// </summary>
        /// <param name="students">The student body to calculate graduation step.</param>
        /// <param name="year">The year of this student body.</param>
        /// <returns>The graduates and dropouts from the student body.</returns>
        private (StudentHistogram grads, StudentHistogram dropouts) CalculateGraduationStep(StudentHistogram students, StudentBodyYear year)
        {
            Dictionary<StudentBodyYear, List<GraduationRateBucket>> graduationRates =
                _config.GraduationRateBuckets
                    .GroupBy(bucket => bucket.Year)
                    .ToDictionary(g => g.Key, g => g.ToList());

                StudentHistogram grads = _generator.GenerateEmpty();
                StudentHistogram dropouts = _generator.GenerateEmpty();

            foreach (GraduationRateBucket rateBucket in graduationRates[year])
            {
                StudentHistogram studentBucket = students.Slice(rateBucket.LowerBoundAcademicScore, rateBucket.UpperBoundAcademicScore);

                int graduatingCount = (int)Math.Round(rateBucket.GraduationRate * studentBucket.TotalStudentCount);
                int dropoutCount = (int)Math.Round(rateBucket.DropoutRate * studentBucket.TotalStudentCount);

                if (graduatingCount + dropoutCount > studentBucket.TotalStudentCount)
                {
                    GameLogger.Error("More students are graduating or dropping out than possible! Bucket: {0} - ({1}, {2}] - Grad: {3:0.00}% - Drop: {4:0.00}%; Students: {5}; Graduating: {6}; DropOuts: {7};",
                        rateBucket.Year,
                        rateBucket.LowerBoundAcademicScore,
                        rateBucket.UpperBoundAcademicScore,
                        rateBucket.GraduationRate,
                        rateBucket.DropoutRate,
                        studentBucket.TotalStudentCount,
                        graduatingCount,
                        dropoutCount);

                    // NB: I'm erring on the side of allowing more graduations
                    graduatingCount = Math.Min(graduatingCount, studentBucket.TotalStudentCount);
                    dropoutCount = Math.Min(dropoutCount, studentBucket.TotalStudentCount - graduatingCount);
                }

                grads.Add(studentBucket.TakeTop(graduatingCount));
                dropouts.Add(studentBucket.TakeBottom(dropoutCount));
            }

            return (grads, dropouts);
        }

        public void LoadGameState(StudentBodySaveState state)
        {
            if (state?.ActiveStudents != null)
            {
                _activeStudents = state.ActiveStudents;
                _graduatedStudents = state.GraduatedStudents ?? new List<GraduationResults>();
            }
        }

        public StudentBodySaveState SaveGameState()
        {
            return new StudentBodySaveState
            {
                ActiveStudents = _activeStudents,
                GraduatedStudents = _graduatedStudents,
            };
        }
    }
}
