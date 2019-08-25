using GameData;

namespace Simulation
{
    /// <summary>
    /// Data structure that represents the entire student body.
    /// The StudentBody is built up of 'Classes' (which is a 'batch' of students that enrolled at the same year).
    /// </summary>
    public class StudentBody : IGameStateSaver<StudentBodySaveState>
    {
        public const int MaximumYearsToGraduate = 6;

        public const int AcademicScoreMin = 60;
        public const int AcademicScoreRange = 51;

        /// <summary>
        /// Index to which student class is the latest freshmen class.
        /// (FreshmenIndex + 1) % MaximumYearsToGraduate = Sophomore class, etc.
        /// </summary>
        private int _freshmenIndex = MaximumYearsToGraduate - 1;

        /// <summary>
        /// The academic score of students bucketed by the class.
        /// </summary>
        private StudentHistogram[] _academicScores = new StudentHistogram[MaximumYearsToGraduate];

        public StudentBody(StudentHistogramGenerator generator)
        {
            for (int i = 0; i < MaximumYearsToGraduate; ++i)
            {
                _academicScores[i] = generator.GenerateEmpty();
            }
        }

        /// <summary>
        /// Enroll a new class of students.
        /// </summary>
        /// <param name="academicScores">The histogram of student academic scores.</param>
        public void EnrollClass(StudentHistogram academicScores)
        {
            int newFreshmenIndex = _freshmenIndex - 1;
            if (newFreshmenIndex < 0)
            {
                newFreshmenIndex = MaximumYearsToGraduate - 1;
            }

            _academicScores[newFreshmenIndex] = academicScores;
            _freshmenIndex = newFreshmenIndex;
        }

        /// <summary>
        /// Gets the 
        /// </summary>
        /// <param name="yearsFromFreshman"></param>
        /// <returns></returns>
        public StudentHistogram GetClassAcademicScores(int yearsFromFreshman)
        {
            return _academicScores[(_freshmenIndex + yearsFromFreshman) % MaximumYearsToGraduate];
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
