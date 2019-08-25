using System;

namespace Simulation
{
    public enum SimulationQuarter
    {
        Fall   = 0,
        Winter = 1,
        Spring = 2,
        Summer = 3
    }

    /// <summary>
    /// Structure that represents the Year/Quarter/Week instant of time in the game simulation.
    /// Aggressively checks for valid bounds. Has logic to increment time to the next week.
    /// </summary>
    [Serializable]
    public struct SimulationDate
    {
        public const int WeeksPerQuarter = 13;

        public SimulationDate(int year, SimulationQuarter quarter, int week)
        {
            if (year < 1)
                throw new ArgumentException($"Invalid year. Value = {year}");

            if (week < 1 || week > WeeksPerQuarter)
                throw new ArgumentException($"Invalid week. Value = {week}");

            Year = year;
            Quarter = quarter;
            Week = week;
        }

        public int Year { get; private set; }

        public SimulationQuarter Quarter { get; private set; }

        public int Week { get; private set; }

        public SimulationDate NextWeek()
        {
            int nextYear = Year;
            SimulationQuarter nextQuarter = Quarter;
            int nextWeek = Week + 1;

            if (nextWeek > WeeksPerQuarter)
            {
                nextWeek = 1;
                int nextQuarterInt = (int)nextQuarter + 1;
                if (nextQuarterInt >= 4)
                {
                    nextQuarterInt = 0;
                    nextYear += 1;
                }

                nextQuarter = (SimulationQuarter)nextQuarterInt;
            }

            return new SimulationDate(nextYear, nextQuarter, nextWeek);
        }
    }
}
