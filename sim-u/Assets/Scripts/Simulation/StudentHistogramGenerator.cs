using Common;
using GameData;
using System;

namespace Simulation
{
    /// <summary>
    /// The StudentHistogram Generator will create a normally distributed population
    /// organized in a histogram datastructure.
    /// </summary>
    public class StudentHistogramGenerator
    {
        private readonly SimulationData _config;

        public StudentHistogramGenerator(SimulationData config)
        {
            _config = config;
        }

        /// <summary>
        /// Generates an empty histogram with 0 students.
        /// </summary>
        /// <returns>The empty histogram.</returns>
        public StudentHistogram GenerateEmpty()
        {
            int range = _config.StudentAcademicScore.MaxValue - _config.StudentAcademicScore.MinValue + 1;
            return new StudentHistogram(new int[range], _config.StudentAcademicScore.MinValue);
        }

        /// <summary>
        /// Converts a student's academic score to an SAT score.
        /// </summary>
        /// <param name="academicScore">The academic score of a student.</param>
        /// <returns>The SAT score for this student.</returns>
        public int ConvertAcademicScoreToSATScore(int academicScore)
        {
            // Haha this is a gross oversimplification. But I've decided to take the randomness out of this.
            // NOTE: The range of student academic scores goes beyond 1600
            if (academicScore > _config.MaxStudentAcademicScoreDuringEnrollment)
            {
                academicScore = _config.MaxStudentAcademicScoreDuringEnrollment;
            }

            return SimulationUtils.LinearMapping(
                value: academicScore,
                minInput: _config.StudentAcademicScore.MinValue,
                maxInput: _config.MaxStudentAcademicScoreDuringEnrollment,
                minOutput: _config.StudentSATScore.MinValue,
                maxOutput: _config.StudentSATScore.MaxValue);
        }

        /// <summary>
        /// Converts a student's SAT score to an academic score.
        /// </summary>
        /// <param name="academicScore">The SAT score of a student.</param>
        /// <returns>The academic score for this student.</returns>
        public int ConvertSATScoreToAcademicScore(int satScore)
        {
            return SimulationUtils.LinearMapping(
                value: satScore,
                minInput: _config.StudentSATScore.MinValue,
                maxInput: _config.StudentSATScore.MaxValue,
                minOutput: _config.StudentAcademicScore.MinValue,
                maxOutput: _config.MaxStudentAcademicScoreDuringEnrollment);
        }

        /// <summary>
        /// Generates a normally distributed population of students.
        /// Calculates the mean academic score and the size of the population based off of the SimulationScore.
        /// </summary>
        /// <param name="tuition">The proposed tuition value for the academic year. ($/yr)</param>
        /// <param name="currentScore">The current university simulation scores.</param>
        /// <returns>A generated population of students represented as a histogram.</returns>
        public StudentHistogram GenerateStudentPopulation(int tuition, SimulationScore currentScore)
        {
            // 1) Calculate 'Target Tuition' based off of Academic Prestige and Research Prestige
            int targetTuition = CalculateTargetTuition(currentScore);

            // 2) Calculate 'Tuition Bonus' based off of Requested Tuition vs Target Tuition
            int tuitionBonus = CalculateTuitionBonus(tuition, targetTuition);

            // 3) Calculate 'ApplyingClassSize' based off of Popularity and Tuition Bonus
            int populationSize = CalculateEnrollingClassPopulationSize(currentScore, tuitionBonus);

            // 4) Calculate 'Mean SAT Score' based off of Academic Prestige
            int populationMeanAcademicScore = CalculateMeanPopulationAcademicScore(currentScore);

            // Generate Population based off of Mean Academic Score, Variance, and Applying Class Size
            double variance = Math.Pow(_config.EnrollingPopulationAcademicScoreStdDev, 2);
            int[] population = GeneratePopulation(
                populationMeanAcademicScore,
                variance,
                _config.StudentAcademicScore.MinValue,
                _config.StudentAcademicScore.MaxValue,
                populationSize);

            var histogram = new StudentHistogram(population, _config.StudentAcademicScore.MinValue);
            GameLogger.Debug("Generating a student population for tuition ${0:n0} /yr. Variables: targetTuition=${1:n0}; tuitionBonus={2}; populationSize={3}; meanAcademicScore={4}; variance={5}. Result={6}",
                tuition,
                targetTuition,
                tuitionBonus,
                populationSize,
                populationMeanAcademicScore,
                variance,
                histogram.ToString());

            return histogram;
        }

