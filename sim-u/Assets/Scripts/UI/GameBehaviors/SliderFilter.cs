using Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI Behavior for filters between a min and max value.
    /// This behavior will keep all texts and the slider in sync.
    /// </summary>
    public class SliderFilter : MonoBehaviour
    {
        private int _minValue;
        private int _maxValue;
        private int _value;

        // .NET String format for printing the range texts
        public string MinMaxValueFormat = "{0}";

        // .NET String format for printing the value text
        public string ValueFormat = "{0}";

        public Text PromptText;
        public Text ValueText;
        public Text MinValueText;
        public Text MaxValueText;

        public Slider Slider;

        public int Value => _value;

        public Action<float> OnValueChanged { get; set; }

        /// <summary>
        /// Unity Start method
        /// </summary>
        internal void Start()
        {
            Slider.onValueChanged.AddListener((value) =>
            {
                int valueInt = (int)Math.Round(value);
                SetValue(valueInt, updateSlider: false);

                OnValueChanged?.Invoke(value);
            });
        }

        /// <summary>
        /// Set the slider into the 'recalculating' mode where it can't be used.
        /// Call <see cref="SetRange"/> after going into this mode to re-enable.
        /// </summary>
        public void Recalculating()
        {
            Slider.enabled = false;
            MinValueText.text = "...";
            MaxValueText.text = "...";
        }

        /// <summary>
        /// Set the minimum and maximum range on the slider.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public void SetRange(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                GameLogger.Error("Attempted to set invalid slider range! MinValue: {0} MaxValue: {1}", minValue, maxValue);
                return;
            }

            _minValue = minValue;
            _maxValue = maxValue;

            Slider.enabled = true;
            Slider.minValue = minValue;
            Slider.maxValue = maxValue;

            MinValueText.text = string.Format(MinMaxValueFormat, minValue);
            MaxValueText.text = string.Format(MinMaxValueFormat, maxValue);

            if (_value < _minValue || _value > _maxValue)
            {
                SetValue(Math.Max(_minValue, Math.Min(_maxValue, _value)));
            }
        }

        /// <summary>
        /// Set the value of the slider.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="updateSlider">Should update the slider's position.</param>
        public void SetValue(int value, bool updateSlider = true)
        {
            if (value < _minValue)
                value = _minValue;

            if (value > _maxValue)
                value = _maxValue;

            if (updateSlider)
            {
                // Setting this slider value can invoke a callback that re-invokes this method.
                Slider.value = value;
            }

            _value = value;
            ValueText.text = string.Format(ValueFormat, value);
        }
    }
}
