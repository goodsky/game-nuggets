using Common;
using System;

namespace Simulation
{
    public static class SimulationUtils
    {
        /// <summary>
        /// Calculates the number of a population at a position along the bell-curve, given the mean, variance and size of the population.
        /// </summary>
        /// <param name="mean">The mean value of the population.</param>
        /// <param name="variance">The variance of the population.</param>
        /// <param name="populationSize">The size of the population.</param>
        /// <param name="x">The value to query.</param>
        /// <returns>The number of the population that should be distributed at the point.</returns>
        public static double NormalFunction(double mean, double variance, double populationSize, double x)
        {
            // https://en.wikipedia.org/wiki/Normal_distribution
            return (populationSize / Math.Sqrt(2 * Math.PI * variance)) *
                Math.Pow(Math.E, -Math.Pow(x - mean, 2) / (2 * mean));
        }

        /// <summary>
        /// Maps an input value from the input range to the output range using a linear model.
        /// </summary>
        /// <param name="value">The value within [minInput, maxInput] to map to the output range.</param>
        /// <param name="minInput">Minimum input value.</param>
        /// <param name="maxInput">Maximum input value.</param>
        /// <param name="minOutput">Minimum output value.</param>
        /// <param name="maxOutput">Maximum output value.</param>
        /// <returns>The value, mapped to the output range.</returns>
        public static double LinearMapping(double value, int minInput, int maxInput, int minOutput, int maxOutput)
        {
            if (value < minInput || value > maxInput)
            {
                GameLogger.Warning("Linearly mapping value out of range! [{0}, {1}] Value = {2}", minInput, maxInput, value);
            }

            double inputRange = maxInput - minInput;
            double outputRange = maxOutput - minOutput;
            double scalingFactor = outputRange / inputRange;

            return ((value - minInput) * scalingFactor) + minOutput;
        }

        /// <summary>
        /// Maps an input value from the input range to the output range using an exponential model.
        /// </summary>
        /// <param name="value">The value within [minInput, maxInput] to map to the output range.</param>
        /// <param name="minInput">Minimum input value.</param>
        /// <param name="maxInput">Maximum input value.</param>
        /// <param name="minOutput">Minimum output value.</param>
        /// <param name="maxOutput">Maximum output value.</param>
        /// <param name="exponent">The exponent used in the model. Larger means curvier.</param>
        /// <returns>The value, mapped to the output range.</returns>
        public static double ExponentialMapping(double value, int minInput, int maxInput, int minOutput, int maxOutput, double exponent)
        {
            if (value < minInput || value > maxInput)
            {
                GameLogger.Warning("Exponential mapping value out of range! [{0}, {1}] Value = {2}", minInput, maxInput, value);
            }

            double maxInputValue = Math.Pow(maxInput - minInput, exponent);
            double slope = (maxOutput - minOutput) / maxInputValue;

            return Math.Pow(value - minInput, exponent) * slope + minOutput;
        }

        /// <summary>
        /// Clamps a value within a specific range using a sigmoid model.
        /// </summary>
        /// <param name="value">The value to clamp within the range.</param>
        /// <param name="inputFor90percentile">Input value where 90% of maxOutput will be met.</param>
        /// <param name="maxOutput">Maximum output value.</param>
        /// <returns>The value, mapped to a sigmoid curve between -maxOutput,maxOutput</returns>
        public static double SigmoidMapping(double value, double inputFor90percentile, int maxOutput)
        {
            const double percentile = 0.90;
            double range = maxOutput * 2.0;
            double minValue = -maxOutput;
            double sigmoidSlope = -inputFor90percentile / Math.Log((1 - percentile) / percentile);
            return range / (1 + Math.Exp(-value / sigmoidSlope)) + minValue;
        }
    }
}