        /// <summary>
        /// Given the state of the university, calculate the 'optimal' tuition for the university.
        /// As judged by prospective students.
        /// </summary>
        /// <param name="score">The current university scores.</param>
        /// <param name="bonus">External modifier to the score. Used to checking a range.</param>
        /// <returns>The optimal tuition / yr in dollars.</returns>
        public int CalculateTargetTuition(SimulationScore score, int bonus = 0)
        {
            int tuitionScore = score.AcademicPrestige + score.ResearchPrestige + bonus;

            int minTuitionScore = _config.AcademicPrestige.MinValue +
                                    _config.ResearchPrestige.MinValue;
            int maxTuitionScore = _config.AcademicPrestige.MaxValue +
                                    _config.ResearchPrestige.MaxValue;

            tuitionScore = Utils.Clamp(tuitionScore, minTuitionScore, maxTuitionScore);

            return SimulationUtils.ExponentialMapping(
                value: tuitionScore,
                minInput: minTuitionScore,
                maxInput: maxTuitionScore,
                minOutput: _config.TuitionRange.MinValue,
                maxOutput: _config.TuitionRange.MaxValue,
                exponent: _config.TuitionRangeExponentialFactor);
        }

        /// <summary>
        /// Compare a requested tuition value to the 'optimal' tuition for the university.
        /// Outputs a population size bonus value for calculating the population size of an enrolling class.
        /// </summary>
        /// <param name="tuition">The requested tuition ($/yr).</param>
        /// <param name="targetTuition">The optimal tuition ($/yr).</param>
        /// <returns>The tuition bonus score.</returns>
        private int CalculateTuitionBonus(int tuition, int targetTuition)
        {
            int tuitionDelta = targetTuition - tuition;
            if (tuitionDelta >= 0)
            {
                // Positive tuition bonuses are dampened by a sigmoid to max out the effect.
                return SimulationUtils.SigmoidMapping(
                    value: tuitionDelta,
                    minOutput: _config.TuitionBonus.MinValue,
                    maxOutput: _config.TuitionBonus.MaxValue,
                    sigmoidSlope: _config.TuitionBonusSigmoidFactor);
            }
            else
            {
                // Negative tuition bonuses (some call these 'penalties') are not dampened and DO NOT MAX OUT.
                return (int)Math.Round(tuitionDelta / _config.TuitionBonusLinearFactor);
            }
        }

        /// <summary>
        /// Calculate the population size of students that are interested in enrolling in the university.
        /// Based off popularity plus other bonuses.
        /// </summary>
        /// <param name="score">The current university scores.</param>
        /// <param name="tuitionBonus">The calculated tuition Bonus</param>
        /// <returns>The population size of the enrolling class.</returns>
        private int CalculateEnrollingClassPopulationSize(SimulationScore score, int tuitionBonus)
        {
            int popularityScore = score.Popularity + tuitionBonus;
            popularityScore = Utils.Clamp(
                popularityScore,
                _config.Popularity.MinValue,
                _config.Popularity.MaxValue);

            return SimulationUtils.ExponentialMapping(
                value: popularityScore,
                minInput: _config.Popularity.MinValue,
                maxInput: _config.Popularity.MaxValue,
                minOutput: _config.EnrollingPopulationSize.MinValue,
                maxOutput: _config.EnrollingPopulationSize.MaxValue,
                exponent: _config.EnrollingPopulationSizeExponentialFactor);
        }

        /// <summary>
        /// Converts the campus Academic Prestige into the hypothetical mean value of an enrolling class.
        /// </summary>
        /// <param name="score">The current university scores.</param>
        /// <returns>The mean student academic score of an enrolling class.</returns>
        private int CalculateMeanPopulationAcademicScore(SimulationScore score)
        {
            return SimulationUtils.LinearMapping(
                value: score.AcademicPrestige,
                minInput: _config.AcademicPrestige.MinValue,
                maxInput: _config.AcademicPrestige.MaxValue,
                minOutput: _config.StudentAcademicScore.MinValue,
                maxOutput: _config.StudentAcademicScore.MaxValue);
        }

        /// <summary>
        /// Generates a population of a given size that fits within the requested min and max value.
        /// Note: this means the sum of the population may not equal the requested population size. As some may miss the cutoff.
        /// </summary>
        /// <param name="mean">The mean value of the normal distribution.</param>
        /// <param name="variance">Variance of the normal distribution.</param>
        /// <param name="minHistValue">The minimum value in the histogram to generate</param>
        /// <param name="maxHistValue">The maximum value in the histogram to generate.</param>
        /// <param name="populationSize">The ideal size of the population (if none are sliced off).</param>
        /// <returns>A histogram of the count of students at each value (where 0 = minHistValue)</returns>
        private int[] GeneratePopulation(double mean, double variance, int minHistValue, int maxHistValue, int populationSize)
        {
            int N = maxHistValue - minHistValue + 1;

            int[] population = new int[N];
            for (int i = 0; i < N; ++i)
            {
                double normal = SimulationUtils.NormalFunction(mean, variance, populationSize, minHistValue + i);
                population[i] = (int)Math.Round(normal);
            }

            return population;
        }
    }
}
