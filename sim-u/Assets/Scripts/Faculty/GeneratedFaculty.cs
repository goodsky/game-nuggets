using System;

namespace Faculty
{
    /// <summary>
    /// Data object that represents a faculty member that you can hire with their stats.
    /// </summary>
    [Serializable]
    public class GeneratedFaculty
    {
        public GeneratedFaculty(int id, string name, int salary, int teachingScore, int researchScore, int maximumSlots)
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

        public HiredFaculty Hire()
        {
            return new HiredFaculty(
                Id,
                Name,
                SalaryPerYear,
                TeachingScore,
                ResearchScore,
                MaximumSlots);
        }

        public override string ToString()
        {
            return $"([{Id}] {Name}: Salary={SalaryPerYear}; Teaching={TeachingScore}; Research={ResearchScore}; Slots={MaximumSlots};)";
        }
    }
}
