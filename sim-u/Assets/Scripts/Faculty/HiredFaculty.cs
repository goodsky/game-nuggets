using Common;
using Simulation;
using System;
using System.Linq;

namespace Faculty
{
    /// <summary>
    /// Data object that represents a hired faculty member and their stats.
    /// </summary>
    [Serializable]
    public class HiredFaculty
    {
        private static readonly Random _rng = new Random();

        private int _weeksSinceTaught;
        private StudentHistogram[] _students;

        public HiredFaculty(int id, string name, int salary, int teachingScore, int researchScore, int maximumSlots)
        {
            Id = id;
            Name = name;
            SalaryPerYear = salary;
            TeachingScore = teachingScore;
            ResearchScore = researchScore;
            MaximumSlots = maximumSlots;

            // this is a hack to get teachers to teach over a random period of time...
            _weeksSinceTaught = _rng.Next(20);
        }

        public int Id { get; }

        public string Name { get; }

        public int SalaryPerYear { get; set; }

        public int TeachingScore { get; set; }

        public int ResearchScore { get; set; }

        public int MaximumSlots { get; set; }

        public int TeachingSlots { get; set; }

        public int ResearchSlots { get; set; }

        public int TeachingOutput
        {
            get
            {
                return TeachingSlots * TeachingScore;
            }
        }

        public int ResearchOutput
        {
            get
            {
                return ResearchSlots * ResearchScore;
            }
        }

        public int UsedSlots
        {
            get
            {
                return TeachingSlots + ResearchSlots;
            }
        }

        /// <summary>
        /// Assign a new set of students to be taught.
        /// </summary>
        /// <param name="students"></param>
        public void AssignStudents(StudentHistogram[] students)
        {
            _students = students;
        }

        /// <summary>
        /// Update the local copy of your students.
        /// Then return the delta needed to make the same change to the student body.
        /// </summary>
        public StudentHistogram[] Teach()
        {
            if (_students == null)
            {
                return null;
            }

            if (++_weeksSinceTaught >= (TeachingPeriod()))
            {
                _weeksSinceTaught = 0;

                StudentHistogram[] delta = new StudentHistogram[_students.Length];
                for (int i = 0; i < delta.Length; ++i)
                {
                    // First erase all old student histogram
                    delta[i] = _students[i].GetInverse();

                    // Then add the new student histogram
                    int modifier = TeachingModifier();
                    _students[i].AddToValues(modifier);
                    delta[i].Add(_students[i]);
                }

                GameLogger.Debug("[Teach] Faculty {0} has added {1} to their {2:n0} students.", Name, TeachingModifier(), _students.Sum(s => s.TotalStudentCount));
                return delta;
            }

            return null;
        }

        /// <summary>
        /// This is mapped to the following table:
        /// Teaching Score  | Expected Change (4 yrs)   | Teach every N weeks
        ///  100    | +20       | 10
        ///  90     | +10       | 21
        ///  80     | +5        | 42
        ///  70     | +0        | NA
        ///  60     | -5        | 42
        ///  50     | -10       | 21
        /// 
        /// https://www.wolframalpha.com/widgets/view.jsp?id=a96a9e81ac4bbb54f8002bb61b8d3472
        /// 
        /// </summary>
        private int TeachingPeriod()
        {
            int calculatedPeriod = (int)Math.Round(
                (-0.0446753 * TeachingScore * TeachingScore) +
                (6.35974 * TeachingScore) +
                -182.727);

            // Make sure it doesn't get any more aggressive than once every 10 weeks.
            return Utils.Clamp(calculatedPeriod, 10, 52);
        }

        /// <summary>
        /// Add 1 to academic scores if you are above the threshold.
        /// Subtract 1 to academic scores if you are below the threshold.
        /// </summary>
        /// <returns></returns>
        private int TeachingModifier()
        {
            return TeachingScore >= 70 ? 1 : -1;
        }
    }
}
