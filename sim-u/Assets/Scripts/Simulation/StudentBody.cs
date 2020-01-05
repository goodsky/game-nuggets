using Common;
using GameData;
using System;
using System.Linq;

namespace Simulation
{
    public enum StudentBodyYear
    {
        Freshmen = 0,
        Sophmores = 1,
        Juniors = 2,
        Seniors = 3,
        Seniors1 = 4,
        Seniors2 = 5,

        // Be careful changing this enum.
        // It could break save games.
        MaxYearsToGraduate
    }

    /// <summary>
    /// Data structure that represents the entire student body.
    /// The StudentBody is built up of 'Classes' (which is a 'batch' of students that enrolled at the same year).
    /// </summary>
    public class StudentBody : IGameStateSaver<StudentBodySaveState>
    {
        public const int AcademicScoreMin = 60;
        public const int AcademicScoreRange = 51;

        private readonly SimulationData _config;
        private readonly StudentHistogramGenerator _generator;

        /// <summary>
        /// The academic score of students bucketed by the class.
        /// </summary>
        private StudentHistogram[] _academicScores = new StudentHistogram[(int)StudentBodyYear.MaxYearsToGraduate];

        public StudentBody(SimulationData config, StudentHistogramGenerator generator)
        {
            _config = config;
            _generator = generator;

            for (int i = 0; i < (int)StudentBodyYear.MaxYearsToGraduate; ++i)
            {
                _academicScores[i] = generator.GenerateEmpty();
            }
        }

        public int TotalStudentCount => _academicScores.Sum(students => students.TotalStudentCount);

        /// <summary>
        /// Execute the graduation ceremony!
        /// This method must only be called once per academic year
        /// otherwise you will graduate students faster than expected!
        /// </summary>
        /// <returns>The results of the graduation.</returns>
        public GraduationResults GraduateStudents()
        {
            // TODO: calculate this from game state
            double graduationRate = _config.GraduationRate;

            StudentHistogram graduated = _generator.GenerateEmpty();
            StudentHistogram dropped = _generator.GenerateEmpty();

            for (int i = (int)StudentBodyYear.Seniors; i < (int)StudentBodyYear.MaxYearsToGraduate; ++i)
            {
                StudentHistogram students = _academicScores[i];
                int graduationCount = (int)Math.Ceiling(graduationRate * students.TotalStudentCount);

                StudentHistogram grads = students.TakeTop(graduationCount);
                StudentHistogram notGrads = students.TakeBottom(students.TotalStudentCount - graduationCount);

                GameLogger.Info("Graduated {0} students from class {1}. Remaining students {2}",
                    grads.TotalStudentCount,
                    ((StudentBodyYear)i).ToString(),
                    notGrads.TotalStudentCount);

                graduated = graduated.Merge(grads);
                if (i == (int)StudentBodyYear.MaxYearsToGraduate - 1)
                {
                    // failed the last chance.
                    dropped = dropped.Merge(notGrads);
                    _academicScores[i] = _generator.GenerateEmpty();
                }
                else
                {
                    _academicScores[i] = notGrads;
                }
            }

            return new GraduationResults
            {
                GraduatedStudents = graduated,
                FailedStudents = dropped,
            };
        }

        /// <summary>
        /// Enroll a new class of students.
        /// </summary>
        /// <param name="academicScores">The histogram of student academic scores.</param>
        public void EnrollClass(StudentHistogram academicScores)
        {
            int maxStudentBodyIndex = (int)StudentBodyYear.MaxYearsToGraduate - 1;
            if (_academicScores[maxStudentBodyIndex].TotalStudentCount != 0)
            {
                GameLogger.Error("Attempting to enroll class before graduation has been completed! {0} Students dropped.",
                    _academicScores[maxStudentBodyIndex].TotalStudentCount);
            }

            for (int i = maxStudentBodyIndex; i > 0; --i)
            {
                _academicScores[i] = _academicScores[i - 1];
            }

            _academicScores[(int)StudentBodyYear.Freshmen] = academicScores;
        }

        /// <summary>
        /// Gets the academic score of a certain class.
        /// </summary>
        /// <returns></returns>
        public StudentHistogram GetClassAcademicScores(StudentBodyYear year)
        {
            return _academicScores[(int)year];
        }

        public void LoadGameState(StudentBodySaveState state)
        {
            if (state?.AcademicScoreHistograms != null)
            {
                _academicScores = state.AcademicScoreHistograms;
            }
        }

        public StudentBodySaveState SaveGameState()
        {
            return new StudentBodySaveState
            {
                AcademicScoreHistograms = _academicScores,
            };
        }
    }
}
