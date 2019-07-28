using System;

namespace Faculty
{
    /// <summary>
    /// Data object that represents a hired faculty member and their stats.
    /// </summary>
    [Serializable]
    public class HiredFaculty
    {
        public HiredFaculty(int id, string name, int salary, int teachingScore, int researchScore, int maximumSlots)
        {
            Id = id;
            Name = name;
            SalaryPerYear = salary;
            TeachingScore = teachingScore;
            ResearchScore = researchScore;
            MaximumSlots = maximumSlots;
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
    }
}
