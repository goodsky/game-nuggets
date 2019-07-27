using System;

namespace Faculty
{
    /// <summary>
    /// Data object that represents a hired faculty member and their stats.
    /// </summary>
    [Serializable]
    public class HiredFaculty
    {
        public HiredFaculty(int id, string name, int salary, int teachingScore, int researchScore)
        {
            Id = id;
            Name = name;
            SalaryPerYear = salary;
            TeachingScore = teachingScore;
            ResearchScore = researchScore;
        }

        public int Id { get; }

        public string Name { get; }

        public int SalaryPerYear { get; private set; }

        public int TeachingScore { get; private set; }

        public int ResearchScore { get; private set; }

        public int TeachingSlots { get; private set; }

        public int ResearchSlots { get; private set; }

        public int MaximumSlots { get; private set; }

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

        private int UsedSlots
        {
            get
            {
                return TeachingSlots + ResearchSlots;
            }
        }

        public void AddTeaching()
        {
            if (UsedSlots < MaximumSlots)
            {
                ++TeachingSlots;
            }
        }

        public void RemoveTeaching()
        {
            if (TeachingSlots > 0)
            {
                --TeachingSlots;
            }
        }

        public void AddResearch()
        {
            if (UsedSlots < MaximumSlots)
            {
                ++ResearchSlots;
            }
        }

        public void RemoveResearch()
        {
            if (ResearchSlots > 0)
            {
                --ResearchSlots;
            }
        }
    }
}
