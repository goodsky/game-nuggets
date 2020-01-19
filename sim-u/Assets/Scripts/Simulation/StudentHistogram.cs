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
        private int[] _scoreHistogram { get; set; }

        public int MinValue { get; private set; }
        private int BucketValue(int index) => MinValue + index;
        public int HistogramLength => _scoreHistogram.Length;
        public int TotalStudentCount => _scoreHistogram.Sum();

        public StudentHistogram(int[] scoreHistogram, int minValue)
        {
            _scoreHistogram = scoreHistogram ?? throw new ArgumentNullException(nameof(scoreHistogram));
            MinValue = minValue;
        }

        public int StudentCount(int index) => _scoreHistogram[index];

        public int Mean
        {
            get
            {
                int count = TotalStudentCount;
                if (count == 0)
                {
                    return 0;
                }

                long sum = 0;
                for (int i = 0; i < _scoreHistogram.Length; ++i)
                {
                    sum += _scoreHistogram[i] * BucketValue(i);
                }

                return (int)(sum / count);
            }
        }

        public (int minScore, int maxScore) GetScoreRange()
        {
            int minScore = MinValue;
            for (int i = 0; i < _scoreHistogram.Length; ++i)
            {
                if (_scoreHistogram[i] > 0)
                {
                    minScore = MinValue + i;
                    break;
                }
            }

            int maxScore = MinValue + 1;
            for (int i = _scoreHistogram.Length - 1; i >= 0; --i)
            {
                if (_scoreHistogram[i] > 0)
                {
                    maxScore = MinValue + i;
                    break;
                }
            }

            return (minScore, maxScore);
        }

        /// <summary>
        /// Merge two histograms together. Sum all parts.
        /// </summary>
        /// <param name="other">The other histogram to merge with this one.</param>
        /// <returns>A merge histogram.</returns>
        public StudentHistogram Merge(StudentHistogram other)
        {
            if (other == null)
            {
                return this;
            }

            if (MinValue != other.MinValue ||
                HistogramLength != other.HistogramLength)
            {
                throw new InvalidOperationException("Can't merge histograms over different ranges!");
            }

            int[] mergedValues = new int[HistogramLength];
            for (int i = 0; i < HistogramLength; ++i)
            {
                mergedValues[i] = StudentCount(i) + other.StudentCount(i);
            }

            return new StudentHistogram(mergedValues, MinValue);
        }

        /// <summary>
        /// Take only the top scores that pass a cutoff in the population.
        /// </summary>
        /// <param name="minCutoff">The minimum (inclusive) value.</param>
        /// <returns>A histogram, minus the cut off population.</returns>
        public StudentHistogram Split(int minCutoff)
        {
            int[] splitValues = new int[HistogramLength];
            for (int i = 0; i < HistogramLength; ++i)
            {
                int value = MinValue + i;
                splitValues[i] = value >= minCutoff ? _scoreHistogram[i] : 0;
            }

            return new StudentHistogram(splitValues, MinValue);
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
    }
}
