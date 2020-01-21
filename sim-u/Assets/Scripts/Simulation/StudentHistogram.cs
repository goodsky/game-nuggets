using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation
{
    /// <summary>
    /// Data structure that represents a single 'batch' of students.
    /// Keeps a histogram of the number of students in each score bucket.
    /// </summary>
    [Serializable]
    public class StudentHistogram
    {
        /// <summary>
        /// Note: This field is serialized when saving game state.
        /// </summary>
        private int[] _scoreHistogram { get; set; }

        public int MinValue { get; private set; }

        public int HistogramLength => _scoreHistogram.Length;
        public int TotalStudentCount => _scoreHistogram.Sum();
        public bool HasValues => _scoreHistogram.Any(i => i != 0);

        public StudentHistogram(int[] scoreHistogram, int minValue)
        {
            _scoreHistogram = scoreHistogram ?? throw new ArgumentNullException(nameof(scoreHistogram));
            MinValue = minValue;
        }

        /// <summary>
        /// Gets the sum of all values in the histogram.
        /// </summary>
        public long GetTotalSum()
        {
            long sum = 0;
            for (int i = 0; i < _scoreHistogram.Length; ++i)
            {
                sum += _scoreHistogram[i] * BucketValue(i);
            }

            return sum;
        }

        /// <summary>
        /// Gets the mean value of the histogram.
        /// </summary>
        public int GetMeanValue()
        {
            int count = TotalStudentCount;
            if (count == 0)
            {
                return 0;
            }

            return (int)(GetTotalSum() / count);
        }

        /// <summary>
        /// Gets the range of values represented in the histogram.
        /// </summary>
        /// <returns></returns>
        public (int minScore, int maxScore) GetScoreRange()
        {
            int minScore = MinValue;
            for (int i = 0; i < _scoreHistogram.Length; ++i)
            {
                if (_scoreHistogram[i] > 0)
                {
                    minScore = BucketValue(i);
                    break;
                }
            }

            int maxScore = MinValue + 1;
            for (int i = _scoreHistogram.Length - 1; i >= 0; --i)
            {
                if (_scoreHistogram[i] > 0)
                {
                    maxScore = BucketValue(i);
                    break;
                }
            }

            return (minScore, maxScore);
        }

        /// <summary>
        /// Add the values to this histogram.
        /// </summary>
        /// <param name="other">The other histogram to add to this one.</param>
        public void Add(StudentHistogram other)
        {
            if (other == null)
            {
                return;
            }

            if (MinValue != other.MinValue ||
                HistogramLength != other.HistogramLength)
            {
                throw new InvalidOperationException("Can't add histograms over different ranges!");
            }

            for (int i = 0; i < HistogramLength; ++i)
            {
                _scoreHistogram[i] += other._scoreHistogram[i];
            }
        }

        /// <summary>
        /// Subtract the values from this histogram.
        /// </summary>
        /// <param name="other">The other histogram to subtract from this one.</param>
        public void Subtract(StudentHistogram other)
        {
            if (other == null)
            {
                return;
            }

            if (MinValue != other.MinValue ||
                HistogramLength != other.HistogramLength)
            {
                throw new InvalidOperationException("Can't subtract histograms over different ranges!");
            }

            for (int i = 0; i < HistogramLength; ++i)
            {
                _scoreHistogram[i] -= other._scoreHistogram[i];
            }
        }

        /// <summary>
        /// Shift the entire histogram by the requested value.
        /// Effectively adds a value to each value in the histogram.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void AddToValues(int value)
        {
            if (value == 0)
            {
                return;
            }

            int start;
            int direction;
            if (value > 0)
            {
                start = HistogramLength - 1;
                direction = -1;
            }
            else
            {
                start = 0;
                direction = 1;
            }

            for (int i = start; 0 <= i && i < HistogramLength; i += direction)
            {
                int count = _scoreHistogram[i];
                _scoreHistogram[i] = 0;

                int updateIndex = Utils.Clamp(i + value, 0, HistogramLength - 1);
                _scoreHistogram[updateIndex] += count;
            }
        }

        /// <summary>
        /// Adds a single value into the histogram.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void AddSingleValue(int value)
        {
            if (value < MinValue || value >= MinValue + HistogramLength)
            {
                GameLogger.FatalError("Attempted to add a value to histogram that is out of range! Value: {0}; Range: [{1}, {2}]",
                    value,
                    MinValue,
                    MinValue + HistogramLength);
            }

            _scoreHistogram[value - MinValue] += 1;
        }

        /// <summary>
        /// Returns the inverse histogram.
        /// i.e. this value plus the inverse = 0.
        /// </summary>
        /// <returns></returns>
        public StudentHistogram GetInverse()
        {
            int[] inverse = new int[HistogramLength];
            for (int i = 0; i < HistogramLength; ++i)
            {
                inverse[i] = _scoreHistogram[i] * -1;
            }

            return new StudentHistogram(inverse, MinValue);
        }

        /// <summary>
        /// Take only the top scores that pass a cutoff in the population.
        /// </summary>
        /// <param name="minBound">The minimum (inclusive) value.</param>
        /// <returns>A histogram, minus the cut off population.</returns>
        public StudentHistogram Slice(int minBound)
        {
            int[] splitValues = new int[HistogramLength];
            for (int i = 0; i < HistogramLength; ++i)
            {
                int value = BucketValue(i);
                splitValues[i] = value >= minBound ? _scoreHistogram[i] : 0;
            }

            return new StudentHistogram(splitValues, MinValue);
        }

        /// <summary>
        /// Takes a subset of the histogram.
        /// </summary>
        /// <param name="minBound">The minimum (inclusive) value.</param>
        /// <param name="maxBound">The maximum (exclusive) value.</param>
        /// <returns>A slice of the histogram between the bounds.</returns>
        public StudentHistogram Slice(int minBound, int maxBound)
        {
            int[] splitValues = new int[HistogramLength];
            for (int i = 0; i < HistogramLength; ++i)
            {
                int value = BucketValue(i);
                splitValues[i] = value >= minBound && value < maxBound 
                    ? _scoreHistogram[i] : 0;
            }

            return new StudentHistogram(splitValues, MinValue);
        }

        /// <summary>
        /// Skips N values, then takes the count.
        /// </summary>
        /// <param name="skip">The number of students to skip.</param>
        /// <param name="count">The number of students to take from the population.</param>
        /// <returns>A histogram of the next N students after skipping.</returns>
        public StudentHistogram SkipThenTake(int skip, int count)
        {
            int leftToSkip = skip;
            int leftToTake = count;

            int[] takeValues = new int[HistogramLength];
            for (int i = 0; i < HistogramLength && leftToTake > 0; ++i)
            {
                int takeCount = _scoreHistogram[i];
                if (leftToSkip > 0)
                {
                    if (takeCount <= leftToSkip)
                    {
                        leftToSkip -= takeCount;
                        continue;
                    }

                    takeCount -= leftToSkip;
                    leftToSkip = 0;
                }

                if (takeCount > leftToTake)
                {
                    takeCount = leftToTake;
                }

                takeValues[i] = takeCount;
                leftToTake -= takeCount;
            }

            return new StudentHistogram(takeValues, MinValue);
        }

        /// <summary>
        /// Take only the top N values from the population.
        /// </summary>
        /// <param name="count">The number of students to take from the population.</param>
        /// <returns>A histogram of the top N students from the population.</returns>
        public StudentHistogram TakeTop(int count)
        {
            int leftToTake = count;

            int[] takeValues = new int[HistogramLength];
            for (int i = HistogramLength - 1; i >= 0 && leftToTake > 0; --i)
            {
                int takeCount = _scoreHistogram[i];
                if (takeCount > leftToTake)
                {
                    takeCount = leftToTake;
                }

                takeValues[i] = takeCount;
                leftToTake -= takeCount;
            }

            return new StudentHistogram(takeValues, MinValue);
        }

        /// <summary>
        /// Take only the bottom N values from the population.
        /// </summary>
        /// <param name="count">The number of students to take from the population.</param>
        /// <returns>A histogram of the bottom N students from the population.</returns>
        public StudentHistogram TakeBottom(int count)
        {
            int leftToTake = count;

            int[] takeValues = new int[HistogramLength];
            for (int i = 0; i < HistogramLength && leftToTake > 0; ++i)
            {
                int takeCount = _scoreHistogram[i];
                if (takeCount > leftToTake)
                {
                    takeCount = leftToTake;
                }

                takeValues[i] = takeCount;
                leftToTake -= takeCount;
            }

            return new StudentHistogram(takeValues, MinValue);
        }

        /// <summary>
        /// I wanted to iterate through all the students.
        /// So I did this. Today is not a "good code" day.
        /// NB: I want to iterate from highest value to lowest value,
        /// to give best students first chance at classrooms.
        /// </summary>
        /// <returns>Enumerable to go through all histogram values.</returns>
        public IEnumerable<int> EnumerateStudents()
        {
            for (int i = HistogramLength - 1; i >= 0; --i)
            {
                for (int j = 0; j < _scoreHistogram[i]; ++j)
                {
                    yield return BucketValue(i);
                }
            }
        }

        public override string ToString()
        {
            var buckets = new Dictionary<int, int>();
            for (int i = 0; i < _scoreHistogram.Length; ++i)
            {
                int value = BucketValue(i);
                int bucket = value / 10;
                int histogramValue = _scoreHistogram[i];

                if (buckets.ContainsKey(bucket))
                {
                    histogramValue += buckets[bucket];
                }

                buckets[bucket] = histogramValue;
            }

            return $"[Population Size: {TotalStudentCount}; S: {buckets[10] + buckets[11]} A: {buckets[9]} B: {buckets[8]} C: {buckets[7]} D: {buckets[6]}]";
        }

        private int BucketValue(int index) => MinValue + index;
    }
}
